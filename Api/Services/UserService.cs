using Api.Dtos;
using Api.Helpers;
using Core.Authorization;
using Core.Entities;
using Core.Interfaces;
using Core.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace Api.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IRefreshTokenHasher _tokenHasher;

    public UserService(IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettingsOptions, IPasswordHasher<User> passwordHasher, IRefreshTokenHasher tokenHasher)
    {
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettingsOptions.Value;
        _passwordHasher = passwordHasher;
        _tokenHasher = tokenHasher;
    }

    public async Task<RegisterResult> RegisterAsync(UserRegisterDto userRegisterDto)
    {
        var userExists = await _unitOfWork.Users
            .GetByUsernameAsync(userRegisterDto.Username);

        if (userExists is not null)
        {
            return new RegisterResult(
                false,
                RegisterError.UsernameAlreadyExists);
        }

        var defaultRole = _unitOfWork.Roles
            .Find(r => r.Name == AppRoles.DefaultRole.ToString())
            .FirstOrDefault();

        if (defaultRole is null)
        {
            return new RegisterResult(
                false,
                RegisterError.DefaultRoleNotFound);
        }

        var user = new User 
        {
            Name = userRegisterDto.Name,
            LastName = userRegisterDto.LastName,
            Username = userRegisterDto.Username,
            Email = userRegisterDto.Email,
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, userRegisterDto.Password);

        var userRole = new UserRoles 
        {
            User = user,
            RoleId = defaultRole.Id,
        };
        user.UserRoles.Add(userRole);
        _unitOfWork.Users.Add(user);

        try
        {
            var affectedRows = await _unitOfWork.SaveAsync();
            if (affectedRows <= 0)
            {
                return new RegisterResult(
                    false,
                    RegisterError.PersistenceError);
            }

            return new RegisterResult(
                true,
                RegisterError.None);
        }
        catch (DbUpdateException ex)
            when (IsUniqueConstraintViolation(ex))
        {
            return new RegisterResult(
                false,
                RegisterError.UsernameAlreadyExists);
        }
    }

    public async Task<LoginResponseDto?> LoginAsync(UserLoginDto userLoginDto)
    {
        var user = await _unitOfWork.Users
            .GetByUsernameAsync(userLoginDto.Username);
        if (user is null)
            return null;

        var verifyPassword = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLoginDto.Password);
        if (verifyPassword == PasswordVerificationResult.Failed)
            return null;

        var (refreshToken, plainToken) = CreateRefreshToken(user.Id);

        var loginResponse = await BuildAccessToken(user);
        loginResponse.RefreshToken = plainToken;
        loginResponse.RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc;

        _unitOfWork.RefreshTokens.Add(refreshToken);
        await _unitOfWork.SaveAsync();
        
        return loginResponse;
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(string cookieToken)
    {
        var tokenHash = _tokenHasher.ComputeHash(cookieToken);
        var refreshTokenDb = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash);
        if (refreshTokenDb is null || refreshTokenDb.RevokedAtUtc is not null)
            return null;

        refreshTokenDb.RevokedAtUtc = DateTime.UtcNow;
        if (refreshTokenDb.IsExpired)
        {
            await _unitOfWork.SaveAsync();
            return null;
        }

        var (newRefreshToken, plainToken) = CreateRefreshToken(refreshTokenDb.User.Id);

        var loginResponse = await BuildAccessToken(refreshTokenDb.User);
        loginResponse.RefreshToken = plainToken;
        loginResponse.RefreshTokenExpiresAtUtc = newRefreshToken.ExpiresAtUtc;

        _unitOfWork.RefreshTokens.Add(newRefreshToken);
        await _unitOfWork.SaveAsync();

        return loginResponse;
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var tokenHash = _tokenHasher.ComputeHash(refreshToken);
        var refreshTokenDb = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash);
        if (refreshTokenDb is null || refreshTokenDb.RevokedAtUtc is not null)
            return;

        refreshTokenDb.RevokedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();
    }


    private async Task<LoginResponseDto> BuildAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            // Id del usuario
            new Claim(JwtClaimNames.Sub, user.Id.ToString()),
    
            // Datos de identidad
            new Claim(JwtClaimNames.Name, user.Username),
            new Claim(JwtClaimNames.Email, user.Email),
            new Claim(JwtClaimNames.Jti, Guid.NewGuid().ToString()), // ID del token
        };

        claims.AddRange(
            user.Roles.Select(role =>
                new Claim(ClaimTypes.Role, role.Name)
            )
        );

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = credentials
        };

        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new LoginResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiration
        };
    }

    private static bool IsUniqueConstraintViolation(
        DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException sqlException
            && sqlException.Number is 2601 or 2627;
    }

    private (RefreshToken refreshToken, string plainToken) CreateRefreshToken(int userId)
    {
        var randomNumber = new byte[32];
        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomNumber);
            var utcNow = DateTime.UtcNow;
            var token = Convert.ToBase64String(randomNumber);

            return (new RefreshToken
            {
                UserId = userId,
                TokenHash = _tokenHasher.ComputeHash(token),
                CreatedAtUtc = utcNow,
                ExpiresAtUtc = utcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
            }, token);
        }
    }
}

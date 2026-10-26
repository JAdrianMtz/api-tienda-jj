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

    public UserService(IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettingsOptions, IPasswordHasher<User> passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettingsOptions.Value;
        _passwordHasher = passwordHasher;
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
        user.Password = _passwordHasher.HashPassword(user, userRegisterDto.Password);

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

        var verifyPassword = _passwordHasher.VerifyHashedPassword(user, user.Password, userLoginDto.Password);
        if (verifyPassword == PasswordVerificationResult.Failed)
            return null;

        var loginResponse = await BuildToken(user);
        var refreshToken = CreateRefreshToken();
        loginResponse.RefreshToken = refreshToken.TokenHash;
        loginResponse.RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc;

        // TODO: Aplicar hash al RefreshToken antes de guardar en la BD

        user.RefreshTokens.Add(refreshToken);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();
        
        return loginResponse;
    }

    // TODO: Agregar acción para refrescar token cuando el JWT ha expirado

    // TODO: Implementar el cierre de sesión y la revocación del refresh token.

    

    private async Task<LoginResponseDto> BuildToken(User user)
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
        var token = new JsonWebTokenHandler().CreateToken(tokenDescriptor);

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

    private RefreshToken CreateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomNumber);
            var utcNow = DateTime.UtcNow;
            return new RefreshToken
            {
                TokenHash = Convert.ToBase64String(randomNumber),
                CreatedAtUtc = utcNow,
                ExpiresAtUtc = utcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
            };
        }
    }

    // TODO: Agregar función para aplicar hash al RefreshToken
}

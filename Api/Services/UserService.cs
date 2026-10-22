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
            .Find(r => r.Name == nameof(AppRoles.DefaultRole))
            .FirstOrDefault();

        if (defaultRole is null)
        {
            return new RegisterResult(
                false,
                RegisterError.DefaultRoleNotFound);
        }

        var user = new User {
            Name = userRegisterDto.Name,
            LastName = userRegisterDto.LastName,
            Username = userRegisterDto.Username,
            Email = userRegisterDto.Email,
        };
        user.Password = _passwordHasher.HashPassword(user, userRegisterDto.Password);

        var userRole = new UserRoles {
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

    private static bool IsUniqueConstraintViolation(
        DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException sqlException
            && sqlException.Number is 2601 or 2627;
    }
}

using Api.Dtos;
using Core.Results;

namespace Api.Services;

public interface IUserService
{
    Task<RegisterResult> RegisterAsync(UserRegisterDto userRegisterDto);
    Task<LoginResponseDto?> LoginAsync(UserLoginDto userLoginDto);
    Task<LoginResponseDto?> RefreshTokenAsync(string cookieToken);
    Task LogoutAsync(string cookieToken);
}

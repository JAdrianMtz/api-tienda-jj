using Api.Dtos;
using Api.Services;
using Core.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> RegisterAsync(UserRegisterDto userRegisterDto)
    {
        if (userRegisterDto is null)
        {
            return BadRequest();
        }

        var result = await _userService.RegisterAsync(userRegisterDto);
        if (result.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                new { message = "Usuario registrado correctamente" });
        }

        return result.Error switch
        {
            RegisterError.UsernameAlreadyExists =>
                Conflict(new { message = "Usuario no disponible" }),

            RegisterError.DefaultRoleNotFound =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error de configuración",
                    detail: "No se pudo completar el registro."),

            RegisterError.PersistenceError =>
                Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error al registrar el usuario"),

            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> LoginAsync(UserLoginDto userLoginDto) 
    { 
        if (userLoginDto is null)
        {
            return BadRequest();
        }

        var loginResult = await _userService.LoginAsync(userLoginDto);
        if (loginResult is null)
        {
            return Unauthorized(new { message = "Credenciales incorretas" });
        }

        SetRefreshTokenInCookie(loginResult.RefreshToken, loginResult.RefreshTokenExpiresAtUtc);
        return loginResult;
    }

    // TODO: Agregar acción para refrescar token cuando el JWT ha expirado

    // TODO: Implementar el cierre de sesión y la revocación del refresh token.

    private void SetRefreshTokenInCookie(string refreshToken, DateTime expirationDate) 
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expirationDate
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}

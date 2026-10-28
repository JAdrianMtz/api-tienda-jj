using Api.Dtos;
using Api.Services;
using Core.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _environment;

    public UsersController(IUserService userService, IWebHostEnvironment environment)
    {
        _userService = userService;
        _environment = environment;
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

    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> RefreshTokenAsync()
    {
        if(!Request.Cookies.TryGetValue("refreshToken", out var cookieToken))
        {
            return Unauthorized();
        }

        var refreshTokenResponse = await _userService.RefreshTokenAsync(cookieToken);
        if (refreshTokenResponse is null)
        {
            return Unauthorized();
        }

        SetRefreshTokenInCookie(refreshTokenResponse.RefreshToken, refreshTokenResponse.RefreshTokenExpiresAtUtc);
        return refreshTokenResponse;
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> LogoutAsync()
    {
        if (Request.Cookies.TryGetValue("refreshToken", out var cookieToken))
        {
            await _userService.LogoutAsync(cookieToken);
        }

        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            Path = "/"
        });
        
        return NoContent();
    }

    private void SetRefreshTokenInCookie(string refreshToken, DateTime expirationDate) 
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = expirationDate
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}

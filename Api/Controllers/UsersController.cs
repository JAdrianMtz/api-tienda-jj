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
    public async Task<ActionResult> RegisterAsync(UserRegisterDto userRegisterDto)
    {
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
}

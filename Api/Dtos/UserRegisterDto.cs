using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public class UserRegisterDto
{
    [Required(ErrorMessage = "El campo {0} es requerido")]
    [StringLength(200, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "El campo {0} es requerido")]
    [StringLength(200, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "El campo {0} es requerido")]
    [StringLength(200, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
    [RegularExpression(
        @"^[a-zA-Z0-9._-]+$",
        ErrorMessage = "El campo {0} solo puede contener letras sin acentos, números, puntos, guiones y guiones bajos.")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "El campo {0} es requerido")]
    [StringLength(200, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
    [EmailAddress]
    public required string Email { get; set; }

    [Required(ErrorMessage = "El campo {0} es requerido")]
    public required string Password { get; set; }
}

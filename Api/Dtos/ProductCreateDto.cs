using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public class ProductCreateDto
{
    [Required(ErrorMessage = "El campo {0} es requerido")]
    [StringLength(300, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "El campo {0} es requerido")]
    [Range(0, double.MaxValue)]
    public required decimal Price { get; set; }

    [Required(ErrorMessage = "El campo {0} es requerido")]
    [Range(1, int.MaxValue)]
    public int BrandId { get; set; }

    [Required(ErrorMessage = "El campo {0} es requerido")]
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

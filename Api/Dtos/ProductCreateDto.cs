using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public class ProductCreateDto
{
    [Required(ErrorMessage = "El campo {0} es requerido")]
    [StringLength(300, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
    public required string Name { get; set; }

    [Range(0, double.MaxValue)]
    public required decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int BrandId { get; set; }
    
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

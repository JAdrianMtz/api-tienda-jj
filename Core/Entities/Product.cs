using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class Product
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El campo {0} es requerido")]
    [StringLength(300, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
    public required string Name { get; set; }
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }

}

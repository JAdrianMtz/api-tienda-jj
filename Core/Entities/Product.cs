using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class Product : BaseEntity
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public int BrandId { get; set; }
    public Brand? Brand { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}

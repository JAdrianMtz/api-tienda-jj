using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Brand
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<Product> Products { get; set; } = [];
}

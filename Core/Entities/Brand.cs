using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Brand : BaseEntity
{
    public required string Name { get; set; }
    public List<Product> Products { get; set; } = [];
}

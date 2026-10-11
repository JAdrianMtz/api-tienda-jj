using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }
    public List<Product> Products { get; set; } = [];
}

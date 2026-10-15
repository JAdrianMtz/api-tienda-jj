namespace Api.Dtos;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public BrandDto Brand { get; set; } = null!;
    public CategoryDto Category { get; set; } = null!;
}

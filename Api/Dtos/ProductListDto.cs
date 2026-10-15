namespace Api.Dtos;

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
} 

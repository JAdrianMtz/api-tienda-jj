namespace Api.Dtos;

public class ProductCreateDto
{
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
}

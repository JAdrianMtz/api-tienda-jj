using Api.Dtos;
using Asp.Versioning;
using AutoMapper;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("1.1")]
// [Route("api/v{version:apiVersion}/[controller]")] // Usado con options.ApiVersionReader = new UrlSegmentApiVersionReader();
public class ProductsDataController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductsDataController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProducts()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        var productsDto = _mapper.Map<IEnumerable<ProductListDto>>(products);
        return Ok(productsDto);
    }

    [ApiVersion("1.1")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsWithNavigation()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        var productsDto = _mapper.Map<IEnumerable<ProductDto>>(products);
        return Ok(productsDto);
    }
}

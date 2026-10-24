using Api.Dtos;
using Api.Dtos.Common;
using Asp.Versioning;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Administrador,Empleado")]
    public class ProductsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResponse<ProductListDto>>> GetProducts([FromQuery] PaginationDTO paginationDTO)
        {
            var (total, products) = await _unitOfWork.Products.GetAllAsync(paginationDTO.PageNumber, paginationDTO.PageSize);
            var productsDto = _mapper.Map<IEnumerable<ProductListDto>>(products);

            var pagedResponse = new PagedResponse<ProductListDto> { 
                PageNumber = paginationDTO.PageNumber,
                PageSize = paginationDTO.PageSize,
                TotalCount = total,
                Data = productsDto
            };
            return Ok(pagedResponse);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductListDto>> CreateProduct(ProductCreateDto productCreateDto)
        {
            if (productCreateDto is null)
            {
                return BadRequest();
            }

            var product = _mapper.Map<Product>(productCreateDto);
            _unitOfWork.Products.Add(product);
            await _unitOfWork.SaveAsync();
            var productResponse = _mapper.Map<ProductListDto>(product);

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, productResponse);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, ProductUpdateDto productUpdateDto)
        {
            if (productUpdateDto is null)
            {
                return BadRequest();
            }

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            _mapper.Map(productUpdateDto, product);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}

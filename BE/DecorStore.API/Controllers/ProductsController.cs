using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DecorStore.API.Models;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // GET: api/Products/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsByCategory(int categoryId)
        {
            var filter = new ProductFilterDTO { CategoryId = categoryId };
            var products = await _productService.GetAllAsync(filter);
            var productDtos = _productService.MapToProductDTOs(products);
            return Ok(productDtos);
        }

        // POST: api/Products
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only admin can create products
        public async Task<ActionResult<ProductDTO>> CreateProduct(CreateProductDTO productDto)
        {
            var createdProduct = await _productService.CreateAsync(productDto);
            var productDtoResult = await _productService.GetProductByIdAsync(createdProduct.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, productDtoResult);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only admin can update products
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDTO productDto)
        {
            try
            {
                await _productService.UpdateAsync(id, productDto);
                return NoContent();
            }
            catch (DecorStore.API.Exceptions.NotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only admin can delete products
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            return NoContent();
        }

        // POST: api/Products/{id}/images
        [HttpPost("{id}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddImageToProduct(int id, IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest("No image file provided");
                }

                var result = await _productService.AddImageToProductAsync(id, image);
                return Ok(new { message = "Image added successfully" });
            }
            catch (DecorStore.API.Exceptions.NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Products/{productId}/images/{imageId}
        [HttpDelete("{productId}/images/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveImageFromProduct(int productId, int imageId)
        {
            try
            {
                var result = await _productService.RemoveImageFromProductAsync(productId, imageId);
                return Ok(new { message = "Image removed successfully" });
            }
            catch (DecorStore.API.Exceptions.NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
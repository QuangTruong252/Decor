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
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(int categoryId)
        {
            var filter = new ProductFilterDTO { CategoryId = categoryId };
            var products = await _productService.GetAllAsync(filter);
            return Ok(products);
        }

        // POST: api/Products
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only admin can create products
        public async Task<ActionResult<Product>> CreateProduct(CreateProductDTO productDto)
        {
            var createdProduct = await _productService.CreateAsync(productDto);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
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
    }
} 
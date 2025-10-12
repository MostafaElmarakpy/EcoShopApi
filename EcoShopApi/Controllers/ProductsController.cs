using EcoShopApi.Application.Common.DTO.UserDTO;
using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace EcoShopApi.Controllers
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


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var products = await _productService.GetProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] ProductCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(FormatModelErrors(ModelState));
            var product = new Product
            {
                Name = dto.Name,
                ProductCode = dto.ProductCode,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                // ImagePath will be handled by service (could be list of URLs)
            };
            _productService.CreateProductAsync(product, dto.Files);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);

        }

        [HttpPut("{id}")]
        //[Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] ProductUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(FormatModelErrors(ModelState));
            if (id != dto.Id) return BadRequest(new { errors = new { id = new[] { "Id mismatch" } } });

            var existingProduct = await _productService.GetProductByIdAsync(id);
            if (existingProduct == null) return NotFound();

            existingProduct.Name = dto.Name;
            existingProduct.Price = dto.Price;
            //existingProduct.Category = dto.Category;
            existingProduct.ProductCode = dto.ProductCode;
            //existingProduct.MinimumQuantity = dto.MinimumQuantity;
            existingProduct.CategoryId = dto.CategoryId;
            // Handle image update logic in service
            if (dto.ExistingImages != null)
            {
                existingProduct.ImagePath = dto.ExistingImages;
            }

            _productService.UpdateProductAsync(existingProduct);

            return NoContent();
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = _productService.GetProductByIdAsync(id).Result;
            if (product == null)
            {
                return NotFound();
            }
            _productService.DeleteProductAsync(id);
            return NoContent();
        }


        private static object FormatModelErrors(ModelStateDictionary ms)
        {
            var errors = ms
                .Where(kvp => kvp.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key.Replace("dto.", ""), // optional cleanup
                    kvp => kvp.Value.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? e.Exception?.Message ?? "Invalid" : e.ErrorMessage).ToArray()
                );
            return new { errors };
        }
    }

}

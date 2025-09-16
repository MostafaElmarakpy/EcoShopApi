using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoShopApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
      
        // private readonly IProductService _productService;

        //public ProductController(IProductService productService) => _productService = productService;

        //[HttpPost]
        //public async Task<IActionResult> Create(ProductCreateDto dto)
        //{
        //    // Validate unique code, save image to wwwroot/images, store path
        //    var product = await _productService.CreateAsync(dto);
        //    return CreatedAtAction(nameof(GetByCode), new { code = product.ProductCode }, product);
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetAll() => Ok(await _productService.GetAllAsync());

        //[HttpGet("{code}")]
        //public async Task<IActionResult> GetByCode(string code) => Ok(await _productService.GetByCodeAsync(code));

        //[HttpPut("{code}")]
        //public async Task<IActionResult> Update(string code, ProductUpdateDto dto)
        //{
        //    await _productService.UpdateAsync(code, dto);
        //    return NoContent();
        //}

        //[HttpDelete("{code}")]
        //public async Task<IActionResult> Delete(string code)
        //{
        //    await _productService.DeleteAsync(code);
        //    return NoContent();
        //}
    }
}
    

namespace BackEnd.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using BackEnd.Data;
    using BackEnd.Model;    
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Threading.Tasks;
   
    [Route("api/product-discounts")]
    [ApiController]
    public class ProductDiscountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductDiscountController> _logger;

        public ProductDiscountController(AppDbContext context, ILogger<ProductDiscountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("list-discount")]
        public async Task<ActionResult<IEnumerable<ProductDiscount>>> GetProductDiscounts()
        {
            return await _context.ProductDiscounts.ToListAsync();
        }



        [HttpGet("Discount/{id}")]
        public async Task<ActionResult<ProductDiscount>> GetProductDiscount( int id ){
            var ProductDiscount = await _context.ProductDiscounts.FindAsync(id);

            if (ProductDiscount == null){
                _logger.LogWarning("Product Discount  with id ",id, "not found");
                return NotFound();
            }

            return ProductDiscount;


        }
        [HttpPost("/Add-discount")]
        public async Task<ActionResult<ProductDiscount>> PostProductDiscount(ProductDiscount ProductDiscount) {
             var lastDiscount = await _context.ProductDiscounts.OrderByDescending(pc => pc.Id).FirstOrDefaultAsync();
            ProductDiscount.Id = (lastDiscount?.Id ?? 0) + 1;
            ProductDiscount.created_at = DateTime.UtcNow;
            ProductDiscount.modified_at = DateTime.MinValue;
            ProductDiscount.deleted_at = null;
           _context.ProductDiscounts.Add(ProductDiscount);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created new product discount with id {Id} at {CreatedAt}", ProductDiscount.Id, ProductDiscount.created_at);
            return CreatedAtAction(nameof(GetProductDiscount), new { id = ProductDiscount.Id }, ProductDiscount);


        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProductDiscount(int id)
        {
            var ProductDiscount = await _context.ProductDiscounts.FindAsync(id);
            if (ProductDiscount == null)
            {
                return NotFound();
            }

            // Soft delete by setting the deleted_at timestamp
              _context.ProductDiscounts.Remove(ProductDiscount);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted product discount  with id {Id}", id);
            return NoContent();
        }



        
    }

     
}   
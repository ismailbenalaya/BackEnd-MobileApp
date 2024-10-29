namespace BackEnd.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using BackEnd.Data;
    using BackEnd.Model;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    [Route("api/product-categories")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductCategoryController> _logger;

        public ProductCategoryController(AppDbContext context, ILogger<ProductCategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<ProductCategory>>> GetProductCategories()
        {
            return await _context.ProductCategories.ToListAsync();
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<ProductCategory>> GetProductCategory(int id)
        {
            var productCategory = await _context.ProductCategories.AsNoTracking().FirstOrDefaultAsync(pc => pc.Id == id);

            if (productCategory == null)
            {
                _logger.LogWarning("Product category with id {Id} not found", id);
                return NotFound();
            }

            return productCategory;
        }

        [HttpPost("create")]
        public async Task<ActionResult<ProductCategory>> PostProductCategory(ProductCategory productCategory)
        {
            var lastCategory = await _context.ProductCategories.OrderByDescending(pc => pc.Id).FirstOrDefaultAsync();
            productCategory.Id = (lastCategory?.Id ?? 0) + 1;
            productCategory.created_at = DateTime.UtcNow;
            productCategory.modified_at = DateTime.MinValue;
            productCategory.deleted_at = null;

            _context.ProductCategories.Add(productCategory);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new product category with id {Id} at {CreatedAt}", productCategory.Id, productCategory.created_at);
            return CreatedAtAction(nameof(GetProductCategory), new { id = productCategory.Id }, productCategory);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutProductCategory(int id, ProductCategory productCategory)
        {
            if (id != productCategory.Id)
            {
                return BadRequest("Id mismatch");
            }

            var existingCategory = await _context.ProductCategories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            // Check if the category has been soft-deleted
            if (existingCategory.deleted_at != null)
            {
                return BadRequest("Cannot update a deleted category");
            }

            existingCategory.Name = productCategory.Name;
            existingCategory.Desc = productCategory.Desc;
            existingCategory.modified_at = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrency exception occurred while updating product category {Id}", id);
                if (!ProductCategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _logger.LogInformation("Updated product category with id {Id}", id);
            return NoContent();
        }

        private bool ProductCategoryExists(int id)
        {
            return _context.ProductCategories.Any(e => e.Id == id);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProductCategory(int id)
        {
            var productCategory = await _context.ProductCategories.FindAsync(id);
            if (productCategory == null)
            {
                return NotFound();
            }

            // Soft delete by setting the deleted_at timestamp
            _context.ProductCategories.Remove(productCategory);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted product category with id {Id}", id);
            return NoContent();
        }
    }
}

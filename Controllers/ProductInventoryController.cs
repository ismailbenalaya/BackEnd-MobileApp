namespace BackEnd.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using BackEnd.Data;
    using BackEnd.Model;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    [Route("api/product-inventories")]
    [ApiController]
    public class ProductInventoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductInventoryController> _logger;

        public ProductInventoryController(AppDbContext context, ILogger<ProductInventoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("inventory-list")]
        public async Task<ActionResult<IEnumerable<ProductInventory>>> GetProductInventories()
        {
            return await _context.ProductInventories.ToListAsync();
        }

        [HttpGet("inventory-details/{id}")]
        public async Task<ActionResult<ProductInventory>> GetProductInventory(int id)
        {
            var productInventory = await _context.ProductInventories.FindAsync(id);
            if (productInventory == null)
           
            {
                _logger.LogWarning("Product inventory with id {Id} not found", id);
                return NotFound();
            }
            return productInventory;
        }

        [HttpPost("create-inventory")]
        public async Task<ActionResult<ProductInventory>> CreateProductInventory(ProductInventory productInventory)
        {
            var lastInventory = await _context.ProductInventories.OrderByDescending(pi => pi.Id).FirstOrDefaultAsync();
            productInventory.Id = (lastInventory?.Id ?? 0) + 1;
            productInventory.created_at = DateTime.UtcNow;
            productInventory.modified_at = DateTime.MinValue;
            productInventory.delete_at = null;

            _context.ProductInventories.Add(productInventory);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new product inventory with id {Id} at {CreatedAt}", productInventory.Id, productInventory.created_at);
            return CreatedAtAction(nameof(GetProductInventory), new { id = productInventory.Id }, productInventory);
        }
        [HttpDelete("delete-inventory/{id}")]
         public async Task<IActionResult> DeleteProductInventory(int id)
        {
        var productInventory = await _context.ProductInventories.FindAsync(id);
        if (productInventory == null)
        {
            _logger.LogWarning("Product inventory with id {Id} not found", id);
            return NotFound();
        }
       _context.ProductInventories.Remove(productInventory);
       
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted product inventory with id {Id} at {DeletedAt}", id, DateTime.UtcNow);
        return NoContent();
    }
    [HttpPut("update-inventory/{id}")]
    public async Task<IActionResult> UpdateProductInventory(int id, ProductInventory productInventory)
    {
        if (id != productInventory.Id)
        {
            return BadRequest();
        }
        productInventory.modified_at = DateTime.UtcNow;
        _context.Entry(productInventory).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated product inventory with id {Id} at {ModifiedAt}", id, productInventory.modified_at);
        return NoContent();
    }

   
}
}

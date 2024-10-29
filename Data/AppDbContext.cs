using BackEnd.Model;
using Microsoft.EntityFrameworkCore;
namespace BackEnd.Data
{
    public class AppDbContext: DbContext
    {
    
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Map ProductCategory to the database
        public DbSet<ProductCategory> ProductCategories { get; set; }
        // Map ProductInventory to the database
        public DbSet<ProductInventory> ProductInventories { get; set; }
        // Map ProductDiscount to the database


        public DbSet<ProductDiscount> ProductDiscounts { get; set; }
        
        
        public DbSet<User> Users { get; set; } //Map Product to the database 
    
        // New DbSets for roles
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed initial roles
            modelBuilder.Entity<Role>().HasData(
                new Role 
                { 
                    Id = 1,
                    Name = "Visitor",
                    
                    created_at = DateTime.UtcNow,
                    modified_at = DateTime.UtcNow
                },
                new Role 
                { 
                    Id = 2,
                    Name = "Administrator",
                    
                    created_at = DateTime.UtcNow,
                    modified_at = DateTime.UtcNow
                }
            );
        }
    }
}

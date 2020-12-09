using Microsoft.EntityFrameworkCore;

namespace test.Models
{
    public class CatalogContext: DbContext
    {
        public DbSet<CatalogItem> Catalog { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<Product> Products { get; set; }
        public CatalogContext()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        public CatalogContext(DbContextOptions<CatalogContext> options) 
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}

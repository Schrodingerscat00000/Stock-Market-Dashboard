using Microsoft.EntityFrameworkCore;

public class StockDbContext : DbContext
{
    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options) {}

    public DbSet<StockData> StockData { get; set; }
}

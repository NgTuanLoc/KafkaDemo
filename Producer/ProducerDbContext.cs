using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary;
using SharedLibrary.TransactionalOutbox;
namespace Producer;

public class ProducerDbContext(DbContextOptions<ProducerDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("EventStreaming");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductTypeConfiguration).Assembly);
        modelBuilder.UseTransactionalOutbox();
    }
}

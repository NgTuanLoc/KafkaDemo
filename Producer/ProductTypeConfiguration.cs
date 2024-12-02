using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SharedLibrary;

namespace Producer;
public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("Product");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
                .HasColumnType("uuid")
                .ValueGeneratedNever()
                .IsRequired();

        builder.HasIndex(o => o.Name)
                .HasDatabaseName("Idx_Name_Ascending");

        builder.Property(o => o.Name)
                .IsRequired();
    }
}


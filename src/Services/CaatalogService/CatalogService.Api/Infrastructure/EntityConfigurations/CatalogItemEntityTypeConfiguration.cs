using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Api.Infrastructure.EntityConfigurations
{
    public class CatalogItemEntityTypeConfiguration : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.ToTable("Catalog", CatalogContext.DEFAULT_SCHEMA);

            builder.Property(ci => ci.Id)
                      .UseHiLo("catalog_hilo")
                        .IsRequired(true);

            builder.Property(cb => cb.Name)
                        .IsRequired(true)
                           .HasMaxLength(50);

            builder.Property(cb => cb.Price)
                       .IsRequired(true);
            
            builder.Property(cb => cb.PictureFileName)
                       .IsRequired(false);

            builder.Ignore(cb => cb.PictureUri);

            builder.HasOne(c => c.CatalogBrand)
                     .WithMany()
                       .HasForeignKey(c => c.CatalogBrandId);

            builder.HasOne(c => c.CatalogType)
                     .WithMany()
                        .HasForeignKey(c => c.CatalogTypeId);
        }
    }
}

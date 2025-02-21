using CatalogService.Api.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContext : DbContext
    {
        public const string DEFAULT_SCHEMA = "catalog";

        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options) 
        {
        }

        public DbSet<CatalogBrand> CatalogBrands {  get; set; }
        public DbSet<CatalogItem> CatalogItems {  get; set; }
        public DbSet<CatalogType> CatalogTypes {  get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


        }
    }
}

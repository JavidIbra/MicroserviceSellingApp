using CatalogService.Api.Core.Domain;
using Microsoft.Data.SqlClient;
using Polly;
using System.Globalization;
using System.IO.Compression;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContextSeed
    {
        public async Task SeedAsync(CatalogContext context, IWebHostEnvironment env, ILogger<CatalogContextSeed> logger)
        {
            var policy = Policy.Handle<SqlException>().WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timeSpan, retry, ctx) =>
                {
                    logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {}");
                });

            var setupDirPath = Path.Combine(env.ContentRootPath, "Infrastructure", "Setup", "SeedFiles");
            var picturePath = "Pics";

            await policy.ExecuteAsync(() => ProcessSeeding(context, setupDirPath, picturePath, logger));

        }

        private async Task ProcessSeeding(CatalogContext context, string setupDirPath, string picturePath, ILogger logger)
        {
            if (!context.CatalogBrands.Any())
            {
                await context.CatalogBrands.AddRangeAsync(GetCatalogBrandsFromFile(setupDirPath));
                await context.SaveChangesAsync();
            }

            if (!context.CatalogTypes.Any())
            {
                await context.CatalogTypes.AddRangeAsync(GetCatalogTypesFromFile(setupDirPath));
                await context.SaveChangesAsync();
            }

            if (!context.CatalogItems.Any())
            {
                await context.CatalogItems.AddRangeAsync(GetCatalogItemsFromFile(setupDirPath, context));
                await context.SaveChangesAsync();

                GetCatalogItemPictures(setupDirPath, picturePath);
            }
        }

        private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentPath)
        {
            IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrands()
            {
                return
                [
                    new() {Brand ="Azure"},
                    new() {Brand =".Net"},
                    new() {Brand ="Visual Studio"},
                    new() {Brand ="SQL Server"},
                    new() {Brand ="Other"},
                ];
            }

            string fileName = Path.Combine(contentPath, "BrandsTextFile.txt");

            if (File.Exists(fileName))
                return GetPreconfiguredCatalogBrands();

            var fileContent = File.ReadAllLines(fileName);

            var list = fileContent.Select(i => new CatalogBrand()
            {
                Brand = i.Trim('"'),
            }).Where(i => i != null);

            return list ?? GetPreconfiguredCatalogBrands();
        }

        private IEnumerable<CatalogType> GetCatalogTypesFromFile(string contentPath)
        {
            static IEnumerable<CatalogType> GetPreconfiguredCatalogTypes()
            {
                return
                [
                    new() {Type ="Mug"},
                    new() {Type ="T-Shirt"},
                    new() {Type ="Sheet"},
                    new() {Type ="USB Memory Stick"},
                ];
            }

            string fileName = Path.Combine(contentPath, "CatalogTypes.txt");

            if (File.Exists(fileName))
                return GetPreconfiguredCatalogTypes();

            var fileContent = File.ReadAllLines(fileName);

            var list = fileContent.Select(i => new CatalogType()
            {
                Type = i.Trim('"'),
            }).Where(i => i != null);

            return list ?? GetPreconfiguredCatalogTypes();
        }

        private IEnumerable<CatalogItem> GetCatalogItemsFromFile(string contentPath, CatalogContext context)
        {
            static IEnumerable<CatalogItem> GetPreconfiguredItems()
            {
                return
                [
                    new() {CatalogTypeId =2,CatalogBrandId =2,AvailableStock = 100,Description="Test",Name="Test",Id=1,PictureFileName="1.jpg", PictureUri = "some", OnReorder=false},
                    new() {CatalogTypeId =2,CatalogBrandId =2,AvailableStock = 100,Description="Test",Name="Test",Id=1,PictureFileName="2.jpg", PictureUri = "some", OnReorder=false},
                    new() {CatalogTypeId =2,CatalogBrandId =2,AvailableStock = 100,Description="Test",Name="Test",Id=1,PictureFileName="3.jpg", PictureUri = "some", OnReorder=false},
                    new() {CatalogTypeId =2,CatalogBrandId =2,AvailableStock = 100,Description="Test",Name="Test",Id=1,PictureFileName="4.jpg", PictureUri = "some", OnReorder=false},
                    new() {CatalogTypeId =2,CatalogBrandId =2,AvailableStock = 100,Description="Test",Name="Test",Id=1,PictureFileName="5.jpg", PictureUri = "some", OnReorder=false},
                    new() {CatalogTypeId =2,CatalogBrandId =2,AvailableStock = 100,Description="Test",Name="Test",Id=1,PictureFileName="6.jpg", PictureUri = "some", OnReorder=false},
                ];
            }

            string fileName = Path.Combine(contentPath, "CatalogItems.txt");

            if (File.Exists(fileName))
                return GetPreconfiguredItems();

            var catalogTypeIdLookUp = context.CatalogTypes.ToDictionary(ct => ct.Type, ct => ct.Id);
            var catalogBrandIdLookUp = context.CatalogBrands.ToDictionary(ct => ct.Brand, ct => ct.Id);

            var fileContent = File.ReadAllLines(fileName).Skip(1).Select(i => i.Split(',')).Select(i => new CatalogItem()
            {
                CatalogTypeId = catalogTypeIdLookUp[i[0]],
                CatalogBrandId = catalogBrandIdLookUp[i[0]],
                Description = i[2].Trim('"').Trim(),
                Name = i[3].Trim('"').Trim(),
                Price = Decimal.Parse(i[4].Trim('"').Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                PictureFileName = i[5].Trim('"').Trim(),
                AvailableStock = string.IsNullOrEmpty(i[6]) ? 0 : int.Parse(i[6]),
                //OnReorder = Convert.ToBoolean(i[7]),

            });

            return fileContent;
        }

        private static void GetCatalogItemPictures(string contentPath, string picturePath)
        {
            picturePath ??= "pics";

            if (picturePath is not null)
            {
                DirectoryInfo directory = new(picturePath);
                foreach (FileInfo item in directory.GetFiles())
                {
                    item.Delete();
                }

                string zibFileCatalogItemsPictures = Path.Combine(contentPath, "CatalogItems.zip");

                ZipFile.ExtractToDirectory(zibFileCatalogItemsPictures, picturePath);
            }
        }

    }
}

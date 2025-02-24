using CatalogService.Api.Core.Application.ViewModels;
using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure.Context;
using CatalogService.Api.Infrastructure.Setup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace CatalogService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogsController : ControllerBase
    {
        private readonly CatalogContext _catalogContext;
        private readonly CatalogSettings _catalogSettings;

        public CatalogsController(CatalogContext context, IOptionsSnapshot<CatalogSettings> settings)
        {
            _catalogContext = context ?? throw new ArgumentNullException(nameof(context));
            _catalogSettings = settings.Value;

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        // GET: api/<CatalogsController>
        [HttpGet]
        [Route("items")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetItemsAsync([FromQuery] int pageSize = 10, int pageIndex = 0, string ids = null)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = await GetItemsByIdAsync(ids);

                if (!items.Any())
                {
                    return BadRequest("ids value invalid , Must be comma-separated list of numbers");

                }

                return Ok(items);
            }

            var totalItems = await _catalogContext.CatalogItems.CountAsync();

            var itemsOnPage = await _catalogContext.CatalogItems.OrderBy(c => c.Name).Skip(pageSize * pageIndex).Take(pageSize).ToListAsync();

            itemsOnPage = ChangeUriPlaceHolder(itemsOnPage);

            var model =new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        private async Task<List<CatalogItem>> GetItemsByIdAsync(string ids)
        {
            var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(ids, out int x), Value: x));

            if (!numIds.All(nid => nid.Ok))
                return [];

            var idsToSelect = numIds.Select(id => id.Value);

            var items = await _catalogContext.CatalogItems.Where(ci => idsToSelect.Contains(ci.Id)).ToListAsync();

            items = ChangeUriPlaceHolder(items);
            return items;
        }


        [HttpGet()]
        [Route("items/{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CatalogItem), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CatalogItem>> ItemByIdAsync(int id)
        {
            if (id <= 0)
                return BadRequest();

            var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);

            var baseUri = _catalogSettings.PicBaseUrl;

            if (item is not null)
            {
                item.PictureUri = baseUri + item.PictureFileName;

                return item;
            }

            return NotFound();
        }

        [HttpGet()]
        [Route("items/withname/{name:minlength(1)}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsWithNameAsync(string name, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 10)
        {
            var totalItems = await _catalogContext.CatalogItems.Where(c => c.Name.StartsWith(name)).CountAsync();

            var itemsOnPage = await _catalogContext.CatalogItems.Where(c => c.Name.StartsWith(name)).Skip(pageSize * pageIndex).Take(pageSize).ToListAsync();

            itemsOnPage = ChangeUriPlaceHolder(itemsOnPage);

            return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
        }

        [HttpGet()]
        [Route("items/type/{catalogTypeId}/brand/{catalogBrandId:int?}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByTypeIdAndBrandIdAsync(int catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 10)
        {
            var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems;

            root = root.Where(ci => ci.CatalogTypeId == catalogTypeId);

            if (catalogBrandId.HasValue)
                root = root.Where(ci => ci.CatalogBrandId == catalogBrandId);

            var totalItems = await root.CountAsync();

            var itemsOnPage = await root.Skip(pageSize * pageIndex).Take(pageSize).ToListAsync();

            itemsOnPage = ChangeUriPlaceHolder(itemsOnPage);

            return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

        }

        [HttpGet()]
        [Route("items/type/all/brand/{catalogBrandId:int?}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByBrandIdAsync(int catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 10)
        {
            var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems;

            if (catalogBrandId.HasValue)
                root = root.Where(ci => ci.CatalogBrandId == catalogBrandId);

            var totalItems = await root.CountAsync();

            var itemsOnPage = await root.Skip(pageSize * pageIndex).Take(pageSize).ToListAsync();

            itemsOnPage = ChangeUriPlaceHolder(itemsOnPage);

            return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

        }

        // GET api/v1/[controller]/CatalogBrands
        [HttpGet]
        [Route("catalogTypes")]
        [ProducesResponseType(typeof(List<CatalogType>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CatalogType>>> CatalogTypesAsync()
        {
            return await _catalogContext.CatalogTypes.ToListAsync();
        }

        // GET api/v1/[controller]/CatalogBrands
        [HttpGet]
        [Route("catalogBrands")]
        [ProducesResponseType(typeof(List<CatalogBrand>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CatalogBrand>>> CatalogBrandsAsync()
        {
            return await _catalogContext.CatalogBrands.ToListAsync();
        }

        // PUT api/v1/[controller]/items
        [Route("items")]
        [HttpPut()]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> UpdateProductAsync([FromBody] CatalogItem productToUpdate)
        {
            var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == productToUpdate.Id);

            if (item is null)
                return new NotFoundResult();

            var oldPrice = item.Price;
            var raiseProductPriceChangeEvent = oldPrice != productToUpdate.Price;

            // Update current Product
            item = productToUpdate;
            _catalogContext.CatalogItems.Update(item);
            await _catalogContext.SaveChangesAsync();

            return CreatedAtAction(nameof(ItemByIdAsync), new { id = productToUpdate.Id }, null);
        }

        // POST api/v1/[controller]/items
        [Route("items")]
        [HttpPost()]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> CrateProductAsync([FromBody] CatalogItem product)
        {
            if (product is null)
                return BadRequest();

           var item = new CatalogItem()
           {
               CatalogBrandId = product.CatalogBrandId,
               CatalogType = product.CatalogType,
               Price = product.Price,
               Description = product.Description,
               Name = product.Name,
               PictureFileName = product.PictureFileName,
           };

            _catalogContext.CatalogItems.Add(item);
            await _catalogContext.SaveChangesAsync();

            return CreatedAtAction(nameof(ItemByIdAsync), new { id = item.Id }, null);
        }

        // DELETE api/v1/[controller]/id
        [Route("{id}")]
        [HttpDelete()]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<ActionResult> DeleteProductAsync(int id)
        {
            var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);

            if (item is null)
                return NotFound();

            _catalogContext.CatalogItems.Remove(item);

            await _catalogContext.SaveChangesAsync();   

            return NoContent();
        }

        private List<CatalogItem> ChangeUriPlaceHolder(List<CatalogItem> items)
        {
            var baseUri = _catalogSettings.PicBaseUrl;

            foreach (var item in items)
            {
                if (item is not null)
                    item.PictureUri = baseUri + item.PictureFileName;
            }

            return items;
        }
    }
}

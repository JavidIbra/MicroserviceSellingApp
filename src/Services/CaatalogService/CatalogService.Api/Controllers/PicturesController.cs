using CatalogService.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CatalogService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PicturesController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly CatalogContext _catalogContext;

        public PicturesController(IWebHostEnvironment webHostEnvironment, CatalogContext catalogContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _catalogContext = catalogContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("App and running");
        }

        [HttpGet]
        [Route("api/vq/catalog/items/{catalogItemId:int}/pic")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetImageAsync(int catalogItemId)
        {
            if (catalogItemId <= 0)
                return BadRequest();

            var item = await _catalogContext.CatalogItems
                .SingleOrDefaultAsync(ci => ci.Id == catalogItemId);

            if (item is not null)
            {
                var webRoot = _webHostEnvironment.WebRootPath;
                var path = Path.Combine(webRoot, item.PictureFileName);

                string imageFileExtension = Path.GetExtension(item.PictureFileName);
                string mimeType = GetImageMimeTypeFromImageFileExtension(imageFileExtension);

                var buffer = await System.IO.File.ReadAllBytesAsync(path);

                return File(buffer, mimeType);
            }

            return NotFound();
        }

        private string GetImageMimeTypeFromImageFileExtension(string imageFileExtension)
        {
            string mimeType = null;

            switch (imageFileExtension)
            {
                case ".png":
                    return "";

            }
            return "";
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<PicturesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<PicturesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PicturesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalog.API.Data;
using VideoGameCatalog.API.DTOs;
using VideoGameCatalog.API.Models;
// Використовуємо псевдонім, бо Attribute - системне слово
using DbAttribute = VideoGameCatalog.API.Models.Attribute;

namespace VideoGameCatalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttributesController : ControllerBase
    {
        private readonly VideoGameCatalogContext _context;

        public AttributesController(VideoGameCatalogContext context)
        {
            _context = context;
        }

        // GET: api/attributes (Повертає список: Платформа, Розробник, Мова...)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DbAttribute>>> GetAttributes()
        {
            return await _context.Attributes.ToListAsync();
        }

        // POST: api/attributes (Додати нову назву характеристики)
        [HttpPost]
        public async Task<ActionResult<DbAttribute>> CreateAttribute(SimpleItemDto dto)
        {
            var attr = new DbAttribute { AttributeName = dto.Name };
            _context.Attributes.Add(attr);
            await _context.SaveChangesAsync();
            return Ok(attr);
        }

        // PUT: api/attributes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttribute(int id, SimpleItemDto dto)
        {
            var attr = await _context.Attributes.FindAsync(id);
            if (attr == null) return NotFound();
            attr.AttributeName = dto.Name;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/attributes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttribute(int id)
        {
            var attr = await _context.Attributes.FindAsync(id);
            if (attr == null) return NotFound();
            _context.Attributes.Remove(attr);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

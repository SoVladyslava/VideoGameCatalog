using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalog.API.Data;
using VideoGameCatalog.API.DTOs;
using VideoGameCatalog.API.Models;

namespace VideoGameCatalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly VideoGameCatalogContext _context;

        public BrandsController(VideoGameCatalogContext context)
        {
            _context = context;
        }

        // GET: /api/brands
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
        {
            var brands = await _context.Brands.ToListAsync();
            return Ok(brands);
        }

        // POST: api/brands
        [HttpPost]
        public async Task<ActionResult<Brand>> CreateBrand(SimpleItemDto dto)
        {
            var brand = new Brand { BrandName = dto.Name };
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetBrands", new { id = brand.Id }, brand);
        }

        // PUT: api/brands/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, SimpleItemDto dto)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            brand.BrandName = dto.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/brands/5
        /*[HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            // Увага: Якщо є ігри цього бренду, видалення впаде (потрібен каскад або перевірка)
            // Для курсової припустимо, що ми видаляємо тільки порожні бренди
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return NoContent();
        }*/
        // DELETE: api/brands/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            // 1. ЗНАЙТИ ТА ВИДАЛИТИ УСІ ПОВ'ЯЗАНІ ІГРИ ТА ЇХНІ ДАНІ
            var gamesToDelete = await _context.Videogames
                .Where(g => g.BrandId == id)
                .ToListAsync();

            // Запобігання Foreign Key помилкам:
            foreach (var game in gamesToDelete)
            {
                // Видаляємо ValueAttributes для цієї гри
                var attrs = _context.Valueattributes.Where(v => v.VideoGameId == game.Id);
                _context.Valueattributes.RemoveRange(attrs);

                // Видаляємо зв'язки з жанрами (VideoGameGenre) для цієї гри
                var genres = _context.VideoGameGenres.Where(v => v.VideoGameId == game.Id);
                _context.VideoGameGenres.RemoveRange(genres);

                // Видаляємо саму гру
                _context.Videogames.Remove(game);
            }

            // 2. ВИДАЛЯЄМО БРЕНД
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

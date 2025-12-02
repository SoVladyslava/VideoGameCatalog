using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VideoGameCatalog.API.Data;
using VideoGameCatalog.API.Models;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalog.API.DTOs;

namespace VideoGameCatalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly VideoGameCatalogContext _context;

        public GenresController(VideoGameCatalogContext context)
        {
            _context = context;
        }

        // GET: /api/genres
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
        {
            // Просто звертаємось до іншої таблиці: _context.Genres
            var genres = await _context.Genres.ToListAsync();
            return Ok(genres);
        }

        // POST: api/brands
        [HttpPost]
        public async Task<ActionResult<Brand>> CreateGenre(SimpleItemDto dto)
        {
            var genre = new Genre { GenreName = dto.Name };
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetBrands", new { id = genre.Id }, genre);
        }

        // PUT: api/genres/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGenre(int id, SimpleItemDto dto)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return NotFound();

            genre.GenreName = dto.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/genres/5
        /* [HttpDelete("{id}")]
         public async Task<IActionResult> DeleteGenre(int id)
         {
             var genre = await _context.Genres.FindAsync(id);
             if (genre == null) return NotFound();

             // Увага: Якщо є ігри цього бренду, видалення впаде (потрібен каскад або перевірка)
             // Для курсової припустимо, що ми видаляємо тільки порожні бренди
             _context.Genres.Remove(genre);
             await _context.SaveChangesAsync();

             return NoContent();
         }*/
        // DELETE: api/genres/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return NotFound();

            // 1. ВИДАЛЯЄМО УСІ ПОВ'ЯЗАНІ ЗАПИСИ У СПОЛУЧНІЙ ТАБЛИЦІ VideoGameGenre
            var associations = _context.VideoGameGenres.Where(vg => vg.GenreId == id);
            _context.VideoGameGenres.RemoveRange(associations);

            // 2. ВИДАЛЯЄМО САМ ЖАНР
            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        } 
    }
}

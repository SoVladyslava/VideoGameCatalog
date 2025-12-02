using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalog.API.Data; // <-- Переконайтесь, що шлях до Data правильний
using VideoGameCatalog.API.DTOs;
using VideoGameCatalog.API.Models; // <-- Переконайтесь, що шлях до Models правильний

namespace VideoGameCatalog.API.Controllers
{
    [Route("api/[controller]")] // Наша URL-адреса буде /api/categories
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        // 1. Створюємо приватне поле для зберігання DbContext
        private readonly VideoGameCatalogContext _context;

        // 2. Використовуємо "Dependency Injection" для отримання DbContext
        // ASP.NET сам "вставить" наш DbContext (який ми налаштували у Program.cs) 
        // у цей конструктор
        public CategoriesController(VideoGameCatalogContext context)
        {
            _context = context;
        }

        // GET: /api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            // 4. Асинхронно звертаємось до БД, до таблиці Categories,
            // і просимо повернути всі записи у вигляді списку.
            var categories = await _context.Categories.ToListAsync();

            // 5. Повертаємо список з кодом "200 OK"
            return Ok(categories);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory(SimpleItemDto dto)
        {
            var category = new Category { CategoryName = dto.Name };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetBrands", new { id = category.Id }, category);
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, SimpleItemDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.CategoryName = dto.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/categories/5
        /*[HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            // Увага: Якщо є ігри цього бренду, видалення впаде (потрібен каскад або перевірка)
            // Для курсової припустимо, що ми видаляємо тільки порожні бренди
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }*/


        // DELETE: api/category/5
         [HttpDelete("{id}")]
         public async Task<IActionResult> DeleteCategory(int id)
         {
             var category = await _context.Categories.FindAsync(id);
             if (category == null) return NotFound();

             // 1. ЗНАЙТИ ТА ВИДАЛИТИ УСІ ПОВ'ЯЗАНІ ІГРИ ТА ЇХНІ ДАНІ
             var gamesToDelete = await _context.Videogames
                 .Where(g => g.CategoryId == id)
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

             // 2. ВИДАЛЯЄМО Category
             _context.Categories.Remove(category);
             await _context.SaveChangesAsync();

             return NoContent();
         }

       
    }
}

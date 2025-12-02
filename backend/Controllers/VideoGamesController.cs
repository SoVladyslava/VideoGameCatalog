using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalog.API.Data;
using VideoGameCatalog.API.DTOs; // <-- Підключаємо наші DTO

namespace VideoGameCatalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoGamesController : ControllerBase
    {
        private readonly VideoGameCatalogContext _context;

        public VideoGamesController(VideoGameCatalogContext context)
        {
            _context = context;
        }

        // --- ЕНДПОІНТ 1: Отримання каталогу з фільтрами ---
        // GET: /api/videogames?categoryId=1&brandId=2&genreId=3
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VideoGameBasicDto>>> GetVideoGames(
            [FromQuery] int? categoryId,
            [FromQuery] int? brandId,
            [FromQuery] int? genreId)
        {
            // 1. Починаємо будувати запит до БД. 
            // Використовуємо IQueryable для ефективної фільтрації
            var query = _context.Videogames.AsQueryable();

            // 2. Динамічно додаємо фільтри, ЯКЩО вони були передані
            if (categoryId.HasValue)
            {
                query = query.Where(g => g.CategoryId == categoryId.Value);
            }
            if (brandId.HasValue)
            {
                query = query.Where(g => g.BrandId == brandId.Value);
            }
            if (genreId.HasValue)
            {
                // query = query.Where(g => g.GenreId == genreId.Value);
                // Перевіряємо, чи гра має *хоча б один* запис у VideoGameGenres
                // з потрібним нам GenreId
                query = query.Where(g => g.VideoGameGenres.Any(vg => vg.GenreId == genreId.Value));
            }

            // 3. Проєкція: Перетворюємо наш запит з БД у VideoGameBasicDto
            // EF Core сам зробить необхідні JOIN'и для отримання імен
            var games = await query
                .Select(g => new VideoGameBasicDto
                {
                    Id = g.Id,
                    GameName = g.GameName,
                    Price = g.Price,
                    BrandName = g.Brand.BrandName, // JOIN з BRAND
                    CategoryName = g.Category.CategoryName // JOIN з CATEGORY
                })
                .ToListAsync();

            return Ok(games);
        }

        // --- ЕНДПОІНТ 2: Отримання детальної інформації про одну гру ---
        // GET: /api/videogames/5 (де 5 - це Id гри)
        [HttpGet("{id}")]
        public async Task<ActionResult<VideoGameDetailDto>> GetVideoGame(int id)
        {
            // 1. Шукаємо гру за 'id' і одразу проєктуємо її в DTO
            var gameDto = await _context.Videogames
                .Where(g => g.Id == id)
                .Select(g => new VideoGameDetailDto
                {
                    Id = g.Id,
                    GameName = g.GameName,
                    Price = g.Price,
                    Rating = g.Rating,
                    BrandName = g.Brand.BrandName,
                    // GenreName = g.Genre.Genre_Name,
                    // Збираємо список імен жанрів з колекції VideoGameGenres
                    GenreNames = g.VideoGameGenres.Select(vg => vg.Genre.GenreName).ToList(),
                    CategoryName = g.Category.CategoryName,

                    // 2. Внутрішня проєкція для дочірньої колекції атрибутів
                    Attributes = g.Valueattributes
                        .Select(va => new AttributeDto
                        {
                            AttributeName = va.Attribute.AttributeName, // JOIN з ATTRIBUTES
                            Value = va.Value
                        }).ToList()
                })
                .FirstOrDefaultAsync(); // Знайти перший або null

            // 3. Якщо гру з таким 'id' не знайдено, повертаємо 404
            if (gameDto == null)
            {
                return NotFound();
            }

            // 4. Якщо все добре, повертаємо DTO з кодом 200 OK
            return Ok(gameDto);
        }

        // --- ЕНДПОІНТ 3: Додавання гри  ---
        // POST: api/videogames (Додавання гри)
        [HttpPost]
        public async Task<ActionResult<VideoGameBasicDto>> CreateVideoGame(VideoGameCreateDto dto)
        {
            // 1. Створюємо гру
            var game = new Models.Videogame
            {
                GameName = dto.GameName,
                Price = (float)dto.Price,
                BrandId = dto.BrandId,
                CategoryId = dto.CategoryId,
                Rating = dto.Rating
            };

            _context.Videogames.Add(game);
            await _context.SaveChangesAsync(); // Отримуємо ID

            // 2. Зберігаємо Жанри
            if (dto.GenreIds != null)
            {
                foreach (var genreId in dto.GenreIds)
                {
                    _context.VideoGameGenres.Add(new Models.VideoGameGenre
                    {
                        VideoGameId = game.Id,
                        GenreId = genreId
                    });
                }
            }

            // 3. ЗБЕРІГАЄМО АТРИБУТИ (ХАРАКТЕРИСТИКИ)
            if (dto.Attributes != null && dto.Attributes.Any())
            {
                foreach (var attrDto in dto.Attributes)
                {
                    _context.Valueattributes.Add(new Models.Valueattribute
                    {
                        VideoGameId = game.Id,
                        AttributeId = attrDto.AttributeId,
                        Value = attrDto.Value
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Гру створено!" });
        }


        // --- ЕНДПОІНТ 4: Редагування гри ---
        // PUT: api/videogames/5 (Редагування гри)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVideoGame(int id, [FromBody] VideoGameUpdateDto dto)
        {
            var game = await _context.Videogames
                .Include(g => g.VideoGameGenres)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null) return NotFound();

            // Оновлення простих полів
            game.GameName = dto.GameName;
            game.Price = (float)dto.Price;
            game.BrandId = dto.BrandId;
            game.CategoryId = dto.CategoryId;
            game.Rating = dto.Rating;

            // Оновлення Жанрів
            _context.VideoGameGenres.RemoveRange(game.VideoGameGenres);
            if (dto.GenreIds != null)
            {
                foreach (var genreId in dto.GenreIds)
                {
                    _context.VideoGameGenres.Add(new Models.VideoGameGenre { VideoGameId = game.Id, GenreId = genreId });
                }
            }

            // ОНОВЛЕННЯ АТРИБУТІВ
            // 1. Знаходимо і видаляємо старі атрибути цієї гри
            var oldAttributes = _context.Valueattributes.Where(v => v.VideoGameId == id);
            _context.Valueattributes.RemoveRange(oldAttributes);

            // 2. Додаємо нові
            if (dto.Attributes != null)
            {
                foreach (var attrDto in dto.Attributes)
                {
                    _context.Valueattributes.Add(new Models.Valueattribute
                    {
                        VideoGameId = game.Id,
                        AttributeId = attrDto.AttributeId,
                        Value = attrDto.Value
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- ЕНДПОІНТ 5: Видалення гри ---
        // DELETE: api/videogames/5 (Видалення гри)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVideoGame(int id)
        {
            var game = await _context.Videogames.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            // Спочатку треба видалити залежні дані (атрибути та жанри),
            // якщо у вас не налаштовано каскадне видалення в БД.
            // Для курсової найпростіше видалити їх вручну:

            var attributes = _context.Valueattributes.Where(v => v.VideoGameId == id);
            _context.Valueattributes.RemoveRange(attributes);

            var genres = _context.VideoGameGenres.Where(v => v.VideoGameId == id);
            _context.VideoGameGenres.RemoveRange(genres);

            // Тепер видаляємо саму гру
            _context.Videogames.Remove(game);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Гру видалено" });
        }
    }
}

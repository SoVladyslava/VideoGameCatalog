using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalog.API.Data;
using VideoGameCatalog.API.DTOs;

namespace VideoGameCatalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly VideoGameCatalogContext _context;

        public AuthController(VideoGameCatalogContext context)
        {
            _context = context;
        }

      /*  [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Шукаємо користувача в БД
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Password == loginDto.Password);

            if (user == null)
            {
                return Unauthorized("Невірний email або пароль");
            }

            // Повертаємо роль користувача (Admin або User)
            return Ok(new { role = user.Role, email = user.Email });
        }*/

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // 1. Шукаємо юзера І ПІДВАНТАЖУЄМО РОЛЬ (Include)
            var user = await _context.Users
                .Include(u => u.Role) // <--- ВАЖЛИВО! Без цього Role буде null
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Password == loginDto.Password);

            if (user == null)
            {
                return Unauthorized("Невірний email або пароль");
            }

            // 2. Формуємо відповідь так, як звик Фронтенд
            return Ok(new
            {
                email = user.Email,
                // Беремо текст із пов'язаної таблиці
                role = user.Role.RoleName
            });
        }
    }
}

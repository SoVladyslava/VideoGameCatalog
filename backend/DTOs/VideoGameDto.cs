using System.ComponentModel.DataAnnotations;

namespace VideoGameCatalog.API.DTOs
{
    // 1. DTO для відображення гри (GET)
    public class AttributeDto
    {
        // Властивості в DTO теж називаємо за C# стандартом 
        public string? AttributeName { get; set; }
        public string? Value { get; set; }
    }

    public class VideoGameBasicDto
    {
        public int Id { get; set; }
        public string? GameName { get; set; }
        public double Price { get; set; }
        public string? BrandName { get; set; }
        public string? CategoryName { get; set; }
    }

    public class VideoGameDetailDto
    {
        public int Id { get; set; }
        public string? GameName { get; set; }
        public double Price { get; set; }

        [Range(0, 5, ErrorMessage = "Рейтинг повинен бути від 0 до 5")]
        public decimal? Rating { get; set; }
        public string? BrandName { get; set; }
        //  public string? GenreName { get; set; }
        public List<string>? GenreNames { get; set; }
        public string? CategoryName { get; set; }

        // Додаємо ? щоб вирішити попередження CS8618
        public List<AttributeDto>? Attributes { get; set; }
    }

    // DTO для входу в систему (Auth)
    public class LoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    // DTO для створення нової гри
    public class VideoGameCreateDto
    {
        public required string GameName { get; set; }
        public double Price { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public decimal? Rating { get; set; }
        // Для простоти жанри передаємо списком ID (наприклад: [1, 2])
        public List<int>? GenreIds { get; set; }
        public List<VideoGameValueDto>? Attributes { get; set; }

    }

    // Універсальний DTO для створення/зміни (Бренд, Категорія, Жанр)
    public class SimpleItemDto
    {
        public required string Name { get; set; }
    }
    // DTO для збереження однієї характеристики
    public class VideoGameValueDto
    {
        public int AttributeId { get; set; }
        public required string Value { get; set; }
    }

    // DTO для редагування гри (можна використати і Create, але для ясності окремо)
    public class VideoGameUpdateDto : VideoGameCreateDto
    {
        // Наслідує всі поля (GameName, Price...), нам нічого додавати не треба
    }
}

using Microsoft.EntityFrameworkCore;
using VideoGameCatalog.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 1. Отримуємо строку підключення з appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Реєструємо DbContext (наш клас для роботи з БД)
// Не забудьте додати "using" для вашого DbContext та EF Core
// using Microsoft.EntityFrameworkCore;
// using VideoGameCatalog.API.Data; // Шлях до вашого VideoGameCatalogContext

builder.Services.AddDbContext<VideoGameCatalogContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Додаємо дозвіл CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // Дозволити будь-якому сайту звертатись
              .AllowAnyMethod()  // Дозволити будь-які методи (GET, POST...)
              .AllowAnyHeader(); // Дозволити будь-які заголовки
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Вмикаємо CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

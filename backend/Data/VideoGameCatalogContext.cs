    using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalog.API.Models;
using DbAttribute = VideoGameCatalog.API.Models.Attribute;

namespace VideoGameCatalog.API.Data;

public partial class VideoGameCatalogContext : DbContext
{
    public VideoGameCatalogContext()
    {
    }

    public VideoGameCatalogContext(DbContextOptions<VideoGameCatalogContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DbAttribute> Attributes { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Valueattribute> Valueattributes { get; set; }

    public virtual DbSet<Videogame> Videogames { get; set; }
    public virtual DbSet<VideoGameGenre> VideoGameGenres { get; set; }
    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbAttribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ATTRIBUT__3214EC27D8D30A09");

            entity.ToTable("ATTRIBUTES");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AttributeName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Attribute_Name");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BRAND__3214EC27BCA48E57");

            entity.ToTable("BRAND");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BrandName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Brand_Name");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CATEGORY__3214EC2703232BB2");

            entity.ToTable("CATEGORY");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Category_Name");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GENRE__3214EC27D1E6AD98");

            entity.ToTable("GENRE");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GenreName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Genre_Name");
        });


        // ▼▼▼ ДОДАТИ НАЛАШТУВАННЯ ДЛЯ РОЛІ ▼▼▼
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("ROLES");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.RoleName).HasMaxLength(50).IsUnicode(false).HasColumnName("Role_Name");
        });
        // ▼▼▼ ОНОВИТИ НАЛАШТУВАННЯ ДЛЯ USERS ▼▼▼
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USERS");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(255).IsUnicode(false);

            // Стару колонку Role видалили, додаємо налаштування зв'язку:
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Role)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });


       /* modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__USERS__3214EC27D554D3F6");

            entity.ToTable("USERS");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);
        });*/

        modelBuilder.Entity<Valueattribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VALUEATT__3214EC27BDD88705");

            entity.ToTable("VALUEATTRIBUTES");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AttributeId).HasColumnName("AttributeID");
            entity.Property(e => e.Value)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.VideoGameId).HasColumnName("VideoGameID");

            entity.HasOne(d => d.Attribute).WithMany(p => p.Valueattributes)
                .HasForeignKey(d => d.AttributeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VALUEATTR__Attri__5812160E");

            entity.HasOne(d => d.VideoGame).WithMany(p => p.Valueattributes)
                .HasForeignKey(d => d.VideoGameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VALUEATTR__Video__59063A47");
        });

        modelBuilder.Entity<Videogame>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VIDEOGAM__3214EC27D186A173");

            entity.ToTable("VIDEOGAMES");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.GameName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Game_Name");
            entity.Property(e => e.Rating).HasColumnType("decimal(2, 1)");

            entity.HasOne(d => d.Brand).WithMany(p => p.Videogames)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VIDEOGAME__Brand__534D60F1");

            entity.HasOne(d => d.Category).WithMany(p => p.Videogames)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VIDEOGAME__Categ__5535A963");

            // 
            // ▼▼▼ НЕПРАВИЛЬНИЙ БЛОК БУЛО ВИДАЛЕНО ЗВІДСИ ▼▼▼
            //
            // entity.HasMany(d => d.Genre).WithMany(p => p.VideoGamesGenres)...
            //
            // ▲▲▲ НЕПРАВИЛЬНИЙ БЛОК БУЛО ВИДАЛЕНО ЗВІДСИ ▲▲▲
            // 
        });

        // 
        // ▼▼▼ ЦЕ ПРАВИЛЬНИЙ БЛОК, ЯКИЙ ЗАЛИШИВСЯ ▼▼▼
        //
        modelBuilder.Entity<VideoGameGenre>(entity =>
        {
            // Вказуємо, що у нас композитний ключ (PK)
            entity.HasKey(e => new { e.VideoGameId, e.GenreId });

            // Вказуємо таблицю, бо EF може не знайти її сам
            entity.ToTable("VIDEOGAMEGENRES");

            // Налаштовуємо зв'язок з Videogame
            entity.HasOne(d => d.VideoGame)
                .WithMany(p => p.VideoGameGenres) // Посилається на ICollection у Videogame
                .HasForeignKey(d => d.VideoGameId) // Зовнішній ключ
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VideoGameGenre_VideoGame");

            // Налаштовуємо зв'язок з Genre
            entity.HasOne(d => d.Genre)
                .WithMany(p => p.VideoGameGenres) // Посилається на ICollection у Genre
                .HasForeignKey(d => d.GenreId) // Зовнішній ключ
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VideoGameGenre_Genre");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

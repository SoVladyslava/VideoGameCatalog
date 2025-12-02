using System;
using System.Collections.Generic;

namespace VideoGameCatalog.API.Models;

public partial class Videogame
{
    public Videogame()
    {
        // Ініціалізуємо обидві колекції
        VideoGameGenres = new HashSet<VideoGameGenre>();
        Valueattributes = new HashSet<Valueattribute>();
    }

    public int Id { get; set; }

    public string GameName { get; set; } = null!;

    public float Price { get; set; }

    public int BrandId { get; set; }

    public int CategoryId { get; set; }

    public decimal? Rating { get; set; }

    public virtual Brand? Brand { get; set; } = null!;

    public virtual Category? Category { get; set; } = null!;

    public virtual ICollection<Valueattribute> Valueattributes { get; set; } = new List<Valueattribute>();

    // public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public virtual ICollection<VideoGameGenre> VideoGameGenres { get; set; }
}

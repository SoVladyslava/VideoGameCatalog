using System;
using System.Collections.Generic;

namespace VideoGameCatalog.API.Models;

public partial class Genre
{
    public Genre()
    {
        // Ініціалізуємо колекцію
        VideoGameGenres = new HashSet<VideoGameGenre>();
    }

    public int Id { get; set; }

    public string GenreName { get; set; } = null!;

    public virtual ICollection<VideoGameGenre> VideoGameGenres { get; set; }
}

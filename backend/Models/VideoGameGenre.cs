using System;
using System.Collections.Generic;namespace VideoGameCatalog.API.Models
{
    public class VideoGameGenre
    {
        // Колонки з вашої таблиці
        public int VideoGameId { get; set; }
        public int GenreId { get; set; }

        // Навігаційні властивості (для зв'язку)
        public virtual Genre? Genre { get; set; }
        public virtual Videogame? VideoGame { get; set; }
    }
}

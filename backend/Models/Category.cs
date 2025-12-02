using System;
using System.Collections.Generic;

namespace VideoGameCatalog.API.Models;

public partial class Category
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Videogame> Videogames { get; set; } = new List<Videogame>();
}

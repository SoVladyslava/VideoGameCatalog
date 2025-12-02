using System;
using System.Collections.Generic;

namespace VideoGameCatalog.API.Models;

public partial class Brand
{
    public int Id { get; set; }

    public string BrandName { get; set; } = null!;

    public virtual ICollection<Videogame> Videogames { get; set; } = new List<Videogame>();
}

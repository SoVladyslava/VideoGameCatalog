using System;
using System.Collections.Generic;
using DbAttribute = VideoGameCatalog.API.Models.Attribute;

namespace VideoGameCatalog.API.Models;

public partial class Valueattribute
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;

    public int AttributeId { get; set; }

    public int VideoGameId { get; set; }

    public virtual DbAttribute Attribute { get; set; } = null!;

    public virtual Videogame VideoGame { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace VideoGameCatalog.API.Models;

public partial class Attribute
{
    public int Id { get; set; }

    public string AttributeName { get; set; } = null!;

    public virtual ICollection<Valueattribute> Valueattributes { get; set; } = new List<Valueattribute>();
}

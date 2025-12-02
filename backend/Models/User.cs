using System;
using System.Collections.Generic;

namespace VideoGameCatalog.API.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    // public string Role { get; set; } = null!;

    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
}

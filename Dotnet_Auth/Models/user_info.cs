using System;
using System.Collections.Generic;

namespace Dotnet_Auth.Models;

public partial class user_info
{
    public Guid id { get; set; }

    public string? user_name { get; set; }

    public string? email { get; set; }

    public string? password { get; set; }

    public virtual ICollection<common_data> common_data { get; set; } = new List<common_data>();
}

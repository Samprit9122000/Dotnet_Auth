using System;
using System.Collections.Generic;

namespace Dotnet_Auth.Models;

public partial class common_data
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public string? comment { get; set; }

    public bool? is_active { get; set; }

    public virtual user_info user { get; set; } = null!;
}

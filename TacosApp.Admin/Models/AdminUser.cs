using Microsoft.AspNetCore.Identity;

namespace TacosApp.Admin.Models;

public class AdminUser : IdentityUser
{
    public string? DisplayName { get; set; }
}

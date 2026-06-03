using Microsoft.AspNetCore.Identity;
using TacosApp.Admin.Models;

namespace TacosApp.Admin.Infrastructure;

public static class IdentitySeeder
{
    public static async Task SeedAdminAsync(IServiceProvider services, IConfiguration config)
    {
        var userManager = services.GetRequiredService<UserManager<AdminUser>>();
        var email = config["AdminSeed:Email"] ?? "admin@tacos.local";
        var password = config["AdminSeed:Password"] ?? "Admin#12345";

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new AdminUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "管理者"
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"管理者シードに失敗しました: {errors}");
        }
    }
}

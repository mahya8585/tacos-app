using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TacosApp.Admin.Data;
using TacosApp.Admin.Hubs;
using TacosApp.Admin.Infrastructure;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AdminDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
        sql.MigrationsHistoryTable("__EFMigrationsHistoryAdmin")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddDefaultIdentity<AdminUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AdminDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToAreaFolder("Identity", "/Account");
});

builder.Services.AddSignalR();

builder.Services.AddHttpClient("TacosWeb", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["WebApiBaseUrl"] ?? "http://localhost:5081");
    client.DefaultRequestHeaders.Add("X-Api-Key", config["WebApiKey"] ?? "");
});

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IToppingService, ToppingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapHub<AdminOrderHub>("/hubs/admin-orders");

// Minimal API — for comparison with the legacy Web API 2 ApiController in TacosApp.Web
var adminApi = app.MapGroup("/api/admin").RequireAuthorization();

adminApi.MapPut("/orders/{id:int}/status", async (
    int id,
    [FromBody] UpdateStatusRequest body,
    IOrderService orderService,
    CancellationToken ct) =>
{
    if (!Enum.IsDefined(body.Status))
    {
        return Results.BadRequest(new { error = "Invalid status." });
    }
    var ok = await orderService.UpdateStatusAsync(id, body.Status, ct);
    return ok ? Results.NoContent() : Results.NotFound();
});

adminApi.MapGet("/orders/stats", async (IOrderService orderService, CancellationToken ct) =>
    Results.Ok(await orderService.GetStatsAsync(ct)));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
    await db.Database.MigrateAsync();
    await IdentitySeeder.SeedAdminAsync(scope.ServiceProvider, app.Configuration);
}

app.Run();

internal sealed record UpdateStatusRequest(OrderStatus Status);

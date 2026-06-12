using Microsoft.EntityFrameworkCore;
using TacosApp.Web.Data;
using TacosApp.Web.Filters;
using TacosApp.Web.Hubs;
using TacosApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// --- サービス登録 ---

// Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ASP.NET Core MVC (Web API 用)
builder.Services.AddControllers()
    .AddNewtonsoftJson();

// ASP.NET Core SignalR (組み込み)
builder.Services.AddSignalR();

// Entity Framework Core
builder.Services.AddDbContext<TacosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TacosDb")));

// セッション (CartService で使用)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// アプリケーションサービス
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddSingleton<OrderStatusNotificationService>();

// Web API フィルター (ServiceFilter で DI 対応)
builder.Services.AddScoped<ApiKeyAuthFilter>();

// CORS (管理アプリからの API アクセス用)
var adminOrigin = builder.Configuration["AdminAppOrigin"] ?? "http://localhost:8080";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.WithOrigins(adminOrigin)
              .WithHeaders("Content-Type", "X-Api-Key")
              .WithMethods("GET", "PUT");
    });
});

var app = builder.Build();

// 起動時にマイグレーションを適用し、シードデータ（HasData）を投入する
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TacosDbContext>();
    db.Database.Migrate();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    logger.LogError(ex, "Failed to apply database migrations at startup.");
}

// --- ミドルウェアパイプライン ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// HTTPS redirect is handled by the platform (Azure Container Apps ingress) in production.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
// Content/images/ フォルダーを /images/ パスで配信（DB の ImageUrl と対応）
var imagesPath = Path.Combine(builder.Environment.ContentRootPath, "Content", "images");
Directory.CreateDirectory(imagesPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(imagesPath),
    RequestPath = "/images"
});

app.UseRouting();
app.UseCors("AdminPolicy");
app.UseSession();
app.UseAntiforgery();

// Web API エンドポイント
app.MapControllers();

// SignalR ハブ
app.MapHub<OrderStatusHub>("/orderStatusHub");

// Blazor Server
app.MapRazorComponents<TacosApp.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

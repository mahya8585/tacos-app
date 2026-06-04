# TacosApp.Web — Blazor Server 移行計画

## 概要

**対象プロジェクト:** `TacosApp.Web` (ASP.NET MVC 5 / Web API 2 / SignalR 2 / EF6, .NET Framework 4.8)  
**移行先:** ASP.NET Core Blazor Server (.NET 10.0 LTS)  
**難易度:** 🔴 High (321 issues — 282 API, 22 NuGet, 3 binding)

アセスメントでは 246 件 (87.2%) が `System.Web.*` 起因の非互換 API であり、これらは ASP.NET Core の等価 API への置き換えが必要。  
MVC コントローラー + .aspx/.cshtml ビューを Blazor Server コンポーネント (.razor) に変換することが最大の作業量。

---

## 移行フェーズ

### Phase 1 — プロジェクトインフラストラクチャ

**目標:** SDK スタイル csproj への変換と NuGet パッケージの更新

| 作業 | 詳細 |
|------|------|
| csproj を SDK スタイルに変換 | `<ProjectTypeGuids>` `<HintPath>` を除去し `<Project Sdk="Microsoft.NET.Sdk.Web">` に変更 |
| ターゲットフレームワーク変更 | `net48` → `net10.0` |
| packages.config 廃止 | `packages.config` 削除、`<PackageReference>` に移行 |
| 削除するパッケージ | `System.Web.Mvc`, `System.Web.Http`, `System.Web.Http.Cors`, `Microsoft.AspNet.SignalR`, `Microsoft.Owin.*`, `System.Web.Optimization`, `EntityFramework`, `WebGrease`, `Antlr`, `Microsoft.Web.Infrastructure` |
| 追加するパッケージ | `Microsoft.AspNetCore.SignalR` (net10 組み込み), `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Newtonsoft.Json` (既存) |
| バインディング リダイレクト修正 | Newtonsoft.Json (13.0.0.0 → 13.0.3)、WebGrease リダイレクト削除 |

**判断:** `TacosApp.Admin` (net10.0 Razor Pages) と同一 SDK スタイルを採用。

---

### Phase 2 — アプリケーション エントリポイント

**目標:** `Global.asax` + `Startup.cs` (OWIN) を `Program.cs` に置き換え

| 削除 | 置き換え |
|------|---------|
| `Global.asax` / `Global.asax.cs` | `Program.cs` (ASP.NET Core Web Application Builder) |
| `Startup.cs` (OWIN `IAppBuilder`) | `builder.Services` + `app.Use*` ミドルウェア |
| `App_Start/RouteConfig.cs` | Blazor ルーティング (`@page` ディレクティブ) |
| `App_Start/FilterConfig.cs` | `builder.Services.AddControllersWithViews()` のグローバルフィルター登録 |
| `App_Start/BundleConfig.cs` | 削除（バンドル不要：wwwroot の静的ファイルを直接参照） |
| `App_Start/WebApiConfig.cs` | `Program.cs` で CORS ポリシー設定 |

**新 `Program.cs` の主要設定:**
```csharp
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddControllers();            // Web API
builder.Services.AddSignalR();               // ASP.NET Core SignalR
builder.Services.AddDbContext<TacosDbContext>(...);
builder.Services.AddSession();               // カート用セッション
builder.Services.AddDistributedMemoryCache();
builder.Services.AddCors(opt => { /* AdminAppOrigin */ });
// ...
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();
app.MapHub<OrderStatusHub>("/hubs/orderstatus");
```

---

### Phase 3 — データレイヤー (EF6 → EF Core 10)

**目標:** `System.Data.Entity` から `Microsoft.EntityFrameworkCore` へ移行

| 変更箇所 | 内容 |
|---------|------|
| `TacosDbContext` の基底クラス | `System.Data.Entity.DbContext` → `Microsoft.EntityFrameworkCore.DbContext` |
| コンストラクター | `base("name=TacosDb")` → `DbContextOptions<TacosDbContext>` を受け取る形式 |
| `OnModelCreating` | `DbModelBuilder` → `ModelBuilder`、`HasPrecision` の構文変更 |
| `DbSet<T>.Include(string)` | 文字列指定 → ラムダ式 (`Include(o => o.Items)`) |
| `ThenInclude` | EF6 の連鎖 `Include` → EF Core の `ThenInclude` |
| 接続文字列 | `Web.config` `connectionStrings` → `appsettings.json` |
| Migrations | EF6 Migrations フォルダー削除、`dotnet ef migrations add InitialCreate` で再作成 |
| `System.Data.Entity` 参照 | 全 using を `Microsoft.EntityFrameworkCore` に更新 |

---

### Phase 4 — サービス層

**目標:** `System.Web` 依存を除去し、DI 対応サービスに改修

#### CartService (`CartService.cs`)
- `HttpSessionStateBase` → `ISession` (`Microsoft.AspNetCore.Http.ISession`)
- コンストラクター引数を `IHttpContextAccessor` 経由に変更 (または Blazor の cascading state / `ProtectedSessionStorage`)
- セッションへの保存：`ISession.SetString` / `ISession.GetString`
- `Newtonsoft.Json` はそのまま使用可

#### OrderService (`OrderService.cs`)
- `TacosDbContext` を DI で注入（コンストラクター注入）
- `new TacosDbContext()` パターンを廃止

---

### Phase 5 — Web API (Web API 2 → ASP.NET Core Web API)

**目標:** `ApiController` を `ControllerBase` に移行

| 変更箇所 | 内容 |
|---------|------|
| 基底クラス | `ApiController` → `ControllerBase` + `[ApiController]` 属性 |
| ルーティング | `[RoutePrefix]` → `[Route]` (クラスレベル)、`[HttpGet,Route]` 統合可 |
| 戻り値 | `IHttpActionResult` → `IActionResult` (または `ActionResult<T>`) |
| `Ok(dto)` / `NotFound()` / `BadRequest(msg)` | 同名メソッド (`ControllerBase`) で互換 |
| `Request.CreateErrorResponse` | `Problem()` または `StatusCode(500, msg)` |
| `TacosDbContext` | DI コンストラクター注入 |
| `IHubContext<OrderStatusHub>` | SignalR Hub へのサーバー側プッシュに `IHubContext<T>` を DI で使用 |
| CORS | `[EnableCors("AdminPolicy")]` 属性 (`Microsoft.AspNetCore.Cors`) |
| ApiKeyAuthFilter | `IAuthorizationFilter` ベースの ASP.NET Core 属性フィルターに書き直し |
| ファイル移動 | `Api/` フォルダー → `Controllers/` フォルダーに統合（または維持） |

---

### Phase 6 — SignalR Hub (SignalR 2 → ASP.NET Core SignalR)

**目標:** `Microsoft.AspNet.SignalR` を `Microsoft.AspNetCore.SignalR` に置き換え

| 変更箇所 | 内容 |
|---------|------|
| 基底クラス | `Microsoft.AspNet.SignalR.Hub` → `Microsoft.AspNetCore.SignalR.Hub` |
| `Groups.Add(...).Wait()` | `await Groups.AddToGroupAsync(Context.ConnectionId, groupName)` |
| `Groups.Remove(...).Wait()` | `await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName)` |
| `Context.ConnectionId` | 同じプロパティ名で利用可 |
| Hub メソッド | 非同期 `async Task` に変更 |
| クライアント JS | `@microsoft/signalr` npm パッケージ (CDN 利用可)、接続 URL を `/hubs/orderstatus` に更新 |
| OWIN マッピング | `app.MapSignalR()` 削除 → `Program.cs` で `app.MapHub<OrderStatusHub>(...)` |

---

### Phase 7 — MVC コントローラー → Blazor ページ

**目標:** `System.Web.Mvc.Controller` 派生クラスを Blazor コンポーネント + API に分解

各 MVC コントローラーを以下のパターンで置き換え：

| MVC コントローラー | Blazor 置き換え |
|------------------|----------------|
| `HomeController.Index()` | `Pages/Index.razor` (DI で `TacosDbContext` または `OrderService` を注入) |
| `OrderController.Cart()` | `Pages/Order/Cart.razor` (カート状態は `CartService` 経由) |
| `OrderController.Checkout()` | `Pages/Order/Checkout.razor` |
| `OrderController.Confirm()` | `Pages/Order/Confirm.razor` |
| `OrderController.Complete()` | `Pages/Order/Complete.razor` |
| `StatusController.Index()` | `Pages/Status/Index.razor` (SignalR 接続を Blazor から確立) |
| `StatusController.NotFound()` | `Pages/Status/NotFound.razor` |
| AJAX (`AddToCart`, `RemoveFromCart` 等) | Blazor のイベントハンドラー (`@onclick`, `EventCallback`) |
| フォーム送信 | `EditForm` + `DataAnnotationsValidator` |
| AJAX JSON レスポンス | Blazor コンポーネント内の直接状態更新 |

**方針:** `System.Web.Mvc.JsonResult` / `ViewResult` / `RedirectToRouteResult` を Blazor の NavigationManager + 状態管理で置き換え。

---

### Phase 8 — ビュー → Blazor コンポーネント

**目標:** `.aspx` / `.cshtml` → `.razor` への変換

#### 削除するファイル
- `Views/` フォルダー全体 (`.aspx`, `.cshtml`, `_ViewStart.cshtml`, `Web.config`)
- `Views/Shared/Site.master`
- `Views/Shared/_Layout.cshtml`

#### 新規作成する Blazor ファイル

| ファイル | 内容 |
|---------|------|
| `Components/App.razor` | ルートコンポーネント |
| `Components/Routes.razor` | ルーターコンポーネント |
| `Components/Layout/MainLayout.razor` | `_Layout.cshtml` の代替 |
| `Components/Layout/NavMenu.razor` | ナビゲーション |
| `Pages/Index.razor` | ホーム（メニュー一覧） |
| `Pages/Order/Cart.razor` | カートページ |
| `Pages/Order/Checkout.razor` | チェックアウト |
| `Pages/Order/Confirm.razor` | 注文確認 |
| `Pages/Order/Complete.razor` | 注文完了 |
| `Pages/Status/Index.razor` | 注文ステータス（SignalR リアルタイム更新） |
| `Pages/Error.razor` | エラーページ |

#### Razor 変換ポイント
- `Html.AntiForgeryToken()` → Blazor は CSRF 保護を内蔵（フォームに自動付与）
- `Html.ActionLink(...)` → `<NavLink href="...">` または `<a href="...">`
- `Html.BeginForm(...)` → `<EditForm>` コンポーネント
- `Url.Action(...)` → ハードコードパス または `NavigationManager.GetUriWithQueryParameters`
- `ViewBag` / `ViewData` → コンポーネントのパラメーター / カスケーディングパラメーター
- `@Scripts.Render(...)` / `@Styles.Render(...)` → `<script>` / `<link>` タグを直接 `MainLayout.razor` または `App.razor` に記述
- SignalR クライアント接続 → `Status/Index.razor` の `OnAfterRenderAsync` で `HubConnectionBuilder` を使用

---

### Phase 9 — 設定とインフラストラクチャ

**目標:** `Web.config` 依存を `appsettings.json` + 環境変数に移行

| 変更箇所 | 内容 |
|---------|------|
| `Web.config` 接続文字列 | `appsettings.json` の `ConnectionStrings:TacosDb` に移動 |
| `AppSettings["AdminAppOrigin"]` | `appsettings.json` の `App:AdminAppOrigin` に移動 |
| `AppSettings["ApiKey"]` | 環境変数または User Secrets / Azure Key Vault 推奨 |
| `ConfigurationManager.AppSettings` | `IConfiguration["App:AdminAppOrigin"]` に置き換え |
| `Web.Debug.config` / `Web.Release.config` | `appsettings.Development.json` / `appsettings.Production.json` に移行 |

**セキュリティ注意:** `ApiKey` は `appsettings.json` にコミットしない。開発時は `dotnet user-secrets`、本番環境では環境変数または Key Vault を使用。

---

### Phase 10 — 静的ファイルとクライアントサイド

**目標:** `Content/` / `Scripts/` を `wwwroot/` に再配置

| 変更 | 内容 |
|-----|------|
| `Content/` → `wwwroot/css/` | CSS ファイルの移動 |
| `Scripts/` → `wwwroot/js/` | JS ファイルの移動 |
| `@microsoft/signalr` クライアント | CDN リンクを `App.razor` または `MainLayout.razor` に追加 |
| Bootstrap | `wwwroot/lib/bootstrap/` または CDN |
| jQuery | Blazor では不要な場合が多い。残す場合は `wwwroot/lib/jquery/` |
| BundleConfig / WebGrease / Antlr | 完全削除（バンドル不要）|

---

### Phase 11 — クリーンアップと検証

| 作業 | 内容 |
|-----|------|
| 削除: `Global.asax` / `Startup.cs` | 置き換え済み |
| 削除: `App_Start/` フォルダー | 全ファイル |
| 削除: `Views/` フォルダー | 全 `.aspx` / `.cshtml` |
| 削除: `Web.config` / `Web.*.config` | `appsettings*.json` に移行済み |
| 削除: `packages.config` | `PackageReference` に移行済み |
| 削除: `Properties/AssemblyInfo.cs` | SDK スタイル csproj では自動生成 |
| `Data/Migrations/` | EF6 マイグレーション削除、EF Core で再作成 |
| ビルド確認 | `dotnet build TacosApp.sln` でエラー 0 |
| 動作確認 | `dotnet run --project TacosApp.Web` で基本フローの動作確認 |

---

## リスクと対策

| リスク | 対策 |
|-------|------|
| セッション管理 (CartService) | Blazor Server はサーバーサイドステートを保持するため、`IHttpContextAccessor` + `ISession` は利用可能。ただし Blazor Server のコンポーネントライフサイクルに注意（接続切断でセッションが失われる可能性）。本番では Redis セッションを推奨。 |
| SignalR の HTTP コンテキスト | Hub メソッド内での `HttpContext` アクセスは制限あり。`IHubContext<T>` を DI でサービスに注入して使用。 |
| EF Core マイグレーション再作成 | 既存 DB スキーマを保持するには `--ignore-changes` フラグで空の初期マイグレーションを作成し、既存 DB に適用済みとしてマーク。 |
| JavaScript との相互運用 | SignalR クライアントコード (status ページ) は Blazor の JS Interop (`IJSRuntime`) または Blazor の `HubConnection` クラスを使って置き換え。 |
| CORS (AdminAppOrigin) | `TacosApp.Admin` が `/api/orders` を呼ぶため、本番環境のオリジン設定を確実に行うこと。 |

---

## 完了基準

- [ ] `dotnet build TacosApp.sln` がエラー 0 で成功
- [ ] メニュー表示（`/`）が正常動作
- [ ] カート追加・削除が正常動作
- [ ] 注文フロー（Checkout → Confirm → Complete）が正常動作
- [ ] 注文ステータスページで SignalR によるリアルタイム更新が動作
- [ ] `api/orders` エンドポイントが API キー認証付きで動作
- [ ] `TacosApp.Admin` からの CORS リクエストが正常に処理される
- [ ] `TacosApp.Admin` との SignalR 通知連携が動作

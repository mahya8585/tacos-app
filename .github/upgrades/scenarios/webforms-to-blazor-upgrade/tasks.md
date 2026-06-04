# TacosApp.Web — 移行タスク一覧

## Phase 1: プロジェクトインフラストラクチャ

- [ ] **TASK-001** `TacosApp.Web.csproj` を SDK スタイルに変換する
  - `<ProjectTypeGuids>` / `<HintPath>` / 旧形式の `<Compile>` `<Content>` タグを削除
  - `<Project Sdk="Microsoft.NET.Sdk.Web">` に変更
  - `<TargetFramework>net10.0</TargetFramework>` に変更
  - `<Nullable>enable</Nullable>` `<ImplicitUsings>enable</ImplicitUsings>` を追加

- [ ] **TASK-002** `packages.config` を `<PackageReference>` に移行する
  - `packages.config` を削除
  - 以下の旧パッケージを `<PackageReference>` から除去:
    `Microsoft.AspNet.Mvc`, `Microsoft.AspNet.WebApi.*`, `Microsoft.AspNet.SignalR.*`,
    `Microsoft.Owin.*`, `Owin`, `Microsoft.Web.Infrastructure`, `System.Web.Optimization`,
    `WebGrease`, `Antlr`, `EntityFramework`, `jQuery`, `bootstrap`
  - 以下の新パッケージを追加:
    `Microsoft.EntityFrameworkCore.SqlServer` (10.x),
    `Microsoft.EntityFrameworkCore.Tools` (10.x),
    `Newtonsoft.Json` (13.x)

- [ ] **TASK-003** `TacosApp.sln` のプロジェクト参照を確認・修正する

---

## Phase 2: アプリケーション エントリポイント

- [ ] **TASK-004** `Global.asax` / `Global.asax.cs` を削除する

- [ ] **TASK-005** `Startup.cs` (OWIN) を削除する

- [ ] **TASK-006** `App_Start/BundleConfig.cs` を削除する

- [ ] **TASK-007** `App_Start/RouteConfig.cs` を削除する

- [ ] **TASK-008** `App_Start/FilterConfig.cs` を削除する (存在する場合)

- [ ] **TASK-009** `App_Start/WebApiConfig.cs` を削除する

- [ ] **TASK-010** `Program.cs` を新規作成する (ASP.NET Core + Blazor Server)
  - `AddRazorComponents().AddInteractiveServerComponents()`
  - `AddControllers()` (Web API 用)
  - `AddSignalR()` (ASP.NET Core SignalR)
  - `AddDbContext<TacosDbContext>(...)` (接続文字列を `appsettings.json` から読み取り)
  - `AddSession()` + `AddDistributedMemoryCache()`
  - `AddCors()` (AdminAppOrigin 設定)
  - `MapRazorComponents<App>().AddInteractiveServerRenderMode()`
  - `MapControllers()`
  - `MapHub<OrderStatusHub>("/hubs/orderstatus")`
  - `MapStaticAssets()` / `UseStaticFiles()`

---

## Phase 3: データレイヤー (EF6 → EF Core 10)

- [ ] **TASK-011** `Data/TacosDbContext.cs` を EF Core に書き換える
  - `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
  - 基底クラス: `System.Data.Entity.DbContext` → `Microsoft.EntityFrameworkCore.DbContext`
  - コンストラクター: `base("name=TacosDb")` → `DbContextOptions<TacosDbContext>` を受け取る形式
  - `OnModelCreating(DbModelBuilder)` → `OnModelCreating(ModelBuilder)`
  - `HasPrecision(10, 0)` → EF Core の `HasPrecision(10, 0)` (構文は互換だが名前空間が変わる)
  - `IsRequired()` / `HasMaxLength()` はそのまま利用可

- [ ] **TASK-012** `Data/Migrations/` の EF6 マイグレーションを削除し、EF Core で再作成する
  - 既存マイグレーションフォルダーをバックアップ (削除は確認後)
  - `dotnet ef migrations add InitialCreate --project TacosApp.Web`
  - 既存 DB がある場合: `dotnet ef database update` または `--ignore-changes` パターンを使用

- [ ] **TASK-013** ドメインモデルの `using System.Data.Entity` を更新する
  - `Models/Domain/` 内の全モデルクラスを確認し、EF6 固有の属性を EF Core 等価物に変換

---

## Phase 4: サービス層

- [ ] **TASK-014** `Services/CartService.cs` を ASP.NET Core 対応に書き換える
  - `using System.Web` を削除
  - `HttpSessionStateBase` → `ISession` (`Microsoft.AspNetCore.Http`)
  - コンストラクター引数を `ISession` に変更
  - `_session["key"]` → `_session.GetString("key")` / `_session.SetString("key", value)`
  - DI 登録: `services.AddScoped<CartService>()` (Program.cs で)

- [ ] **TASK-015** `Services/OrderService.cs` を DI 対応に書き換える
  - `new TacosDbContext()` パターンを廃止
  - コンストラクターで `TacosDbContext` を受け取る
  - DI 登録: `services.AddScoped<OrderService>()`

---

## Phase 5: Web API (Web API 2 → ASP.NET Core Web API)

- [ ] **TASK-016** `Filters/ApiKeyAuthFilter.cs` を ASP.NET Core 対応に書き換える
  - `using System.Web.Http.*` を削除
  - `AuthorizationFilterAttribute` (Web API) → `IAuthorizationFilter` または `Attribute` + `IActionFilter` (ASP.NET Core)
  - `HttpActionContext` → `AuthorizationFilterContext` / `ActionExecutingContext`
  - `actionContext.Request.CreateErrorResponse(...)` → `context.Result = new ObjectResult(...) { StatusCode = ... }`
  - `ConfigurationManager.AppSettings["ApiKey"]` → コンストラクター DI (`IConfiguration`)

- [ ] **TASK-017** `Api/OrdersApiController.cs` を ASP.NET Core Web API に書き換える
  - `using System.Web.Http` → `using Microsoft.AspNetCore.Mvc`
  - 基底クラス: `ApiController` → `ControllerBase` + `[ApiController]` 属性
  - `[RoutePrefix("api/orders")]` → `[Route("api/orders")]` (クラスレベル)
  - `IHttpActionResult` → `IActionResult`
  - `Request.CreateErrorResponse(...)` → `Problem(...)` または `StatusCode(500, ...)`
  - `TacosDbContext` をコンストラクターで DI 注入
  - `IHubContext<OrderStatusHub>` をコンストラクターで DI 注入
  - SignalR グループへのプッシュ: `_hub.Clients.Group(orderNumber).SendAsync("statusUpdated", ...)`
  - `[EnableCors]` 属性 (`Microsoft.AspNetCore.Cors`) を追加

---

## Phase 6: SignalR Hub (SignalR 2 → ASP.NET Core SignalR)

- [ ] **TASK-018** `Hubs/OrderStatusHub.cs` を ASP.NET Core SignalR に書き換える
  - `using Microsoft.AspNet.SignalR` → `using Microsoft.AspNetCore.SignalR`
  - 基底クラス: `Microsoft.AspNet.SignalR.Hub` → `Microsoft.AspNetCore.SignalR.Hub`
  - `public void JoinOrderGroup(string orderNumber)` → `public async Task JoinOrderGroup(string orderNumber)`
  - `Groups.Add(Context.ConnectionId, orderNumber).Wait()` → `await Groups.AddToGroupAsync(Context.ConnectionId, orderNumber)`
  - `Groups.Remove(Context.ConnectionId, orderNumber).Wait()` → `await Groups.RemoveFromGroupAsync(Context.ConnectionId, orderNumber)`

---

## Phase 7: MVC コントローラーの変換準備

- [ ] **TASK-019** `Controllers/HomeController.cs` のロジックを `Pages/Index.razor` に移植する
  - DB クエリロジックを `@code` ブロックに移植
  - `TacosDbContext` を `@inject` で DI 注入
  - `[Inject]` 属性または `@inject` ディレクティブを使用
  - `HomeController.cs` を削除

- [ ] **TASK-020** `Controllers/OrderController.cs` のロジックを Blazor ページに移植する
  - `Cart()` → `Pages/Order/Cart.razor`
  - `AddToCart()` / `RemoveFromCart()` / `UpdateQuantity()` → Blazor イベントハンドラーに変換
  - `Checkout()` / `PostCheckout()` → `Pages/Order/Checkout.razor`
  - `Confirm()` / `PostConfirm()` → `Pages/Order/Confirm.razor`
  - `Complete()` → `Pages/Order/Complete.razor`
  - `CartService` を `@inject` で DI 注入
  - セッション: `IHttpContextAccessor` を介して `ISession` にアクセス
  - `OrderController.cs` を削除

- [ ] **TASK-021** `Controllers/StatusController.cs` のロジックを Blazor ページに移植する
  - `Index()` → `Pages/Status/Index.razor` (SignalR クライアント接続を含む)
  - `NotFound()` → `Pages/Status/NotFound.razor`
  - `StatusController.cs` を削除

---

## Phase 8: ビュー → Blazor コンポーネント

- [ ] **TASK-022** Blazor アプリ基盤コンポーネントを作成する
  - `Components/App.razor` (ルートコンポーネント)
  - `Components/Routes.razor` (Router コンポーネント)
  - `Components/Layout/MainLayout.razor` (`_Layout.cshtml` の代替)
  - `Components/Layout/NavMenu.razor` (ナビゲーション)
  - `_Imports.razor`

- [ ] **TASK-023** `Pages/Index.razor` (ホーム画面 — メニュー一覧) を作成する
  - `@page "/"` ディレクティブ
  - メニュー一覧とトッピング一覧を表示
  - カートに追加するフォーム / ボタン

- [ ] **TASK-024** `Pages/Order/Cart.razor` (カート画面) を作成する
  - `@page "/order/cart"`
  - カート内アイテム一覧表示
  - 数量変更・削除ボタン
  - 小計表示

- [ ] **TASK-025** `Pages/Order/Checkout.razor` (チェックアウト) を作成する
  - `@page "/order/checkout"`
  - `EditForm` で顧客情報入力フォーム
  - バリデーション

- [ ] **TASK-026** `Pages/Order/Confirm.razor` (注文確認) を作成する
  - `@page "/order/confirm"`
  - 注文内容確認表示
  - 確定ボタン

- [ ] **TASK-027** `Pages/Order/Complete.razor` (注文完了) を作成する
  - `@page "/order/complete"`
  - 注文番号表示
  - ステータスページへのリンク

- [ ] **TASK-028** `Pages/Status/Index.razor` (注文ステータス) を作成する
  - `@page "/status/{orderNumber}"`
  - `OnAfterRenderAsync` で ASP.NET Core SignalR クライアント接続
  - `HubConnectionBuilder` を使って `/hubs/orderstatus` に接続
  - `statusUpdated` イベントを受信してステータス表示を更新
  - `IDisposable` を実装してコンポーネント破棄時に接続を切断

- [ ] **TASK-029** `Pages/Status/NotFound.razor` (注文未発見) を作成する
  - `@page "/status/notfound"`

- [ ] **TASK-030** 旧 `Views/` フォルダーを削除する
  - `Views/Home/Index.aspx`, `Index.cshtml`
  - `Views/Order/Cart.aspx/cshtml`, `Checkout.aspx/cshtml`, `Complete.aspx/cshtml`, `Confirm.aspx/cshtml`
  - `Views/Status/Index.aspx/cshtml`, `NotFound.aspx/cshtml`
  - `Views/Shared/_Layout.cshtml`, `Site.master`, `Error.cshtml`
  - `Views/_ViewStart.cshtml`, `Views/Web.config`

---

## Phase 9: 設定

- [ ] **TASK-031** `appsettings.json` に設定値を移行する
  ```json
  {
    "ConnectionStrings": {
      "TacosDb": "<SQL Server 接続文字列>"
    },
    "App": {
      "AdminAppOrigin": "http://localhost:8080"
    }
  }
  ```
  - `ApiKey` は `appsettings.json` に含めず、環境変数 `App__ApiKey` または User Secrets で管理

- [ ] **TASK-032** `appsettings.Development.json` を作成/更新する
  - ローカル開発用接続文字列
  - `App:AdminAppOrigin` 開発用オリジン

- [ ] **TASK-033** `Web.config` / `Web.Debug.config` / `Web.Release.config` を削除する

- [ ] **TASK-034** コード内の `ConfigurationManager.AppSettings` 参照を `IConfiguration` に更新する
  - `ApiKeyAuthFilter.cs` (`IConfiguration` DI)
  - `WebApiConfig.cs` (削除済みのため対応不要)

---

## Phase 10: 静的ファイルとクライアントサイド

- [ ] **TASK-035** `Content/` の CSS ファイルを `wwwroot/css/` に移動する

- [ ] **TASK-036** `Scripts/` の JS ファイルを `wwwroot/js/` に移動する
  - 不要な jQuery プラグイン / Bundle は削除

- [ ] **TASK-037** `wwwroot/index.html` または `App.razor` に外部ライブラリの参照を追加する
  - Bootstrap CDN または `wwwroot/lib/bootstrap/`
  - `@microsoft/signalr` CDN: `https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.x/signalr.min.js`

- [ ] **TASK-038** `wwwroot/js/status.js` を作成し SignalR クライアントコードを実装する (オプション)
  - Blazor JS Interop パターンまたは純粋な Blazor C# パターン (`HubConnectionBuilder` in C#) のどちらかを選択

---

## Phase 11: クリーンアップと検証

- [ ] **TASK-039** `Properties/AssemblyInfo.cs` を削除する (SDK スタイル csproj では自動生成)

- [ ] **TASK-040** `dotnet build TacosApp.sln` を実行してビルドエラーを確認する

- [ ] **TASK-041** EF Core マイグレーションを適用してスキーマを確認する

- [ ] **TASK-042** 全ページの動作を手動確認する
  - ホーム（メニュー一覧）
  - カート操作（追加・削除・数量変更）
  - 注文フロー（Checkout → Confirm → Complete）
  - 注文ステータス（SignalR リアルタイム更新）
  - Web API (`/api/orders`) の認証付き動作確認

- [ ] **TASK-043** `TacosApp.Admin` との連携を確認する
  - CORS 設定が正しく機能することを確認
  - SignalR プッシュ通知が `TacosApp.Admin` からトリガーされることを確認

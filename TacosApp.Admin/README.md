# TacosApp.Admin（管理者サイト・.NET 10）

TacosApp 管理者向け Web サイト。既存の `TacosApp.Web`（消費者向け / .NET Framework 4.8）と
**同じ SQL Server データベース `TacosDb` を共有**しつつ、新世代のアーキテクチャで実装しています。

このプロジェクトの目的は **新旧アーキテクチャの比較サンプル** を提供することです。

---

## アーキテクチャ比較

| 項目 | TacosApp.Web（旧） | TacosApp.Admin（新） |
| --- | --- | --- |
| ランタイム | .NET Framework 4.8（IIS / `System.Web`） | .NET 10（Kestrel / `Microsoft.AspNetCore`） |
| プロジェクト形式 | 旧 csproj（大量の `<Compile Include=...>`） | SDK スタイル（暗黙的 globbing） |
| 依存管理 | `packages.config`（NuGet v2 形式） | `<PackageReference>`（PackageReference 形式） |
| Web フレームワーク | ASP.NET MVC 5（Controller + Views） | ASP.NET Core Razor Pages（PageModel） |
| 起動 | `Global.asax.cs` + `Startup.cs`（OWIN） | `Program.cs`（トップレベルステートメント / Minimal Hosting） |
| 依存性注入 | なし（手動 `new`） | 組み込み DI コンテナ |
| ORM | Entity Framework 6（同期主体） | EF Core 10（async-first） |
| API | ASP.NET Web API 2（`ApiController`） | Minimal API（`MapGroup` / `MapPut`） |
| リアルタイム通信 | SignalR 2（OWIN） | SignalR Core（`MapHub`） |
| 認証 | API キーフィルタ（`ActionFilterAttribute`） | ASP.NET Core Identity（Cookie + パスワード） |
| 設定 | `Web.config` + `<appSettings>` | `appsettings.json` + IOptions |
| C# バージョン | C# 7 系（推定） | C# 13（primary constructor / collection expressions / `required` 等） |
| ファイル構成 | View / Controller / ApiController を分離 | Razor Pages（.cshtml + .cshtml.cs を 1 ペアで配置） |

---

## 主要技術スタック

- **.NET 10** (SDK 10.0.204)
- **ASP.NET Core 10**
  - Razor Pages（管理画面 UI）
  - Minimal API（`/api/admin/*`）
  - SignalR Core（リアルタイム注文ステータス配信）
  - ASP.NET Core Identity（Cookie 認証 / `AdminUser : IdentityUser`）
- **Entity Framework Core 10**（SQL Server プロバイダ）
- **C# 13 言語機能**
  - Primary constructor（`class OrderService(...)`）
  - Collection expression（`IReadOnlyList<X> Items = [];`）
  - `required` / `init` プロパティ
  - File-scoped namespace
  - `sealed record` DTO

---

## ディレクトリ構成

```
TacosApp.Admin/
├── Program.cs                  # トップレベル：DI / ミドルウェア / マッピング / シード
├── appsettings.json            # ConnectionString / AdminSeed
├── Models/                     # ドメインモデル（EF6 と同じスキーマにマップ）
│   ├── AdminUser.cs            # IdentityUser を継承
│   ├── Menu.cs / Topping.cs
│   ├── Order.cs / OrderItem.cs / OrderItemTopping.cs
│   └── OrderStatus.cs
├── Data/
│   ├── AdminDbContext.cs       # IdentityDbContext<AdminUser>
│   └── Migrations/             # __EFMigrationsHistoryAdmin（EF6 と独立）
├── Services/                   # ビジネスロジック層（DI で注入）
│   ├── OrderService.cs         # 注文 + SignalR ブロードキャスト
│   ├── MenuService.cs
│   └── ToppingService.cs
├── Hubs/AdminOrderHub.cs       # `[Authorize]` SignalR Hub
├── Infrastructure/IdentitySeeder.cs  # 初回管理者ユーザー作成
├── Pages/
│   ├── _ViewStart.cshtml
│   ├── Shared/_Layout.cshtml   # ダーク基調ナビゲーション
│   ├── Index.cshtml(.cs)       # ダッシュボード（統計 + 最近の注文）
│   ├── Orders/
│   │   ├── Index.cshtml(.cs)   # 一覧 + ステータスフィルタ + ライブ更新
│   │   └── Details.cshtml(.cs) # 詳細 + ステータス更新 + ライブ反映
│   ├── Menus/Index|Create|Edit|Delete.cshtml(.cs)
│   └── Toppings/Index|Create|Edit|Delete.cshtml(.cs)
└── Areas/Identity/             # Identity UI（既定スキャフォールド）
```

---

## データベースについて

- 接続先：`Server=.\SQLEXPRESS;Database=TacosDb;Integrated Security=True;...`
- ドメインテーブル（`Menus` / `Toppings` / `Orders` / `OrderItems` / `OrderItemToppings`）は
  既存の `TacosApp.Web`（EF6）が管理しています。`AdminDbContext` はそれらを **既存テーブルとしてマップのみ**します。
- EF Core 側のマイグレーションは **Identity テーブル（`AspNet*`）のみ** を作成します。
- マイグレーション履歴テーブル名を `__EFMigrationsHistoryAdmin` に変更し、EF6 の
  `__MigrationHistory` と衝突しないようにしています。

---

## セットアップ

### 1. 復元 & ビルド

```powershell
dotnet restore TacosApp.Admin\TacosApp.Admin.csproj
dotnet build   TacosApp.Admin\TacosApp.Admin.csproj
```

### 2. データベースマイグレーション適用（Identity テーブル作成）

```powershell
# dotnet-ef ツール（未インストールの場合）
dotnet tool install --global dotnet-ef --version 10.0.*

dotnet ef database update --project TacosApp.Admin\TacosApp.Admin.csproj
```

> アプリ起動時にも `db.Database.MigrateAsync()` が走るため、手動適用は任意です。

### 3. 起動

```powershell
dotnet run --project TacosApp.Admin\TacosApp.Admin.csproj
```

### 4. 初期管理者アカウント

`appsettings.json` の `AdminSeed` セクションを使用して初回起動時に自動作成されます。

- **Email**: `admin@tacos.local`
- **Password**: `Admin#12345`

> 本番運用時は必ず別の安全な値に変更してください。

---

## エンドポイント一覧

### Razor Pages（UI）

| URL | 説明 |
| --- | --- |
| `/` | ダッシュボード（注文統計 + 最近の注文） |
| `/Orders` | 注文一覧（ステータスフィルタ / SignalR ライブ更新） |
| `/Orders/Details/{id}` | 注文詳細 + ステータス更新 |
| `/Menus` `/Menus/Create` `/Menus/Edit/{id}` `/Menus/Delete/{id}` | メニュー CRUD |
| `/Toppings` `/Toppings/Create` `/Toppings/Edit/{id}` `/Toppings/Delete/{id}` | トッピング CRUD |
| `/Identity/Account/Login` | ログイン |

### Minimal API（`/api/admin/*`、要認証）

| メソッド | URL | 説明 |
| --- | --- | --- |
| `PUT` | `/api/admin/orders/{id}/status` | 注文ステータス更新（body: `{ "status": 0..3 }`） |
| `GET` | `/api/admin/orders/stats` | 統計情報を取得 |

### SignalR Hub

| パス | 説明 |
| --- | --- |
| `/hubs/admin-orders` | 認証必須。サーバーから `orderStatusChanged` イベントを配信 |

---

## ライセンス

ルートの `LICENSE` を参照。

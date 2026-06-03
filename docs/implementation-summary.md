# 実装サマリー

このドキュメントは、今回の一連の改修で実施した内容、検証方法、今後触るときの注意点をまとめたものです。

## 全体像

- 消費者向けサイト: `TacosApp.Web`（ASP.NET MVC 5 / .NET Framework 4.8）
- 管理者向けサイト: `TacosApp.Admin`（ASP.NET Core / .NET 10）
- 共有DB: SQL Server LocalDB `TacosDb`
- 消費者向けURL: `http://localhost:5081/`
- 管理者向けURL: `http://localhost:5069/`

## 消費者向けサイトの変更

### Web Forms ビュー化

消費者向けサイトの主要ビューを Web Forms 形式へ移行しました。

- `Views/Shared/Site.master`
- `Views/Home/Index.aspx`
- `Views/Order/Cart.aspx`
- `Views/Order/Checkout.aspx`
- `Views/Order/Confirm.aspx`
- `Views/Order/Complete.aspx`
- `Views/Status/Index.aspx`
- `Views/Status/NotFound.aspx`

`Global.asax.cs` では Web Forms view engine を優先し、既存 Razor ビューは fallback として残しています。

### 文字化け対応

Web Forms 化後に日本語が文字化けしたため、`TacosApp.Web/Web.config` の `<globalization>` で UTF-8 を明示しました。

- `requestEncoding="utf-8"`
- `responseEncoding="utf-8"`
- `fileEncoding="utf-8"`

各 `.aspx` ページ側も `ResponseEncoding="utf-8"` に揃えています。

### 静的アセット

Bootstrap、jQuery、サイトCSS、商品画像を物理ファイルとして配信できるように整理しました。商品画像は SVG アイコンではなく JPEG 写真へ差し替え済みです。

画像配置先:

- `TacosApp.Web/Content/images/*.jpg`

商品画像は `Menus.ImageUrl` から参照されます。DBの値が古い場合は `tools/seed-database.ps1` を再実行してください。

## メニューと価格

現在のシードメニューは8品です。

| ID | 商品 | 価格 | 画像 |
| --- | --- | ---: | --- |
| 1 | クラシックビーフタコス | 350円 | `/Content/images/beef-taco.jpg` |
| 2 | チキンタコス | 320円 | `/Content/images/chicken-taco.jpg` |
| 3 | シュリンプタコス | 380円 | `/Content/images/shrimp-taco.jpg` |
| 4 | ベジタコス | 300円 | `/Content/images/veg-taco.jpg` |
| 5 | チーズケサディーヤ | 560円 | `/Content/images/quesadilla.jpg` |
| 6 | メキシカンナチョス | 260円 | `/Content/images/nachos.jpg` |
| 7 | ライムソーダ | 270円 | `/Content/images/lime-soda.jpg` |
| 8 | マンゴーラッシー | 440円 | `/Content/images/mango-lassi.jpg` |

価格変更履歴:

- チーズケサディーヤ: 280円から560円へ変更
- ライムソーダ: 180円から270円へ変更
- マンゴーラッシー: 220円から440円へ変更

主な定義元:

- `tools/seed-database.ps1`
- `TacosApp.Web/Data/Migrations/Configuration.cs`

## 管理者向けサイトの変更

`TacosApp.Admin` は管理者向けの .NET 10 アプリとして追加・整備されています。

主な機能:

- 管理者ログイン
- ダッシュボード
- 注文一覧と注文詳細
- 注文ステータス更新
- メニューCRUD
- トッピングCRUD
- SignalR Core による管理画面のライブ更新

詳細は [../TacosApp.Admin/README.md](../TacosApp.Admin/README.md) を参照してください。

## DB 初期化

LocalDB に `TacosDb` を作成し、メニュー、トッピング、サンプル注文を投入します。

```powershell
powershell -ExecutionPolicy Bypass -File tools\seed-database.ps1
```

DBを作り直す場合:

```powershell
powershell -ExecutionPolicy Bypass -File tools\seed-database.ps1 -Reset
```

現在の投入件数:

- Menus: 8
- Toppings: 6
- Orders: 3
- OrderItems: 4
- OrderItemToppings: 4

## 起動手順

消費者向けサイト:

```powershell
& 'C:\Program Files\IIS Express\iisexpress.exe' /path:"$PWD\TacosApp.Web" /port:5081 /clr:v4.0
```

管理者向けサイト:

```powershell
dotnet run --project TacosApp.Admin\TacosApp.Admin.csproj
```

## 検証手順

消費者向けサイト:

```powershell
powershell -ExecutionPolicy Bypass -File tools\smoke-web.ps1
```

管理者向けサイト:

```powershell
powershell -ExecutionPolicy Bypass -File tools\smoke-admin.ps1
powershell -ExecutionPolicy Bypass -File tools\smoke-admin-login.ps1
```

直近の確認では、消費者向けサイトのスモークテストは全件パスしています。

## 停止手順

IIS Express は実行中ターミナルで `Q` を入力します。プロセスが残った場合は次を実行します。

```powershell
Stop-Process -Name iisexpress -Force -ErrorAction SilentlyContinue
```

管理者向けサイトは実行中ターミナルで `Ctrl+C` を押します。

ポート確認:

```powershell
netstat -ano | Select-String ':5081|:5069'
```

`LISTENING` がなければ停止済みです。`TIME_WAIT` は停止直後に一時的に残ることがあります。

## 注意点

- 商品画像URLはDBに保存されるため、画像ファイルを差し替えただけでは既存DBの参照は変わりません。`tools/seed-database.ps1` を再実行して同期してください。
- ブラウザに古い画像が残る場合は `Ctrl+F5` で再読み込みしてください。
- Web Forms の日本語表示では、`Web.config` の `fileEncoding` とページの `ResponseEncoding` を崩さないでください。
- 写真アセットはローカルデモ用です。本番利用前にはライセンス、利用条件、クレジット要否を確認してください。
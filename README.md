# tacos-app

タコスの注文体験を題材にしたサンプルアプリです。消費者向けサイトと管理者向けサイトが同じ `TacosDb` を共有し、注文、メニュー、トッピング、注文ステータス更新を扱います。

## アプリ構成

| アプリ | 技術 | 既定URL | 役割 |
| --- | --- | --- | --- |
| `TacosApp.Web` | ASP.NET MVC 5 / Web Forms views / .NET Framework 4.8 | `http://localhost:5081/` | 消費者向けメニュー、カート、注文、注文状況確認 |
| `TacosApp.Admin` | ASP.NET Core / Razor Pages / .NET 10 | `http://localhost:5069/` | 管理者向けダッシュボード、注文・メニュー・トッピング管理 |

詳細な管理サイト仕様は [TacosApp.Admin/README.md](TacosApp.Admin/README.md) を参照してください。

## 今回の改修概要

今回の作業では、消費者向けサイトの表示改善、Web Forms 化、商品ラインアップ拡張、管理画面刷新、写真画像への差し替え、価格更新を実施しました。

詳しい変更履歴と運用メモは [docs/implementation-summary.md](docs/implementation-summary.md) にまとめています。

## ローカル実行

### 1. データベース初期化

```powershell
powershell -ExecutionPolicy Bypass -File tools\seed-database.ps1
```

### 2. 消費者向けサイト起動

```powershell
& 'C:\Program Files\IIS Express\iisexpress.exe' /path:"$PWD\TacosApp.Web" /port:5081 /clr:v4.0
```

### 3. 管理者向けサイト起動

```powershell
dotnet run --project TacosApp.Admin\TacosApp.Admin.csproj
```

初期管理者アカウント:

- Email: `admin@tacos.local`
- Password: `Admin#12345`

## 検証

```powershell
powershell -ExecutionPolicy Bypass -File tools\smoke-web.ps1
powershell -ExecutionPolicy Bypass -File tools\smoke-admin.ps1
powershell -ExecutionPolicy Bypass -File tools\smoke-admin-login.ps1
```

## サーバー停止

IIS Express は実行中のターミナルで `Q` を入力するか、必要に応じて次で停止します。

```powershell
Stop-Process -Name iisexpress -Force -ErrorAction SilentlyContinue
```

.NET 管理サイトは `Ctrl+C` で停止します。
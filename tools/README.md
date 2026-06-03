# tools/

ローカル開発・動作確認用のユーティリティスクリプト群です。すべて PowerShell 5.1 で動作します。

## 主要スクリプト

### `seed-database.ps1` — データベース初期化 & ダミーデータ投入（メイン）
LocalDB (`(localdb)\MSSQLLocalDB`) に `TacosDb` データベースを作成し、メニュー・トッピング・サンプル注文 (`SAMPLE-000001`〜`SAMPLE-000003`) を投入する **冪等** スクリプトです。何度実行しても同じ状態に収束します。

```powershell
# 通常実行（不足するレコードのみ追加）
powershell -ExecutionPolicy Bypass -File tools\seed-database.ps1

# サンプル注文のみリセットして再投入
powershell -ExecutionPolicy Bypass -File tools\seed-database.ps1 -Reset

# 接続先カスタマイズ
powershell -ExecutionPolicy Bypass -File tools\seed-database.ps1 -Server '(localdb)\MSSQLLocalDB' -Database 'TacosDb'
```

投入される件数: Menus=4, Toppings=6, Orders=3, OrderItems=4, OrderItemToppings=4

## スモークテスト

### `smoke-web.ps1` — 消費者向け Web (http://localhost:5081)
Home / Cart / Status / 静的 CSS を HTTP で叩いて応答コードと本文を検証します。

### `smoke-admin.ps1` — 管理サイト (http://localhost:5069) 未認証
Root が Login へリダイレクトすること、Login ページが 200 であることを確認します。

### `smoke-admin-login.ps1` — 管理サイト ログインフロー
`admin@tacos.local` / `Admin#12345` でログイン POST し、ダッシュボード・Orders 一覧に `SAMPLE-*` の表示があることまで確認します。

### `smoketest.ps1` — 汎用 HTTP 検証ヘルパー（他スモークから呼ばれる）

## ユーティリティ

### `build-web.ps1` — TacosApp.Web (.NET Framework 4.8) ビルド
VS 2026 同梱の MSBuild を直接呼び、`VSToolsPath` を明示します。

### `add-bom-cshtml.ps1` — `.cshtml` を UTF-8 BOM 付きで保存
日本語を含む Razor ビューが文字化けする場合に実行します。`-DryRun` で対象一覧のみ表示。

## 典型的なローカル実行手順

```powershell
# 1. データベース初期化
powershell -ExecutionPolicy Bypass -File tools\seed-database.ps1

# 2. Admin (.NET 10) を起動
dotnet run --project TacosApp.Admin

# 3. 別ターミナルで Web (.NET Framework 4.8 / IIS Express) を起動
& 'C:\Program Files\IIS Express\iisexpress.exe' /path:"$PWD\TacosApp.Web" /port:5081 /clr:v4.0

# 4. スモーク確認
powershell -ExecutionPolicy Bypass -File tools\smoke-web.ps1
powershell -ExecutionPolicy Bypass -File tools\smoke-admin-login.ps1
```

ブラウザでアクセス:
- 消費者向け: http://localhost:5081/
- 管理サイト: http://localhost:5069/ (`admin@tacos.local` / `Admin#12345`)

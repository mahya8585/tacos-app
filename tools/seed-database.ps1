<#
.SYNOPSIS
    TacosApp 用ローカル DB を作成し、メニュー／トッピング／サンプル注文を投入する再実行可能スクリプト。

.DESCRIPTION
    - 既定の接続先は SQL Server LocalDB ((localdb)\MSSQLLocalDB) の TacosDb。
    - 何度実行しても安全（冪等）。
        * DB が存在しなければ作成。
        * テーブルが無ければ作成（TacosApp.Web の EF6 モデルに合わせたスキーマ）。
        * Menus / Toppings は MERGE で更新。
        * Orders / OrderItems / OrderItemToppings はサンプル分のみ削除→再挿入。
    - -Reset を付けると TacosDb を DROP してから作り直す。

.PARAMETER Server
    SQL Server 接続先（既定: (localdb)\MSSQLLocalDB）

.PARAMETER Database
    データベース名（既定: TacosDb）

.PARAMETER Reset
    既存 DB を破棄してから作り直す。

.EXAMPLE
    .\tools\seed-database.ps1
    既定設定で（再）シードする。

.EXAMPLE
    .\tools\seed-database.ps1 -Reset
    DB を破棄して新規作成し、シードする。
#>
[CmdletBinding()]
param(
    [string]$Server   = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'TacosDb',
    [switch]$Reset
)

$ErrorActionPreference = 'Stop'

function Invoke-Sql {
    param(
        [Parameter(Mandatory)] [string]$DbName,
        [Parameter(Mandatory)] [string]$Query
    )
    # 複雑な T-SQL（マルチライン・引用符・ハイフン等）は -Q 渡しだと PowerShell/CMD の
    # 解釈で壊れるため、UTF-8 BOM 付きで一時ファイルに書き出して -i で渡す。
    $tmp = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "seed-$([Guid]::NewGuid().ToString('N')).sql")
    try {
        $utf8Bom = New-Object System.Text.UTF8Encoding($true)
        [System.IO.File]::WriteAllText($tmp, $Query, $utf8Bom)
        & sqlcmd -S $Server -d $DbName -b -C -f 65001 -i $tmp | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed against $DbName (exit $LASTEXITCODE)" }
    }
    finally {
        if (Test-Path $tmp) { Remove-Item $tmp -Force }
    }
}

Write-Host "==> Server   : $Server"
Write-Host "==> Database : $Database"

# -------------------------------------------------------------------
# 1) (LocalDB のみ) インスタンスを必ず起動
# -------------------------------------------------------------------
if ($Server -match 'localdb') {
    $instance = ($Server -replace '^\(localdb\)\\','')
    Write-Host "==> Ensuring LocalDB instance '$instance' is running..."
    & sqllocaldb start $instance 2>&1 | Out-Null
}

# -------------------------------------------------------------------
# 2) DB の作成（-Reset 指定時は DROP→CREATE）
# -------------------------------------------------------------------
if ($Reset) {
    Write-Host "==> Dropping and recreating database..."
    $resetSql = @"
IF DB_ID(N'$Database') IS NOT NULL
BEGIN
    ALTER DATABASE [$Database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$Database];
END
CREATE DATABASE [$Database];
"@
    Invoke-Sql -DbName 'master' -Query $resetSql
} else {
    $createSql = @"
IF DB_ID(N'$Database') IS NULL
BEGIN
    CREATE DATABASE [$Database];
END
"@
    Invoke-Sql -DbName 'master' -Query $createSql
}

# -------------------------------------------------------------------
# 3) スキーマ作成（TacosApp.Web の EF6 モデルに一致）
# -------------------------------------------------------------------
Write-Host "==> Ensuring schema..."
$schemaSql = @"
SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.Menus', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Menus (
        MenuId        INT             IDENTITY(1,1) NOT NULL,
        Name          NVARCHAR(100)   NOT NULL,
        Description   NVARCHAR(500)   NULL,
        Price         DECIMAL(10,0)   NOT NULL,
        ImageUrl      NVARCHAR(500)   NULL,
        IsAvailable   BIT             NOT NULL,
        DisplayOrder  INT             NOT NULL,
        CONSTRAINT PK_Menus PRIMARY KEY CLUSTERED (MenuId)
    );
END

IF OBJECT_ID(N'dbo.Toppings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Toppings (
        ToppingId     INT             IDENTITY(1,1) NOT NULL,
        Name          NVARCHAR(100)   NOT NULL,
        Price         DECIMAL(10,0)   NOT NULL,
        IsAvailable   BIT             NOT NULL,
        DisplayOrder  INT             NOT NULL,
        CONSTRAINT PK_Toppings PRIMARY KEY CLUSTERED (ToppingId)
    );
END

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        OrderId          INT             IDENTITY(1,1) NOT NULL,
        OrderNumber      NVARCHAR(30)    NOT NULL,
        CustomerName     NVARCHAR(100)   NOT NULL,
        Phone            NVARCHAR(20)    NOT NULL,
        DeliveryAddress  NVARCHAR(500)   NOT NULL,
        DeliveryNote     NVARCHAR(500)   NULL,
        TotalAmount      DECIMAL(10,0)   NOT NULL,
        Status           INT             NOT NULL,
        OrderedAt        DATETIME        NOT NULL,
        UpdatedAt        DATETIME        NOT NULL,
        CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (OrderId)
    );
    CREATE UNIQUE INDEX IX_Orders_OrderNumber ON dbo.Orders(OrderNumber);
END

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        OrderItemId   INT             IDENTITY(1,1) NOT NULL,
        OrderId       INT             NOT NULL,
        MenuId        INT             NOT NULL,
        Quantity      INT             NOT NULL,
        UnitPrice     DECIMAL(10,0)   NOT NULL,
        CONSTRAINT PK_OrderItems PRIMARY KEY CLUSTERED (OrderItemId),
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId)
            REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Menus  FOREIGN KEY (MenuId)
            REFERENCES dbo.Menus(MenuId)
    );
    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
    CREATE INDEX IX_OrderItems_MenuId  ON dbo.OrderItems(MenuId);
END

IF OBJECT_ID(N'dbo.OrderItemToppings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItemToppings (
        OrderItemToppingId INT           IDENTITY(1,1) NOT NULL,
        OrderItemId        INT           NOT NULL,
        ToppingId          INT           NOT NULL,
        UnitPrice          DECIMAL(10,0) NOT NULL,
        CONSTRAINT PK_OrderItemToppings PRIMARY KEY CLUSTERED (OrderItemToppingId),
        CONSTRAINT FK_OIT_OrderItems FOREIGN KEY (OrderItemId)
            REFERENCES dbo.OrderItems(OrderItemId) ON DELETE CASCADE,
        CONSTRAINT FK_OIT_Toppings   FOREIGN KEY (ToppingId)
            REFERENCES dbo.Toppings(ToppingId)
    );
    CREATE INDEX IX_OIT_OrderItemId ON dbo.OrderItemToppings(OrderItemId);
    CREATE INDEX IX_OIT_ToppingId   ON dbo.OrderItemToppings(ToppingId);
END
"@
Invoke-Sql -DbName $Database -Query $schemaSql

# -------------------------------------------------------------------
# 4) マスタ（Menus / Toppings）の MERGE
# -------------------------------------------------------------------
Write-Host "==> Seeding master data (Menus / Toppings)..."
$masterSql = @"
SET NOCOUNT ON;
SET IDENTITY_INSERT dbo.Menus ON;
MERGE dbo.Menus AS T
USING (VALUES
    (1, N'クラシックビーフタコス', N'こだわりのスパイスで味付けしたビーフと新鮮野菜のタコス', 350, N'/Content/images/beef-taco.jpg',    1, 1),
    (2, N'チキンタコス',           N'やわらかグリルチキンとアボカドソースのタコス',                 320, N'/Content/images/chicken-taco.jpg', 1, 2),
    (3, N'シュリンプタコス',       N'プリプリのエビとマンゴーサルサのタコス',                       380, N'/Content/images/shrimp-taco.jpg',  1, 3),
    (4, N'ベジタコス',             N'彩り豊かな野菜とブラックビーンズのタコス',                     300, N'/Content/images/veg-taco.jpg',     1, 4),
    (5, N'チーズケサディーヤ',     N'とろけるチーズをたっぷり挟んだ香ばしいサイドディッシュ',           560, N'/Content/images/quesadilla.jpg',    1, 5),
    (6, N'メキシカンナチョス',     N'クリスピーなチップスにサルサとチーズを重ねた人気サイド',         260, N'/Content/images/nachos.jpg',        1, 6),
    (7, N'ライムソーダ',           N'爽やかなライムの酸味が効いた炭酸ドリンク',                     270, N'/Content/images/lime-soda.jpg',     1, 7),
    (8, N'マンゴーラッシー',       N'濃厚なマンゴーの甘みとヨーグルトのまろやかさが楽しいドリンク',   440, N'/Content/images/mango-lassi.jpg',   1, 8)
) AS S (MenuId, Name, Description, Price, ImageUrl, IsAvailable, DisplayOrder)
ON T.MenuId = S.MenuId
WHEN MATCHED THEN UPDATE SET
    Name=S.Name, Description=S.Description, Price=S.Price, ImageUrl=S.ImageUrl,
    IsAvailable=S.IsAvailable, DisplayOrder=S.DisplayOrder
WHEN NOT MATCHED THEN
    INSERT (MenuId, Name, Description, Price, ImageUrl, IsAvailable, DisplayOrder)
    VALUES (S.MenuId, S.Name, S.Description, S.Price, S.ImageUrl, S.IsAvailable, S.DisplayOrder);
SET IDENTITY_INSERT dbo.Menus OFF;

SET IDENTITY_INSERT dbo.Toppings ON;
MERGE dbo.Toppings AS T
USING (VALUES
    (1, N'グアカモーレ',   80, 1, 1),
    (2, N'追加チーズ',     50, 1, 2),
    (3, N'ハラペーニョ',   30, 1, 3),
    (4, N'サワークリーム', 50, 1, 4),
    (5, N'サルサ',         30, 1, 5),
    (6, N'パクチー',       20, 1, 6)
) AS S (ToppingId, Name, Price, IsAvailable, DisplayOrder)
ON T.ToppingId = S.ToppingId
WHEN MATCHED THEN UPDATE SET
    Name=S.Name, Price=S.Price, IsAvailable=S.IsAvailable, DisplayOrder=S.DisplayOrder
WHEN NOT MATCHED THEN
    INSERT (ToppingId, Name, Price, IsAvailable, DisplayOrder)
    VALUES (S.ToppingId, S.Name, S.Price, S.IsAvailable, S.DisplayOrder);
SET IDENTITY_INSERT dbo.Toppings OFF;
"@
Invoke-Sql -DbName $Database -Query $masterSql

# -------------------------------------------------------------------
# 5) サンプル注文（OrderNumber プレフィックス SAMPLE- のみ削除→挿入）
# -------------------------------------------------------------------
Write-Host "==> Seeding sample orders (prefix SAMPLE-) ..."
$ordersSql = @"
SET NOCOUNT ON;
-- 既存のサンプル注文だけ削除（CASCADE で子レコードも消える）
DELETE FROM dbo.Orders WHERE OrderNumber LIKE N'SAMPLE-%';

DECLARE @now DATETIME = GETDATE();

-- 注文 1: 受付済み（クラシックビーフ x2 + チーズ・サルサ）
INSERT INTO dbo.Orders (OrderNumber, CustomerName, Phone, DeliveryAddress, DeliveryNote, TotalAmount, Status, OrderedAt, UpdatedAt)
VALUES (N'SAMPLE-000001', N'山田 太郎', N'090-1111-2222', N'東京都渋谷区道玄坂1-2-3',     N'インターフォン故障中', 780, 0, DATEADD(MINUTE, -25, @now), DATEADD(MINUTE, -25, @now));
DECLARE @o1 INT = SCOPE_IDENTITY();
INSERT INTO dbo.OrderItems (OrderId, MenuId, Quantity, UnitPrice) VALUES (@o1, 1, 2, 350);
DECLARE @oi1 INT = SCOPE_IDENTITY();
INSERT INTO dbo.OrderItemToppings (OrderItemId, ToppingId, UnitPrice) VALUES (@oi1, 2, 50), (@oi1, 5, 30);

-- 注文 2: 調理中（チキン x1 + グアカモーレ、ベジ x1 + パクチー）
INSERT INTO dbo.Orders (OrderNumber, CustomerName, Phone, DeliveryAddress, DeliveryNote, TotalAmount, Status, OrderedAt, UpdatedAt)
VALUES (N'SAMPLE-000002', N'佐藤 花子', N'080-3333-4444', N'東京都新宿区西新宿2-8-1',     NULL, 720, 1, DATEADD(MINUTE, -15, @now), DATEADD(MINUTE, -10, @now));
DECLARE @o2 INT = SCOPE_IDENTITY();
INSERT INTO dbo.OrderItems (OrderId, MenuId, Quantity, UnitPrice) VALUES (@o2, 2, 1, 320);
DECLARE @oi2a INT = SCOPE_IDENTITY();
INSERT INTO dbo.OrderItemToppings (OrderItemId, ToppingId, UnitPrice) VALUES (@oi2a, 1, 80);
INSERT INTO dbo.OrderItems (OrderId, MenuId, Quantity, UnitPrice) VALUES (@o2, 4, 1, 300);
DECLARE @oi2b INT = SCOPE_IDENTITY();
INSERT INTO dbo.OrderItemToppings (OrderItemId, ToppingId, UnitPrice) VALUES (@oi2b, 6, 20);

-- 注文 3: 配達中（シュリンプ x3）
INSERT INTO dbo.Orders (OrderNumber, CustomerName, Phone, DeliveryAddress, DeliveryNote, TotalAmount, Status, OrderedAt, UpdatedAt)
VALUES (N'SAMPLE-000003', N'鈴木 次郎', N'070-5555-6666', N'神奈川県横浜市西区みなとみらい4-5', N'裏口へお願いします', 1140, 2, DATEADD(MINUTE, -45, @now), DATEADD(MINUTE, -5, @now));
DECLARE @o3 INT = SCOPE_IDENTITY();
INSERT INTO dbo.OrderItems (OrderId, MenuId, Quantity, UnitPrice) VALUES (@o3, 3, 3, 380);
"@
Invoke-Sql -DbName $Database -Query $ordersSql

# -------------------------------------------------------------------
# 6) 件数サマリ
# -------------------------------------------------------------------
Write-Host "==> Summary:"
$summarySql = @"
SET NOCOUNT ON;
SELECT 'Menus'              AS [Table], COUNT(*) AS [Rows] FROM dbo.Menus
UNION ALL SELECT 'Toppings',          COUNT(*) FROM dbo.Toppings
UNION ALL SELECT 'Orders',            COUNT(*) FROM dbo.Orders
UNION ALL SELECT 'OrderItems',        COUNT(*) FROM dbo.OrderItems
UNION ALL SELECT 'OrderItemToppings', COUNT(*) FROM dbo.OrderItemToppings;
"@
Invoke-Sql -DbName $Database -Query $summarySql

Write-Host ""
Write-Host "Done." -ForegroundColor Green

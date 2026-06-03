$base = 'http://localhost:5081'

$checks = @(
    @{ Name = 'Home (menu)';      Path = '/';                                       Expect = @(200) },
    @{ Name = 'Order/Cart';       Path = '/Order/Cart';                             Expect = @(200) },
    @{ Name = 'Status (empty)';   Path = '/Status';                                 Expect = @(302) },
    @{ Name = 'Status (sample)';  Path = '/Status?orderNumber=SAMPLE-000001';       Expect = @(200) },
    @{ Name = 'Status (missing)'; Path = '/Status?orderNumber=NO-SUCH-ORDER';       Expect = @(200) },
    @{ Name = 'Static CSS';       Path = '/Content/site.css';                       Expect = @(200) }
)

& "$PSScriptRoot\smoketest.ps1" -BaseUrl $base -Title 'TacosApp.Web' -Checks $checks
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# 本文に seed データが含まれるか確認
Add-Type -AssemblyName System.Net.Http
$h = New-Object System.Net.Http.HttpClientHandler; $h.AllowAutoRedirect = $true
$c = New-Object System.Net.Http.HttpClient($h); $c.Timeout = [TimeSpan]::FromSeconds(20)

$home = $c.GetAsync("$base/").GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult()
$hasMenuItem = ($home -match 'クラシックビーフタコス') -or ($home -match 'タコス') -or ($home -match 'Taco')
Write-Output ""
Write-Output ("Home contains menu item : {0} (len={1})" -f $hasMenuItem, $home.Length)
$c.Dispose()

if ($hasMenuItem) { Write-Output 'TacosApp.Web smoke test: PASS' } else { Write-Output 'TacosApp.Web smoke test: PARTIAL (no seed text in home)' }

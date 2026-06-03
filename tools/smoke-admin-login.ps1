$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http

$base  = 'http://localhost:5069'
$email = 'admin@tacos.local'
$pass  = 'Admin#12345'

$cookieContainer = New-Object System.Net.CookieContainer
$handler = New-Object System.Net.Http.HttpClientHandler
$handler.CookieContainer = $cookieContainer
$handler.AllowAutoRedirect = $true
$client = New-Object System.Net.Http.HttpClient($handler)
$client.Timeout = [TimeSpan]::FromSeconds(20)

function Show-Resp([string]$tag, [System.Net.Http.HttpResponseMessage]$r) {
    $body = $r.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    $len  = $body.Length
    Write-Output ("[{0}] status={1} url='{2}' len={3}" -f $tag, [int]$r.StatusCode, $r.RequestMessage.RequestUri, $len)
    return $body
}

# 1. ログイン GET — フォームと AntiForgery トークンを取得
$loginGet = $client.GetAsync("$base/Identity/Account/Login").GetAwaiter().GetResult()
$loginHtml = Show-Resp 'login GET' $loginGet

$m = [regex]::Match($loginHtml, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
if (-not $m.Success) { throw 'AntiForgery token not found' }
$token = $m.Groups[1].Value
Write-Output "AntiForgery token len=$($token.Length)"

# 2. ログイン POST
$pairsList = New-Object 'System.Collections.Generic.List[System.Collections.Generic.KeyValuePair[string,string]]'
$pairsList.Add([System.Collections.Generic.KeyValuePair[string,string]]::new('Input.Email',                $email))
$pairsList.Add([System.Collections.Generic.KeyValuePair[string,string]]::new('Input.Password',             $pass))
$pairsList.Add([System.Collections.Generic.KeyValuePair[string,string]]::new('Input.RememberMe',           'false'))
$pairsList.Add([System.Collections.Generic.KeyValuePair[string,string]]::new('__RequestVerificationToken', $token))
$content = New-Object System.Net.Http.FormUrlEncodedContent(,$pairsList)
$loginPost = $client.PostAsync("$base/Identity/Account/Login?returnUrl=%2F", $content).GetAwaiter().GetResult()
$null = Show-Resp 'login POST' $loginPost

if ([int]$loginPost.StatusCode -ne 200) {
    throw "Login failed: HTTP $([int]$loginPost.StatusCode)"
}

# 3. ログイン後 Index アクセス
$indexResp = $client.GetAsync("$base/").GetAwaiter().GetResult()
$indexHtml = Show-Resp 'Index after login' $indexResp

# 4. 注文一覧（Orders ページ）
$ordersResp = $client.GetAsync("$base/Orders").GetAwaiter().GetResult()
$ordersHtml = Show-Resp 'Orders page' $ordersResp

# 5. キーワードチェック
$indexHasStats = $indexHtml -match '本日の注文|Today|Total' -or $indexHtml -match 'SAMPLE-000001'
$ordersHasSample = $ordersHtml -match 'SAMPLE-000001' -or $ordersHtml -match 'SAMPLE-000002' -or $ordersHtml -match 'SAMPLE-000003'

Write-Output ""
Write-Output ("Index contains expected content : {0}" -f $indexHasStats)
Write-Output ("Orders contains SAMPLE-*        : {0}" -f $ordersHasSample)

$client.Dispose()
if ($indexHasStats -and $ordersHasSample) {
    Write-Output 'Admin authenticated smoke test: PASS'
} else {
    Write-Output 'Admin authenticated smoke test: PARTIAL'
}

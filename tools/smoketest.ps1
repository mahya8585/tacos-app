param([Parameter(Mandatory)][string]$BaseUrl, [Parameter(Mandatory)][string]$Title, [Parameter(Mandatory)][array]$Checks)

Add-Type -AssemblyName System.Net.Http
$handler = New-Object System.Net.Http.HttpClientHandler
$handler.AllowAutoRedirect = $false
$client = New-Object System.Net.Http.HttpClient($handler)
$client.Timeout = [TimeSpan]::FromSeconds(15)

Write-Output "=== $Title ==="
$failed = 0
foreach ($c in $Checks) {
    $url = $BaseUrl + $c.Path
    try {
        $resp = $client.GetAsync($url).GetAwaiter().GetResult()
        $code = [int]$resp.StatusCode
        $len  = if ($resp.Content -and $resp.Content.Headers.ContentLength) { $resp.Content.Headers.ContentLength } else { 0 }
        $loc  = if ($resp.Headers.Location) { $resp.Headers.Location.ToString() } else { '' }
        $resp.Dispose()
    } catch {
        $code = -1; $len = 0; $loc = "EX:" + $_.Exception.Message
    }
    $ok = if ($c.Expect -contains $code) { 'OK ' } else { $failed++; 'NG ' }
    Write-Output ("{0} [{1}] expected={2} actual={3} len={4} loc='{5}'  {6}" -f $ok, $c.Name, ($c.Expect -join '/'), $code, $len, $loc, $url)
}
$client.Dispose()
if ($failed -gt 0) { Write-Output "FAILED checks: $failed"; exit 1 }
Write-Output "All checks passed."

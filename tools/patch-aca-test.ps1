param([string]$Image='mcr.microsoft.com/dotnet/samples:aspnetapp', [int]$Port=8080)
$ErrorActionPreference = 'Continue'
$env:AZURE_CORE_ONLY_SHOW_ERRORS = 'true'
$RG = 'aca-lab'
$ACA = 'tacos-shop-express'
$SubId = '70bcc220-4d88-48f2-a59a-77bae4785eac'
$url = "https://management.azure.com/subscriptions/$SubId/resourceGroups/$RG/providers/Microsoft.App/containerApps/$ACA"

$body = @{
    properties = @{
        configuration = @{
            ingress = @{ external = $true; targetPort = $Port; transport = 'http'; allowInsecure = $false }
        }
        template = @{
            containers = @(
                @{
                    name  = 'tacos-shop-express'
                    image = $Image
                    resources = @{ cpu = 0.5; memory = '1Gi' }
                }
            )
            scale = @{ minReplicas = 1; maxReplicas = 1 }
        }
    }
} | ConvertTo-Json -Depth 20 -Compress

$bf = Join-Path $env:TEMP 'aca-test.json'
$body | Set-Content -Path $bf -Encoding utf8
Write-Host "PATCH image=$Image port=$Port"
az rest --method PATCH --uri $url --uri-parameters "api-version=2024-03-01" --body "@$bf" --headers "Content-Type=application/json" -o none 2>$null

Start-Sleep -Seconds 60
$URL2 = 'https://tacos-shop-express.greenbay-d1ee36c7.westcentralus.azurecontainerapps.io/'
for ($i = 1; $i -le 6; $i++) {
    try {
        $r = Invoke-WebRequest -Uri $URL2 -UseBasicParsing -TimeoutSec 30 -MaximumRedirection 0 -ErrorAction Stop
        Write-Host ("Attempt {0}: HTTP {1}  len={2}" -f $i, $r.StatusCode, $r.Content.Length)
        break
    } catch {
        $sc = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 'ERR' }
        Write-Host ("Attempt {0}: {1} {2}" -f $i, $sc, $_.Exception.Message.Split([Environment]::NewLine)[0])
        if ($sc -ne 'ERR') { break }
        Start-Sleep -Seconds 15
    }
}

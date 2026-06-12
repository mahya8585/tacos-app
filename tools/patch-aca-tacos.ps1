param()
$ErrorActionPreference = 'Continue'
$env:AZURE_CORE_ONLY_SHOW_ERRORS = 'true'
$RG = 'aca-lab'
$ACA = 'tacos-shop-express'
$ACR = 'tacosacrjet6hr'
$SqlServer = 'tacos-sql-jet6hr'
$SqlDb = 'TacosDb'
$SpAppId = $env:SP_APP_ID ?? 'b5517418-a909-4b84-bb06-382ff183e22f'
$SpPwd = $env:SP_PWD ?? (Read-Host -Prompt "SP Password" -AsSecureString | ConvertFrom-SecureString -AsPlainText)
$ApiKey = $env:API_KEY ?? 'c79a6929b1b044ecb18e463848487478'
$SubId = $env:SUB_ID ?? '70bcc220-4d88-48f2-a59a-77bae4785eac'

$connStr = "Server=tcp:$SqlServer.database.windows.net,1433;Initial Catalog=$SqlDb;Encrypt=True;TrustServerCertificate=False;Authentication=Active Directory Service Principal;User Id=$SpAppId;Password=$SpPwd;"

$url = "https://management.azure.com/subscriptions/$SubId/resourceGroups/$RG/providers/Microsoft.App/containerApps/$ACA"

$body = @{
    properties = @{
        configuration = @{
            ingress = @{ external = $true; targetPort = 8080; transport = 'http'; allowInsecure = $false }
        }
        template = @{
            containers = @(
                @{
                    name  = 'tacos-shop-express'
                    image = "$ACR.azurecr.io/tacosapp-web:v7"
                    env   = @(
                        @{ name = 'ASPNETCORE_URLS';        value = 'http://+:8080' }
                        @{ name = 'ASPNETCORE_HTTP_PORTS';  value = '8080' }
                        @{ name = 'Logging__LogLevel__Default'; value = 'Information' }
                        @{ name = 'ConnectionStrings__TacosDb'; value = $connStr }
                        @{ name = 'ApiKey';                 value = $ApiKey }
                        @{ name = 'AdminAppOrigin';         value = 'https://placeholder-admin.example.com' }
                        @{ name = 'ASPNETCORE_ENVIRONMENT'; value = 'Production' }
                    )
                    resources = @{ cpu = 0.5; memory = '1Gi' }
                }
            )
            scale = @{ minReplicas = 1; maxReplicas = 1 }
        }
    }
} | ConvertTo-Json -Depth 20 -Compress

$bf = Join-Path $env:TEMP 'aca-tacos.json'
$body | Set-Content -Path $bf -Encoding utf8

Write-Host "PATCH -> $url"
az rest --method PATCH --uri $url --uri-parameters "api-version=2024-03-01" --body "@$bf" --headers "Content-Type=application/json" -o json 2>&1 | Select-Object -Last 3

Start-Sleep -Seconds 60
Write-Host ''
Write-Host '=== Status ==='
$show = az containerapp show -n $ACA -g $RG -o json 2>$null | ConvertFrom-Json
[pscustomobject]@{
    ps        = $show.properties.provisioningState
    rs        = $show.properties.runningStatus
    image     = $show.properties.template.containers[0].image
    port      = $show.properties.configuration.ingress.targetPort
    latestRev = $show.properties.latestRevisionName
} | Format-List

Write-Host ''
Write-Host '=== Replicas ==='
$replicas = az containerapp replica list -n $ACA -g $RG -o json 2>$null | ConvertFrom-Json
$replicas | ForEach-Object {
    [pscustomobject]@{
        name         = $_.name
        runningState = $_.properties.runningState
        restartCount = $_.properties.containers[0].restartCount
        ready        = $_.properties.containers[0].ready
        started      = $_.properties.containers[0].started
    }
} | Format-Table -AutoSize

Write-Host ''
Write-Host '=== Probe ==='
$URL2 = 'https://tacos-shop-express.greenbay-d1ee36c7.westcentralus.azurecontainerapps.io/'
for ($i = 1; $i -le 6; $i++) {
    try {
        $r = Invoke-WebRequest -Uri $URL2 -UseBasicParsing -TimeoutSec 20 -MaximumRedirection 0 -ErrorAction Stop
        Write-Host ("Attempt {0}: HTTP {1}" -f $i, $r.StatusCode)
        if ($r.Content) { Write-Host ($r.Content.Substring(0, [Math]::Min(300, $r.Content.Length))) }
        break
    } catch {
        $msg = $_.Exception.Message
        Write-Host ("Attempt {0}: {1}" -f $i, $msg)
        if ($_.Exception.Response) {
            Write-Host ("  StatusCode: {0}" -f [int]$_.Exception.Response.StatusCode)
            break
        } else {
            Start-Sleep -Seconds 15
        }
    }
}

$msb = 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe'
$vstools = 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Microsoft\VisualStudio\v18.0'
& $msb 'TacosApp.Web\TacosApp.Web.csproj' "/p:VSToolsPath=$vstools" /p:Configuration=Debug /v:minimal /nologo 2>&1 | Select-Object -Last 30
Write-Output ("LASTEXITCODE=" + $LASTEXITCODE)

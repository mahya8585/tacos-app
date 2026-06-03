param([switch]$DryRun)

$files = Get-ChildItem -Path 'TacosApp.Web' -Recurse -Include *.cshtml -File
$utf8Bom = New-Object System.Text.UTF8Encoding $true
$utf8NoBom = New-Object System.Text.UTF8Encoding $false

foreach ($f in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($f.FullName)
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        Write-Output ("SKIP (already BOM): " + $f.FullName.Substring((Get-Location).Path.Length + 1))
        continue
    }
    # Attempt strict UTF-8 decode; if fails it's not UTF-8
    try {
        $strict = New-Object System.Text.UTF8Encoding $false, $true
        $text = $strict.GetString($bytes)
    } catch {
        Write-Output ("WARN (not valid UTF-8, skipped): " + $f.FullName)
        continue
    }
    if ($DryRun) {
        Write-Output ("WOULD ADD BOM: " + $f.FullName.Substring((Get-Location).Path.Length + 1))
    } else {
        [System.IO.File]::WriteAllText($f.FullName, $text, $utf8Bom)
        Write-Output ("ADDED BOM    : " + $f.FullName.Substring((Get-Location).Path.Length + 1))
    }
}
Write-Output "Done."

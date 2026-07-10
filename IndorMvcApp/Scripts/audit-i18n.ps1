$spanish = @{}
Get-ChildItem "$PSScriptRoot\..\Localization" -Filter 'UiTranslations*.cs' | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $matches = [regex]::Matches($content, '\["([^"]+)"\]\s*=\s*"([^"]*)"')
    foreach ($m in $matches) {
        $spanish[$m.Groups[1].Value] = $m.Groups[2].Value
    }
}

$missing = @{}
$same = @{}
Get-ChildItem "$PSScriptRoot\..\Views" -Filter '*.cshtml' -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $matches = [regex]::Matches($content, '@L\["([^"]+)"\]')
    foreach ($m in $matches) {
        $k = $m.Groups[1].Value
        if (-not $spanish.ContainsKey($k)) { $missing[$k] = 1 }
        elseif ($spanish[$k] -eq $k) { $same[$k] = 1 }
    }
}

Write-Host "Missing keys: $($missing.Count)"
Write-Host "Same-as-English keys: $($same.Count)"
Write-Host "--- Missing (first 60) ---"
$missing.Keys | Sort-Object | Select-Object -First 60
Write-Host "--- Same (first 60) ---"
$same.Keys | Sort-Object | Select-Object -First 60

# Final i18n audit with intentional exclusions and effective coverage.
$ErrorActionPreference = "Stop"
$root = Join-Path $PSScriptRoot ".."
$locDir = Join-Path $root "Localization"
$viewsDir = Join-Path $root "Views"

function Test-IntentionalSame([string]$key) {
    if ($key -match '^\$') { return $true }
    if ($key -match '^\(\d{3}\)|^\d{3}[- ]?\d{3}|^\d{10}$') { return $true }
    if ($key -match 'NC \d{5}|TX \d{5}|\.jpeg|\.jpg|\.png|/inspeccion') { return $true }
    if ($key -match '^\d{5}(-\d{4})?(,\s*\d{5})*') { return $true }
    if ($key -match '^(HVAC|SMS|Email|Push|ETA|INDOR|PRO|BRRRR|W-9|JPG|WEBP|PDF|HEIC|Redfin|Realtor|Zillow)$') { return $true }
    if ($key -match '^\d+(\.\d+)?\s*(mi|yr|yrs|bed|bath|bin|cleaner|story|stories|PM|AM)\b') { return $true }
    if ($key -match 'AM\s*[–-]|PM\s*[–-]|/mo|/hr|k\+|^\d+:\d{2}') { return $true }
    if ($key -match '^24/7|^x$|^\d+x\d+|^12PM|^8AM|^5PM') { return $true }
    if ($key -match '^\d{1,3}\s*(Business|Main|Maple|Riverside|Wallace)') { return $true }
    if ($key -match '^\$1,000,000$|^12-3456789$') { return $true }
    if ($key -match '^/ \d+ characters$') { return $true }
    return $false
}

$spanish = @{}
Get-ChildItem $locDir -Filter 'UiTranslations*.cs' | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding UTF8
    [regex]::Matches($content, '\["([^"]+)"\]\s*=\s*"([^"]*)"') | ForEach-Object {
        $spanish[$_.Groups[1].Value] = $_.Groups[2].Value
    }
}

$allKeys = @{}
$missing = @{}
$same = @{}
$sameIntentional = @{}
$sameReal = @{}

Get-ChildItem $viewsDir -Filter '*.cshtml' -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding UTF8
    [regex]::Matches($content, '@L\["([^"]+)"\]') | ForEach-Object {
        $k = $_.Groups[1].Value
        $allKeys[$k] = 1
        if (-not $spanish.ContainsKey($k)) {
            $missing[$k] = 1
        } elseif ($spanish[$k] -eq $k) {
            $same[$k] = 1
            if (Test-IntentionalSame $k) { $sameIntentional[$k] = 1 } else { $sameReal[$k] = 1 }
        }
    }
}

$total = $allKeys.Count
$translated = $total - $missing.Count - $sameReal.Count
$coverage = if ($total -gt 0) { [math]::Round(100.0 * $translated / $total, 1) } else { 100.0 }

Write-Host "=== Final i18n Audit ==="
Write-Host "Total unique @L[] keys in views: $total"
Write-Host "Missing translations: $($missing.Count)"
Write-Host "Same-as-English (all): $($same.Count)"
Write-Host "Same-as-English (intentional): $($sameIntentional.Count)"
Write-Host "Same-as-English (real gaps): $($sameReal.Count)"
Write-Host "Effective coverage: $coverage%"
Write-Host ""
Write-Host "--- Missing keys ---"
$missing.Keys | Sort-Object
Write-Host ""
Write-Host "--- Real same-as-English (first 40) ---"
$sameReal.Keys | Sort-Object | Select-Object -First 40

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$locDir = Join-Path $root "IndorMvcApp\Localization"
$viewsDir = Join-Path $root "IndorMvcApp\Views"

$spanish = @{}
Get-ChildItem $locDir -Filter 'UiTranslations*.cs' | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding UTF8
    [regex]::Matches($content, '\["([^"]+)"\]\s*=\s*"([^"]*)"') | ForEach-Object {
        $spanish[$_.Groups[1].Value] = $_.Groups[2].Value
    }
}

$serviceFolders = @(
    'Lawn','Trash','CleaningPro','Cleaning','HvacMaintenance','SafeAir','Moving','Packing',
    'FurnitureAssembly','GutterCleaning','ExteriorPaint','WaterHeaterFlush','CrawlspaceCheck',
    'RoofInspection','PestControl','HvacFilterReplacement','PowerWash','Microservicios',
    'Inspecciones','Utilities','RealtorRequest'
)

$viewKeys = @{}
foreach ($f in $serviceFolders) {
    $path = Join-Path $viewsDir $f
    if (-not (Test-Path $path)) { continue }
    Get-ChildItem $path -Filter '*.cshtml' -Recurse | ForEach-Object {
        $content = Get-Content $_.FullName -Raw -Encoding UTF8
        [regex]::Matches($content, '@L\["([^"]+)"\]') | ForEach-Object {
            $viewKeys[$_.Groups[1].Value] = 1
        }
    }
}

$same = @()
$missing = @()
foreach ($k in ($viewKeys.Keys | Sort-Object)) {
    if (-not $spanish.ContainsKey($k)) { $missing += $k }
    elseif ($spanish[$k] -eq $k) { $same += $k }
}

Write-Host "Service view @L keys: $($viewKeys.Count)"
Write-Host "Missing: $($missing.Count)"
Write-Host "Same-as-English: $($same.Count)"
Write-Host "--- Same-as-EN ---"
$same | ForEach-Object { Write-Host $_ }

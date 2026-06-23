# Consolidate Inspecciones per-view <style> blocks into wwwroot/css/inspecciones-wizard.css
# and remove @section Head from views.

$root = Join-Path $PSScriptRoot "..\Views\Inspecciones"
$cssOut = Join-Path $PSScriptRoot "..\wwwroot\css\inspecciones-wizard.css"
$skipSel = '(?i)^(\*|\s*body\s*|\.page-container|\.top-bar|\.back-btn)(\s|,|$)'
$rules = [ordered]@{}
$files = Get-ChildItem $root -Filter "*.cshtml" | Where-Object { $_.Name -notlike '_*' }

foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw
    if ($content -notmatch '(?s)@section Head\s*\{\s*<style>(.*?)</style>\s*\}') { continue }
    $css = $matches[1]
    foreach ($m in [regex]::Matches($css, '(?s)([^{}]+)\{([^{}]*)\}')) {
        $sel = $m.Groups[1].Value.Trim() -replace '\s+', ' '
        if ($sel -match $skipSel -or [string]::IsNullOrWhiteSpace($sel)) { continue }
        $rules[$sel] = $m.Groups[2].Value.Trim()
    }
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("/* Inspecciones + emergency flows — consolidated per-view styles */")
[void]$sb.AppendLine("/* Regenerate: Scripts/ConsolidateInspeccionesCss.ps1 */")
[void]$sb.AppendLine("")
foreach ($kv in $rules.GetEnumerator()) {
    [void]$sb.AppendLine("$($kv.Key) { $($kv.Value) }")
}
[void]$sb.AppendLine("")
[void]$sb.AppendLine("/* INDOR design tokens */")
[void]$sb.AppendLine(".iw-wizard-shell .field-card, .iw-wizard-shell .summary-card, .iw-wizard-shell .upload-card { border-radius: 20px; }")
[void]$sb.AppendLine(".iw-wizard-shell .btn-primary { background: var(--indor-grad-primary, linear-gradient(135deg, #2E8BFF, #0066CC)); border: none; }")
[void]$sb.AppendLine(".iw-wizard-shell .page-heading { color: var(--indor-navy, #0A2540); }")
[void]$sb.AppendLine(".iw-wizard-shell .chip-btn.active, .iw-wizard-shell .option-btn.active, .iw-wizard-shell .segment-btn.active { border-color: #0066CC; }")
$sb.ToString() | Set-Content $cssOut -Encoding utf8

$changed = 0
foreach ($f in $files) {
    $c = Get-Content $f.FullName -Raw
    $orig = $c
    $c = [regex]::Replace($c, '(?s)\r?\n@section Head\s*\{\s*<style>.*?</style>\s*\}\s*', "`n")
    $c = [regex]::Replace($c, '(?s)(ViewData\["WizardTotalSteps"\][^\}]*\}\s*)\r?\n\s*</div>\s*', "`${1}`n")
    if ($c -ne $orig) {
        Set-Content $f.FullName $c.TrimEnd() -Encoding utf8 -NoNewline
        $changed++
    }
}

Write-Host "CSS rules: $($rules.Count) -> $cssOut"
Write-Host "Views cleaned: $changed"

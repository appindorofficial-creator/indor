# Generates UiTranslationsServiceWizards.cs — overrides untranslated UiTranslationsFlows wizard strings.
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$flowsFile = Join-Path $root "IndorMvcApp\Localization\UiTranslationsFlows.cs"
$outFile = Join-Path $root "IndorMvcApp\Localization\UiTranslationsServiceWizards.cs"
$csvFiles = Get-ChildItem (Join-Path $PSScriptRoot "service-wizards-es*.csv") -ErrorAction SilentlyContinue

function Escape-Cs([string]$s) {
    return $s.Replace('\', '\\').Replace('"', '\"')
}

function Is-IntentionalSame([string]$key) {
    if ($key -match '^\$') { return $true }
    if ($key -match '^\d{3} ') { return $true }
    if ($key -match '^\(\d{3}\)') { return $true }
    if ($key -match 'NC \d{5}|TX \d{5}|\.jpeg|\.jpg|\.png') { return $true }
    if ($key -match '^\d{5}(-\d{4})?$') { return $true }
    if ($key -match '^HVAC$|^SMS$|^Email$|^Push$|^ETA$|^JPG|^WEBP|^PDF') { return $true }
    if ($key -match '^\d+(\.\d+)?\s*(mi|yr|yrs|bed|bath|bin|cleaner|story|stories)\b') { return $true }
    if ($key -match '^\d{1,2}:\d{2}\s*(AM|PM)|AM\s*[–-]|PM\s*[–-]|/mo|/hr|k\+|\$') { return $true }
    if ($key -match '^24/7|^x$|^\d+x\d+') { return $true }
    return $false
}

# Load manual CSV translations (key|value per line)
$map = @{}
foreach ($csv in ($csvFiles | Sort-Object Name)) {
    Get-Content $csv.FullName -Encoding UTF8 | ForEach-Object {
        if ($_ -match '^\s*$' -or $_ -match '^\s*#') { return }
        $idx = $_.IndexOf('|')
        if ($idx -lt 0) { return }
        $key = $_.Substring(0, $idx)
        $val = $_.Substring($idx + 1)
        $map[$key] = $val
    }
}

# Also pick same-as-english keys from Flows that have CSV translations
$flowsContent = Get-Content $flowsFile -Raw -Encoding UTF8
[regex]::Matches($flowsContent, '\["([^"]+)"\]\s*=\s*"([^"]*)"') | ForEach-Object {
    $key = $_.Groups[1].Value
    $val = $_.Groups[2].Value
    if ($val -eq $key -and -not (Is-IntentionalSame $key) -and $map.ContainsKey($key)) {
        # already in map
    }
}

if ($map.Count -eq 0) {
    Write-Error "No translations found in Scripts/service-wizards-es*.csv"
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("namespace IndorMvcApp.Localization;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("/// <summary>Overrides untranslated/broken UiTranslationsFlows entries for homeowner service wizards.</summary>")
[void]$sb.AppendLine("public static class UiTranslationsServiceWizards")
[void]$sb.AppendLine("{")
[void]$sb.AppendLine("    public static IEnumerable<KeyValuePair<string, string>> Entries =>")
[void]$sb.AppendLine("        new Dictionary<string, string>(StringComparer.Ordinal)")
[void]$sb.AppendLine("        {")

foreach ($key in ($map.Keys | Sort-Object)) {
    $es = $map[$key]
    [void]$sb.AppendLine("            [`"$([string](Escape-Cs $key))`"] = `"$([string](Escape-Cs $es))`",")
}

[void]$sb.AppendLine("        };")
[void]$sb.AppendLine("}")

[System.IO.File]::WriteAllText($outFile, $sb.ToString(), [System.Text.UTF8Encoding]::new($false))
Write-Output "Wrote $($map.Count) entries to $outFile"

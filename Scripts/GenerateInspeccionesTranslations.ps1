# Generates UiTranslationsInspeccionesEmergency.cs from CSV maps
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$outFile = Join-Path $root "IndorMvcApp\Localization\UiTranslationsInspeccionesEmergency.cs"

function Escape-Cs([string]$s) {
    return $s.Replace('\', '\\').Replace('"', '\"')
}

$map = @{}
Get-ChildItem (Join-Path $PSScriptRoot "inspecciones-es-part*.csv") | Sort-Object Name | ForEach-Object {
    Get-Content $_.FullName -Encoding UTF8 | ForEach-Object {
        if ($_ -match '^\s*$') { return }
        $idx = $_.IndexOf('|')
        if ($idx -lt 0) { return }
        $key = $_.Substring(0, $idx)
        $val = $_.Substring($idx + 1)
        $map[$key] = $val
    }
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("namespace IndorMvcApp.Localization;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("/// <summary>Overrides broken/missing UiTranslationsFlows entries for inspections and emergency flows.</summary>")
[void]$sb.AppendLine("public static class UiTranslationsInspeccionesEmergency")
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

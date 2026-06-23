$root = "c:\Users\oscar.sierra\source\repos\Indor\IndorMvcApp\Views\Inspecciones"
$stub = "@* Progress rendered by _ServiceWizardChrome *@`n"

$stepperTotals = @{}
Get-ChildItem (Join-Path $root "_*Stepper.cshtml") | ForEach-Object {
    $c = [IO.File]::ReadAllText($_.FullName)
    if ($c -match 'var steps = new\[\] \{ ([^\}]+) \}') {
        $stepperTotals[$_.Name] = ($matches[1] -split ',').Count
    }
}

function Convert-BackAttrsToUrlAction {
    param([string]$Attrs)
    if ([string]::IsNullOrWhiteSpace($Attrs) -or $Attrs -match 'onclick=.*history\.back') {
        return 'Url.Action("Index", "Home")'
    }
    $action = if ($Attrs -match 'asp-action="([^"]+)"') { $matches[1] } else { $null }
    if (-not $action) { return 'Url.Action("Index", "Home")' }
    $controller = if ($Attrs -match 'asp-controller="([^"]+)"') { $matches[1] } else { 'Inspecciones' }
    $routeProp = if ($Attrs -match 'asp-route-id="@Model\.(\w+)"') { $matches[1] } else { $null }
    $fragment = if ($Attrs -match 'asp-fragment="([^"]+)"') { $matches[1] } else { $null }
    if ($routeProp -and $fragment) {
        return "Url.Action(`"$action`", `"$controller`", new { id = Model.$routeProp, fragment = `"$fragment`" })"
    }
    if ($routeProp) {
        return "Url.Action(`"$action`", `"$controller`", new { id = Model.$routeProp })"
    }
    if ($fragment) {
        return "Url.Action(`"$action`", `"$controller`", new { fragment = `"$fragment`" })"
    }
    return "Url.Action(`"$action`", `"$controller`")"
}

function Inject-WizardBlock {
    param([string]$Content, [string]$Inject)
    return [regex]::Replace($Content, '(?s)(@\{.*?)(\r?\n\})', "`$1`n$Inject`$2", 1)
}

function Wrap-ScriptsSection {
    param([string]$Content)
    if ($Content -match '(?s)<script>' -and $Content -notmatch '@section Scripts') {
        $Content = [regex]::Replace($Content, '(?s)(\r?\n)(<script>)', "`$1@section Scripts {`n`$2", 1)
        $Content = [regex]::Replace($Content, '(?s)</script>\s*$', "`n</script>`n}", 1)
    }
    return $Content.TrimEnd() + "`n"
}

function Convert-InspeccionView {
    param([string]$Path)

    $content = [IO.File]::ReadAllText($Path)
    if ($content -notmatch 'Layout\s*=\s*null') { return $false }

    $step = 0
    $totalSteps = 0
    $stepperName = $null
    if ($content -match '@await Html\.PartialAsync\("(_[^"]+Stepper)",\s*(\d+)\)') {
        $stepperName = $matches[1] + '.cshtml'
        $step = [int]$matches[2]
        if ($stepperTotals.ContainsKey($stepperName)) {
            $totalSteps = $stepperTotals[$stepperName]
        }
    }

    $title = 'Inspection'
    if ($content -match '(?s)<div class="top-bar">[\s\S]*?<strong>([^<]+)</strong>') {
        $title = $matches[1].Trim()
    }
    elseif ($content -match 'ViewData\["Title"\]\s*=\s*"([^"]+)"') {
        $title = ($matches[1] -replace '\s*-\s*INDOR\s*$', '').Trim()
    }

    $backUrl = 'Url.Action("Index", "Home")'
    if ($content -match '(?s)<a\s+([^>]*class="back-btn"[^>]*)>') {
        $backUrl = Convert-BackAttrsToUrlAction $matches[1]
    }
    elseif ($content -match '(?s)<a\s+([^>]*class="back-btn"[^>]*)>') {
        $backUrl = Convert-BackAttrsToUrlAction $matches[1]
    }

    $headSection = ''
    if ($content -match '(?s)(<style>[\s\S]*?</style>)') {
        $headSection = "@section Head {`n$($matches[1])`n}`n`n"
        $content = $content -replace '(?s)\s*<style>[\s\S]*?</style>\s*', "`n"
    }

    $content = $content -replace '\s*Layout\s*=\s*null;\s*', "`n"
    $content = [regex]::Replace($content, '(?s)<!DOCTYPE html>[\s\S]*?</head>\s*<body>\s*', "`n", 1)
    $content = [regex]::Replace($content, '(?s)<div class="page-container">\s*', "`n", 1)
    $content = [regex]::Replace($content, '(?s)<div class="top-bar">[\s\S]*?</div>\s*', "`n", 1)
    $content = [regex]::Replace($content, '(?s)^\s*<a\s+[^>]*class="back-btn"[^>]*>[\s\S]*?</a>\s*', "`n", 1)
    if ($stepperName) {
        $partial = $stepperName -replace '\.cshtml$', ''
        $content = [regex]::Replace($content, "(?s)@await Html\.PartialAsync\(`"$partial`",\s*\d+\)\s*", "`n", 1)
    }
    $content = [regex]::Replace($content, '(?s)</div>\s*</body>\s*</html>\s*$', "`n", 1)
    $content = [regex]::Replace($content, '(?s)</body>\s*</html>\s*$', "`n", 1)

    $wiz = @(
        "    ViewData[`"WizardTitle`"] = `"$($title.Replace('"', '\"'))`";",
        "    ViewData[`"WizardBackUrl`"] = $backUrl;"
    )
    if ($step -gt 0 -and $totalSteps -gt 0) {
        $wiz += "    ViewData[`"WizardStep`"] = $step;"
        $wiz += "    ViewData[`"WizardTotalSteps`"] = $totalSteps;"
    }
    $content = Inject-WizardBlock $content (($wiz -join "`n") + "`n")
    $content = Wrap-ScriptsSection $content

    if ($headSection) {
        $content = [regex]::Replace($content, '(?s)(\r?\n\})\s*(\r?\n)', "`$1`n`n$headSection", 1)
    }

    [IO.File]::WriteAllText($Path, $content)
    return $true
}

$converted = 0
Get-ChildItem $root -Filter *.cshtml | Where-Object { $_.Name -notlike '_*' } | ForEach-Object {
    if (Convert-InspeccionView $_.FullName) { $converted++ }
}

Get-ChildItem $root -Filter _*Stepper.cshtml | ForEach-Object {
    [IO.File]::WriteAllText($_.FullName, $stub)
}

# Cleanup orphan top-bars / spacers
Get-ChildItem $root -Filter *.cshtml | Where-Object { $_.Name -notlike '_*' } | ForEach-Object {
    $c = [IO.File]::ReadAllText($_.FullName)
    $n = [regex]::Replace($c, '(?s)<div class="top-bar">[\s\S]*?</div>\s*', '')
    $n = [regex]::Replace($n, '(?s)^\s*<a\s+[^>]*class="back-btn"[^>]*>[\s\S]*?</a>\s*', "`n", 1)
    $n = [regex]::Replace($n, '(?s)<div style="width:44px;"></div>\s*</div>\s*', '')
    if ($n -ne $c) { [IO.File]::WriteAllText($_.FullName, $n) }
}

Write-Output "Converted $converted inspection views"

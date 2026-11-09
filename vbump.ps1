# ==============================================================================
# Version Bump Script
# ==============================================================================
# Bumps <Version> in src/Directory.Build.props, which every extension package
# picks up (all packages share one version).
#
# Usage:
#   ./vbump.ps1                  # bump minor, reset patch, e.g. 10.2.0 -> 10.3.0
#   ./vbump.ps1 -Part Patch      # 10.2.0 -> 10.2.1
#   ./vbump.ps1 -Part Major      # 10.2.0 -> 11.0.0
#   ./vbump.ps1 -Version 10.5.0  # set Version explicitly

param(
    [string]$Version,
    [ValidateSet("Major", "Minor", "Patch")]
    [string]$Part = "Minor"
)

$propsPath = Resolve-Path (Join-Path $PSScriptRoot "src\Directory.Build.props")

# Note: this intentionally does NOT round-trip the file through XmlDocument/XDocument,
# as that would reformat unrelated parts of the file. We only read the value via XPath
# and patch the single <Version> line in place to keep the diff minimal.
[xml]$propsXml = Get-Content $propsPath -Raw
$versionNode = $propsXml.SelectSingleNode("//Project/PropertyGroup/Version")

if (-not $versionNode) {
    Write-Host "[X] Error: Could not find <Version> in '$propsPath'." -ForegroundColor Red
    exit 1
}

$oldVersion = $versionNode.InnerText.Trim()

if ($Version) {
    if ($Version -notmatch '^\d+\.\d+\.\d+$') {
        Write-Host "[X] Error: Version '$Version' is not in format 'Major.Minor.Patch'." -ForegroundColor Red
        exit 1
    }
    $newVersion = $Version
} else {
    if ($oldVersion -notmatch '^(\d+)\.(\d+)\.(\d+)$') {
        Write-Host "[X] Error: Cannot auto-bump Version '$oldVersion' (expected format 'Major.Minor.Patch'). Pass -Version explicitly." -ForegroundColor Red
        exit 1
    }
    $major = [int]$Matches[1]
    $minor = [int]$Matches[2]
    $patch = [int]$Matches[3]

    switch ($Part) {
        "Major" { $major++; $minor = 0; $patch = 0 }
        "Minor" { $minor++; $patch = 0 }
        "Patch" { $patch++ }
    }
    $newVersion = "$major.$minor.$patch"
}

$content = Get-Content $propsPath -Raw
$content = $content.Replace("<Version>$oldVersion</Version>", "<Version>$newVersion</Version>")
[System.IO.File]::WriteAllText($propsPath.Path, $content, (New-Object System.Text.UTF8Encoding($false)))

Write-Host "Version: $oldVersion -> $newVersion" -ForegroundColor Green

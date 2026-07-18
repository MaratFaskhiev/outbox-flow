param(
    [string]$SamplesDir,
    [string]$DocsDir
)

$RepoRoot = Split-Path -Parent $PSScriptRoot
if (-not $SamplesDir) { $SamplesDir = Join-Path $RepoRoot "samples\OutboxFlow.Sample" }
if (-not $DocsDir) { $DocsDir = Join-Path $RepoRoot "docs" }

$ErrorActionPreference = "Stop"

& (Join-Path $PSScriptRoot "embed-doc-snippets.ps1") -SamplesDir $SamplesDir -DocsDir $DocsDir

$mdFiles = @(Get-ChildItem -Path $DocsDir -Filter "*.md")
$mdFiles += Get-Item (Join-Path $RepoRoot "README.md")

$dirty = $false
foreach ($mdFile in $mdFiles) {
    $diff = git diff $mdFile.FullName
    if ($diff) {
        Write-Host "[DIRTY] $($mdFile.Name)"
        $dirty = $true
    }
}

if ($dirty) {
    Write-Host ""
    Write-Error "Documentation snippets are out of sync. Run embed-doc-snippets.ps1 to update."
    exit 1
}

Write-Host "[OK] All documentation snippets are in sync."
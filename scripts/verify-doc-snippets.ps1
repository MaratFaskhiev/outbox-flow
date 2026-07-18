<#
.SYNOPSIS
    Checks that embedded snippets in documentation are synchronized with sample code.
.DESCRIPTION
    Runs embed-doc-snippets.ps1 and compares documentation changes with the last commit,
    ignoring formatting changes (whitespace, blank lines, line endings).
    If there are meaningful changes, the script exits with an error.
.PARAMETER SamplesDir
    Path to the samples folder (default: samples/OutboxFlow.Sample).
.PARAMETER DocsDir
    Path to the documentation folder (default: docs).
.PARAMETER ExcludeFiles
    List of files (glob pattern) to exclude from verification (e.g., configuration.md).
#>

param(
    [string]$SamplesDir,
    [string]$DocsDir,
    [string[]]$ExcludeFiles = @("configuration.md")
)

$ErrorActionPreference = "Continue"

$RepoRoot = Split-Path -Parent $PSScriptRoot
if (-not $SamplesDir) { $SamplesDir = Join-Path $RepoRoot "samples\OutboxFlow.Sample" }
if (-not $DocsDir) { $DocsDir = Join-Path $RepoRoot "docs" }

$embedScript = Join-Path $PSScriptRoot "embed-doc-snippets.ps1"
if (-not (Test-Path $embedScript)) {
    Write-Error "embed-doc-snippets.ps1 not found at $embedScript"
    exit 1
}

# Run snippet generation
Write-Host "Running embed-doc-snippets.ps1..." -ForegroundColor Cyan
& $embedScript -SamplesDir $SamplesDir -DocsDir $DocsDir
# Ignore $LASTEXITCODE as it may be a false positive

# Collect files to check
$filesToCheck = @()
Get-ChildItem -Path $DocsDir -Filter "*.md" -File -ErrorAction SilentlyContinue | ForEach-Object { $filesToCheck += $_ }
$readme = Join-Path $RepoRoot "README.md"
if (Test-Path $readme) {
    $filesToCheck += Get-Item $readme
}

# Exclude files by glob pattern
if ($ExcludeFiles) {
    $filesToCheck = $filesToCheck | Where-Object {
        $excluded = $false
        foreach ($pattern in $ExcludeFiles) {
            if ($_.Name -like $pattern) { $excluded = $true; break }
        }
        -not $excluded
    }
}

# Git diff options to ignore all whitespace and blank lines
# -w                : ignore all whitespace (including tabs, spaces, line endings)
# --ignore-blank-lines : ignore addition/deletion of blank lines
$ignoreOptions = @("-w", "--ignore-blank-lines")

$dirty = $false
foreach ($file in $filesToCheck) {
    if (-not (Test-Path $file.FullName)) {
        Write-Host "[DIRTY] $($file.Name) (missing)" -ForegroundColor Red
        $dirty = $true
        continue
    }

    # Run git diff ignoring whitespace
    git diff $ignoreOptions --exit-code HEAD -- $file.FullName 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        # If there are meaningful changes, show diff (without ignoring whitespace for clarity)
        Write-Host "[DIRTY] $($file.Name) - meaningful changes detected" -ForegroundColor Red
        git diff HEAD -- $file.FullName
        $dirty = $true
    } else {
        Write-Host "[OK] $($file.Name)" -ForegroundColor Green
    }
}

if ($dirty) {
    Write-Host ""
    Write-Error "Documentation contains changes that affect meaning (not only whitespace)."
    Write-Host "Run 'scripts\embed-doc-snippets.ps1' locally and commit the changes."
    exit 1
}

Write-Host "[OK] All documentation snippets are synchronized." -ForegroundColor Green
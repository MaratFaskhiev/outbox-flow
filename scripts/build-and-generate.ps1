<#
.SYNOPSIS
    Minimal script for building, publishing, and generating documentation.
.DESCRIPTION
    Automatically detects the repository root, builds, publishes, and generates documentation.
    Verifies success by checking for the output file rather than the exit code.
#>

# Detect repository root
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

# Check for .NET SDK
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "❌ .NET SDK not found. Please install .NET 10 SDK." -ForegroundColor Red
    exit 1
}

# Check for existing published assemblies
$requiredDlls = @("OutboxFlow.dll", "OutboxFlow.Postgres.dll", "OutboxFlow.Kafka.dll")
$publishExists = $true
foreach ($dll in $requiredDlls) {
    if (-not (Test-Path "publish\$dll")) {
        $publishExists = $false
        break
    }
}

if (-not $publishExists) {
    Write-Host "▶ Building and publishing projects..." -ForegroundColor Cyan
    dotnet restore
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    dotnet build -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    dotnet publish -c Release -f net10.0 -o ./publish --no-build
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "✓ Build and publish completed." -ForegroundColor Green
} else {
    Write-Host "✓ publish folder already contains assemblies, skipping build." -ForegroundColor Green
}

# Run documentation generation
Write-Host "▶ Generating documentation..." -ForegroundColor Cyan
$asmPaths = @(
    "publish\OutboxFlow.dll",
    "publish\OutboxFlow.Postgres.dll",
    "publish\OutboxFlow.Kafka.dll"
)
$resolveRoots = @("publish")

# Run the script and ignore possible non-zero exit code,
# because errors inside the script may not affect the output file.
& .\scripts\generate-config-docs.ps1 -Configuration Release -TargetFramework net10.0 -AssemblyPaths $asmPaths -ResolveRoots $resolveRoots

# Check if documentation file was created
$outputFile = "docs\configuration.md"
if (Test-Path $outputFile) {
    $size = (Get-Item $outputFile).Length
    if ($size -gt 0) {
        Write-Host "✅ Documentation successfully generated: $outputFile ($size bytes)" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "❌ Documentation file is empty: $outputFile" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "❌ Documentation file not found: $outputFile" -ForegroundColor Red
    exit 1
}
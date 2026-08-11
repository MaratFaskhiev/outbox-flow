param(
    [string]$SamplesDir,
    [string]$DocsDir
)

$RepoRoot = Split-Path -Parent $PSScriptRoot
if (-not $SamplesDir) { $SamplesDir = Join-Path $RepoRoot "samples\OutboxFlow.Sample" }
if (-not $DocsDir) { $DocsDir = Join-Path $RepoRoot "docs" }

$ExtraMdFiles = @()
$readmePath = Join-Path $RepoRoot "README.md"
if (Test-Path $readmePath) { $ExtraMdFiles += Get-Item $readmePath }

$ErrorActionPreference = "Stop"

$regionPattern = '#region\s+(docs_\w+)'
$endRegionPattern = '#endregion'
$snippetPattern = '<!--\s*SNIPPET:\s+(docs_\w+)\s*-->'
$endSnippetPattern = '<!--\s*ENDSNIPPET:\s+(docs_\w+)\s*-->'

# Function to normalize content (remove BOM, normalize line endings, whitespace)
function Normalize-Content {
    param([string]$Content)
    if (-not $Content) { return "" }
    # Remove BOM if present
    if ($Content -and $Content[0] -eq [char]0xFEFF) {
        $Content = $Content.Substring(1)
    }
    # Normalize line endings to LF
    $Content = $Content -replace "`r`n", "`n"
    # Remove trailing whitespace from each line
    $lines = $Content -split "`n"
    $normalizedLines = $lines | ForEach-Object { $_.TrimEnd() }
    # Remove blank lines at the end of the file
    while ($normalizedLines.Count -gt 0 -and $normalizedLines[-1] -eq "") {
        $normalizedLines = $normalizedLines[0..($normalizedLines.Count-2)]
    }
    return ($normalizedLines -join "`n")
}

function Find-MatchingEndRegion {
    param([string[]]$Lines, [int]$StartIndex)
    $depth = 0
    for ($j = $StartIndex; $j -lt $Lines.Count; $j++) {
        if ($Lines[$j] -match $regionPattern) { $depth++ }
        elseif ($Lines[$j] -match $endRegionPattern) {
            if ($depth -eq 0) { return $j }
            $depth--
        }
    }
    return -1
}

$regionMap = @{}

Write-Host "Scanning .cs files in $SamplesDir ..."
Get-ChildItem -Path $SamplesDir -Filter "*.cs" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    # Split by any combination of \r\n or \n
    $lines = $content -split "`r?`n"

    $regionStarts = @()
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match $regionPattern) {
            $regionStarts += @{ Index = $i; Name = $matches[1] }
        }
    }

    foreach ($rs in $regionStarts) {
        $regionName = $rs.Name
        $startLine = $rs.Index
        $endLine = Find-MatchingEndRegion -Lines $lines -StartIndex ($startLine + 1)
        if ($endLine -eq -1) {
            Write-Error "Unterminated region '$regionName' in $($_.Name)"
            exit 1
        }

        $regionLines = $lines[($startLine + 1)..($endLine - 1)] | Where-Object { $_ -notmatch '^\s*#region\b' -and $_ -notmatch '^\s*#endregion\b' }
        $nonEmpty = $regionLines | Where-Object { $_.Trim() -ne '' }
        if ($nonEmpty.Count -eq 0) {
            Write-Host "  [SKIP] Empty region '$regionName' in $($_.Name)"
            continue
        }

        $minIndent = ($nonEmpty | ForEach-Object {
            if ($_ -match '^(\s*)') { $matches[1].Length } else { 0 }
        } | Measure-Object -Minimum).Minimum

        $trimmed = $regionLines | ForEach-Object {
            if ($_.Length -ge $minIndent) { $_.Substring($minIndent) } else { $_ }
        }

        if ($regionMap.ContainsKey($regionName)) {
            Write-Error "Duplicate region name '$regionName' found in $($_.Name) and $($regionMap[$regionName].File)"
            exit 1
        }

        $regionMap[$regionName] = @{
            File = $_.Name
            Lines = $trimmed
        }
        Write-Host "  [OK]   Region '$regionName' from $($_.Name)"
    }
}

Write-Host "`nScanning .md files ..."
$mdFiles = @(Get-ChildItem -Path $DocsDir -Filter "*.md")
if ($ExtraMdFiles) { $mdFiles += $ExtraMdFiles }
$usedRegions = @{}

foreach ($mdFile in $mdFiles) {
    $content = Get-Content $mdFile.FullName -Raw
    $original = $content

    $lines = $content -split "`r?`n"
    $newLines = @()
    $i = 0
    while ($i -lt $lines.Count) {
        if ($lines[$i] -match $snippetPattern) {
            $regionName = $matches[1]
            $usedRegions[$regionName] = $true

            if (-not $regionMap.ContainsKey($regionName)) {
                Write-Error "Marker '$regionName' found in $($mdFile.Name) but no matching #region exists in any .cs file"
                exit 1
            }

            $newLines += $lines[$i]
            $i++
            $firstContentLineIndex = $i
            while ($i -lt $lines.Count -and $lines[$i] -notmatch $endSnippetPattern) {
                $i++
            }
            if ($i -ge $lines.Count) {
                Write-Error "Unterminated SNIPPET marker '$regionName' in $($mdFile.Name)"
                exit 1
            }

            $regionLines = $regionMap[$regionName].Lines
            $codeBlockIndent = ''
            if ($firstContentLineIndex -lt $lines.Count -and $lines[$firstContentLineIndex] -match '^(\s*)') {
                $codeBlockIndent = $matches[1]
            }

            # Detect a manually placed fenced code block between the markers:
            # the first non-blank line is an opening fence (``` or ```lang) and
            # the last non-blank line is a closing fence (```). Preserve both and
            # replace only the content between them.
            $openingFence = ''
            $closingFence = ''
            if ($firstContentLineIndex -lt $i) {
                $fenceStartIndex = $firstContentLineIndex
                while ($fenceStartIndex -lt $i -and $lines[$fenceStartIndex].Trim() -eq '') {
                    $fenceStartIndex++
                }
                if ($fenceStartIndex -lt $i -and $lines[$fenceStartIndex] -match '^```\w*\s*$') {
                    $fenceEndIndex = $i - 1
                    while ($fenceEndIndex -gt $fenceStartIndex -and $lines[$fenceEndIndex].Trim() -eq '') {
                        $fenceEndIndex--
                    }
                    if ($fenceEndIndex -gt $fenceStartIndex -and $lines[$fenceEndIndex] -match '^```\s*$') {
                        $openingFence = $lines[$fenceStartIndex]
                        $closingFence = $lines[$fenceEndIndex]
                    }
                }
            }

            if ($openingFence) {
                $newLines += $openingFence
                foreach ($line in $regionLines) {
                    if ($line -eq '') {
                        $newLines += ''
                    } else {
                        $newLines += $line
                    }
                }
                $newLines += $closingFence
            } else {
                foreach ($line in $regionLines) {
                    if ($line -eq '') {
                        $newLines += ''
                    } else {
                        $newLines += "$codeBlockIndent$line"
                    }
                }
            }

            $newLines += $lines[$i]
            Write-Host "  [EMBED] '$regionName' -> $($mdFile.Name)"
        } else {
            $newLines += $lines[$i]
        }
        $i++
    }

    $newContent = $newLines -join "`n"

    # Normalize and compare content
    $normalizedOriginal = Normalize-Content $original
    $normalizedNew = Normalize-Content $newContent

    if ($normalizedOriginal -ne $normalizedNew) {
        # Write file with UTF-8 without BOM (available in PowerShell Core)
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText($mdFile.FullName, $newContent, $utf8NoBom)
        Write-Host "  [UPDATED] $($mdFile.Name) (meaningful changes)"
    } else {
        Write-Host "  [SKIP]   $($mdFile.Name) (no meaningful changes)"
    }
}

$unusedRegions = $regionMap.Keys | Where-Object { -not $usedRegions.ContainsKey($_) }
if ($unusedRegions.Count -gt 0) {
    Write-Host ""
    Write-Warning "Regions found in .cs files but no matching SNIPPET markers in any .md file:"
    foreach ($name in $unusedRegions) {
        Write-Warning "  - $name (from $($regionMap[$name].File))"
    }
}

Write-Host "`nDone."
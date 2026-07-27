<#
.SYNOPSIS
    Generates configuration reference documentation from compiled assemblies.
.DESCRIPTION
    Loads the specified assemblies, extracts public types, their methods and properties,
    as well as XML comments (if available), and generates a Markdown file with improved structure.
    Supports passing assembly paths and folders for dependency resolution.
.PARAMETER Configuration
    Build configuration (Release/Debug). Used only for forming default paths.
.PARAMETER TargetFramework
    Target framework (e.g., net10.0). Used only for default paths.
.PARAMETER AssemblyPaths
    Array of full paths to the main assemblies (OutboxFlow.dll, OutboxFlow.Postgres.dll, OutboxFlow.Kafka.dll).
    If not specified, default paths are used: src/.../bin/...
.PARAMETER ResolveRoots
    Array of folders to search for dependencies (dll) for AssemblyResolve resolution.
    If not specified, default folders are used (bin/... of each project).
.EXAMPLE
    .\generate-config-docs.ps1 -Configuration Release -TargetFramework net10.0
    Uses default paths.
.EXAMPLE
    .\generate-config-docs.ps1 -AssemblyPaths @("publish/OutboxFlow.dll","publish/OutboxFlow.Postgres.dll","publish/OutboxFlow.Kafka.dll") -ResolveRoots @("publish")
    Uses published assemblies and the publish folder for dependencies.
#>

param(
    [string]$Configuration = "Release",
    [string]$TargetFramework = "net10.0",
    [string[]]$AssemblyPaths = $null,
    [string[]]$ResolveRoots = $null
)

# Detect repository root (folder above scripts)
$RepoRoot = Split-Path -Parent $PSScriptRoot

# Set default values if parameters are not provided
if (-not $AssemblyPaths) {
    $AssemblyPaths = @(
        Join-Path $RepoRoot "src\OutboxFlow\bin\$Configuration\$TargetFramework\OutboxFlow.dll"
        Join-Path $RepoRoot "src\OutboxFlow.Postgres\bin\$Configuration\$TargetFramework\OutboxFlow.Postgres.dll"
        Join-Path $RepoRoot "src\OutboxFlow.Kafka\bin\$Configuration\$TargetFramework\OutboxFlow.Kafka.dll"
    )
}

if (-not $ResolveRoots) {
    $ResolveRoots = @(
        Join-Path $RepoRoot "src\OutboxFlow\bin\$Configuration\$TargetFramework"
        Join-Path $RepoRoot "src\OutboxFlow.Postgres\bin\$Configuration\$TargetFramework"
        Join-Path $RepoRoot "src\OutboxFlow.Kafka\bin\$Configuration\$TargetFramework"
    )
}

# Convert relative paths to absolute relative to $RepoRoot (if needed)
$AssemblyPaths = $AssemblyPaths | ForEach-Object {
    if ([System.IO.Path]::IsPathRooted($_)) { $_ } else { Join-Path $RepoRoot $_ }
}
$ResolveRoots = $ResolveRoots | ForEach-Object {
    if ([System.IO.Path]::IsPathRooted($_)) { $_ } else { Join-Path $RepoRoot $_ }
}

# Output file
$OutputFile = Join-Path $RepoRoot "docs\configuration.md"

# Collect all dll files that may be useful for dependency resolution
$allAssemblyFiles = $ResolveRoots |
    ForEach-Object { Get-ChildItem -Path $_ -Filter *.dll -Recurse -ErrorAction SilentlyContinue } |
    Select-Object -ExpandProperty FullName |
    Sort-Object -Unique

# Simple cache for Resolve to avoid scanning every time
$resolveMap = @{}
foreach ($f in $allAssemblyFiles) {
    $name = [System.IO.Path]::GetFileNameWithoutExtension($f)
    if (-not $resolveMap.ContainsKey($name)) {
        $resolveMap[$name] = $f
    }
}

# AssemblyResolve handler: search by name in the built cache
$handler = [System.ResolveEventHandler]{
    param($sender, $args)
    $shortName = $args.Name.Split(',')[0]
    if ($resolveMap.ContainsKey($shortName)) {
        try {
            return [System.Reflection.Assembly]::LoadFrom($resolveMap[$shortName])
        } catch {
            return $null
        }
    }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($handler)

function Get-FriendlyTypeName {
    param([System.Type]$Type)
    if (-not $Type) { return "void" }
    if ($Type.IsGenericType) {
        $name = $Type.Name.Split('`')[0]
        $args = ($Type.GetGenericArguments() | ForEach-Object { Get-FriendlyTypeName $_ }) -join ", "
        return "$name<$args>"
    }
    if ($Type.IsGenericParameter) { return $Type.Name }
    return $Type.Name
}

function Get-XmlDoc {
    param([System.Reflection.MemberInfo]$Member)
    $xmlPath = [System.IO.Path]::ChangeExtension($Member.Module.FullyQualifiedName, ".xml")
    if (-not (Test-Path $xmlPath)) { return "" }
    try {
        [xml]$xml = Get-Content $xmlPath -Raw
        $prefix = if ($Member.MemberType -eq "Constructor") { "M:" + $Member.DeclaringType.FullName + ".#ctor" }
                  elseif ($Member.MemberType -eq "Property") { "P:" + $Member.DeclaringType.FullName + "." + $Member.Name }
                  else { "M:" + $Member.DeclaringType.FullName + "." + $Member.Name }
        $memberNode = $xml.SelectSingleNode("//member[starts-with(@name, '$prefix')]")
        if ($memberNode -and $memberNode.summary) {
            return $memberNode.summary.Trim()
        }
    } catch {}
    return ""
}

function Get-TypeXmlDoc {
    param([System.Type]$Type)
    $xmlPath = [System.IO.Path]::ChangeExtension($Type.Module.FullyQualifiedName, ".xml")
    if (-not (Test-Path $xmlPath)) { return "" }
    try {
        [xml]$xml = Get-Content $xmlPath -Raw
        $memberNode = $xml.SelectSingleNode("//member[starts-with(@name, 'T:$($Type.FullName)')]")
        if ($memberNode -and $memberNode.summary) {
            return $memberNode.summary.Trim()
        }
    } catch {}
    return ""
}

# Function to check whether a member (method or property) should be excluded
function Should-ExcludeMember {
    param([System.Reflection.MemberInfo]$Member)
    # Exclude methods with CompilerGenerated or DebuggerNonUserCode attributes
    if ($Member.IsDefined([System.Runtime.CompilerServices.CompilerGeneratedAttribute], $false)) { return $true }
    if ($Member.IsDefined([System.Diagnostics.DebuggerNonUserCodeAttribute], $false)) { return $true }
    # For methods, additionally check the name and declaring type
    if ($Member -is [System.Reflection.MethodInfo]) {
        $method = $Member
        # Exclude methods from System.Object (Equals, GetHashCode, ToString, GetType)
        if ($method.DeclaringType -eq [System.Object]) { return $true }
        if ($method.Name -in @('Equals', 'GetHashCode', 'ToString', 'GetType', 'Finalize', 'MemberwiseClone')) { return $true }
        # Exclude explicit interface implementations (they usually contain a dot in the name)
        if ($method.Name -match '\.') { return $true }
    }
    return $false
}

Write-Host "Loading assemblies..."
$loadedAssemblies = @()
foreach ($path in $AssemblyPaths) {
    if (-not (Test-Path $path)) {
        Write-Host "Warning: Assembly not found at $path, skipping."
        continue
    }
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($path)
        $loadedAssemblies += $asm
        Write-Host "Loaded: $($asm.GetName().Name)"
    } catch {
        Write-Host "Warning: Could not load assembly $path - $_"
    }
}

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# Configuration Reference")
$lines.Add("")
$lines.Add("> Auto-generated from compiled assemblies. Run ``scripts\generate-config-docs.ps1`` to regenerate.")
$lines.Add("")

# ------------------------------------------------------------
# Collect data for table of contents and grouping by namespaces
# ------------------------------------------------------------
$allTypesList = @()  # list of all types with their assemblies for the table of contents

foreach ($a in $loadedAssemblies) {
    $assemblyName = $a.GetName().Name
    $lines.Add("## $assemblyName")
    $lines.Add("")

    # Safe type retrieval
    $allTypes = @()
    try {
        $allTypes = $a.GetExportedTypes() | Where-Object { $_.IsPublic -and -not $_.IsNested } | Sort-Object Name
    } catch [System.Reflection.ReflectionTypeLoadException] {
        $allTypes = $_.Types | Where-Object { $_ -ne $null -and $_.IsPublic -and -not $_.IsNested } | Sort-Object Name
        foreach ($ex in $_.LoaderExceptions) {
            Write-Host "Warning: Could not load a type from $assemblyName - $($ex.Message)"
        }
    } catch {
        Write-Host "Warning: Could not retrieve types from $assemblyName - $_"
        continue
    }

    $ordinaryTypes = $allTypes | Where-Object { -not ($_.IsSealed -and $_.IsAbstract -and $_.IsPublic) }
    $extensionTypes = $allTypes | Where-Object { $_.IsSealed -and $_.IsAbstract -and $_.IsPublic }

    # Save for table of contents
    $allTypesList += $ordinaryTypes | ForEach-Object { @{ Type = $_; Assembly = $assemblyName } }
    $allTypesList += $extensionTypes | ForEach-Object { @{ Type = $_; Assembly = $assemblyName } }

    # ------------------------
    # Regular types
    # ------------------------
    foreach ($type in $ordinaryTypes) {
        $typeName = $type.FullName
        $lines.Add("### $typeName")
        $lines.Add("")

        # Base type and interfaces
        if ($type.BaseType -and $type.BaseType.Name -ne "Object" -and $type.BaseType.Name -ne "ValueType") {
            $lines.Add("- **Base type:** ``$($type.BaseType.FullName)``")
        }
        try {
            $interfaces = $type.GetInterfaces() | ForEach-Object { $_.Name } | Where-Object { $_ -notin @("IDisposable", "IAsyncDisposable") }
        } catch {
            $interfaces = @()
        }
        if ($interfaces) {
            $lines.Add("- **Implements:** $($interfaces -join ', ')")
        }

        $doc = Get-TypeXmlDoc $type
        if ($doc) {
            $lines.Add("- **Description:** $doc")
        }
        $lines.Add("")

        # Properties (displayed before methods as they are shorter)
        try {
            $properties = $type.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance) |
                Where-Object { -not (Should-ExcludeMember $_) } |
                Sort-Object Name
        } catch {
            Write-Host "Warning: Could not retrieve properties for $($type.Name)"
            $properties = @()
        }
        if ($properties) {
            $lines.Add("#### Properties")
            $lines.Add("")
            foreach ($prop in $properties) {
                try {
                    $propTypeName = Get-FriendlyTypeName $prop.PropertyType
                } catch {
                    $propTypeName = "(unknown)"
                }
                $propDoc = Get-XmlDoc $prop
                $name = $prop.Name
                $line = "- **$name** : ``$propTypeName``"
                if ($propDoc) { $line += " — $propDoc" }
                $lines.Add($line)
            }
            $lines.Add("")
        }

        # Methods
        try {
            $methods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static) |
                Where-Object { -not $_.IsSpecialName -and -not (Should-ExcludeMember $_) } |
                Sort-Object Name
        } catch {
            Write-Host "Warning: Could not retrieve methods for $($type.Name)"
            $methods = @()
        }

        if ($methods) {
            $lines.Add("#### Methods")
            $lines.Add("")
            foreach ($method in $methods) {
                # Method name
                $methodName = $method.Name
                $methodDoc = Get-XmlDoc $method

                # Method heading
                $lines.Add("##### $methodName")
                $lines.Add("")

                # Return type
                $returnType = Get-FriendlyTypeName $method.ReturnType
                $lines.Add("- **Return type:** ``$returnType``")

                # Parameters
                try {
                    $params = $method.GetParameters()
                } catch {
                    $params = @()
                }
                if ($params.Count -gt 0) {
                    $lines.Add("- **Parameters:**")
                    foreach ($p in $params) {
                        $paramType = Get-FriendlyTypeName $p.ParameterType
                        $paramName = $p.Name
                        # Attempt to get parameter description from XML (can be extended)
                        $paramDoc = ""
                        $lines.Add("  - ``$paramName`` (``$paramType``)" + ($paramDoc ? " — $paramDoc" : ""))
                    }
                } else {
                    $lines.Add("- **Parameters:** none")
                }

                # Method description
                if ($methodDoc) {
                    $lines.Add("- **Description:** $methodDoc")
                }

                $lines.Add("")  # blank line between methods
            }
        }
        $lines.Add("")
    }

    # ------------------------
    # Extension types (static classes with extension methods)
    # ------------------------
    if ($extensionTypes) {
        $lines.Add("### Extension Methods")
        $lines.Add("")
        foreach ($extType in $extensionTypes) {
            try {
                $extMethods = $extType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static) |
                    Where-Object { -not $_.IsSpecialName -and -not (Should-ExcludeMember $_) } |
                    Sort-Object Name
            } catch {
                Write-Host "Warning: Could not retrieve extension methods for $($extType.Name)"
                $extMethods = @()
            }

            if ($extMethods) {
                foreach ($method in $extMethods) {
                    if (-not $method.IsStatic) { continue }
                    $methodName = $method.Name
                    $methodDoc = Get-XmlDoc $method
                    $lines.Add("#### $methodName")
                    $lines.Add("")

                    $returnType = Get-FriendlyTypeName $method.ReturnType
                    $lines.Add("- **Return type:** ``$returnType``")

                    try {
                        $params = $method.GetParameters()
                    } catch {
                        $params = @()
                    }
                    if ($params.Count -gt 0) {
                        $lines.Add("- **Parameters:**")
                        foreach ($p in $params) {
                            $paramType = Get-FriendlyTypeName $p.ParameterType
                            $paramName = $p.Name
                            $lines.Add("  - ``$paramName`` (``$paramType``)")
                        }
                    } else {
                        $lines.Add("- **Parameters:** none")
                    }
                    if ($methodDoc) {
                        $lines.Add("- **Description:** $methodDoc")
                    }
                    $lines.Add("")
                }
            }
        }
    }
}

# ------------------------------------------------------------
# Add table of contents to the beginning of the document
# ------------------------------------------------------------
$tocLines = New-Object System.Collections.Generic.List[string]
$tocLines.Add("# Configuration Reference")
$tocLines.Add("")
$tocLines.Add("> Auto-generated from compiled assemblies. Run ``scripts\generate-config-docs.ps1`` to regenerate.")
$tocLines.Add("")
$tocLines.Add("## Table of Contents")
$tocLines.Add("")

# Group by assemblies
$assemblies = $allTypesList | Group-Object { $_.Assembly } | Sort-Object Name
foreach ($asmGroup in $assemblies) {
    $asmName = $asmGroup.Name
    $tocLines.Add("### $asmName")
    foreach ($item in $asmGroup.Group | Sort-Object { $_.Type.FullName }) {
        $type = $item.Type
        $typeFullName = $type.FullName
        $anchor = $typeFullName -replace '[^a-zA-Z0-9\-_]', '-'  # Simple anchor conversion
        $tocLines.Add("- [$typeFullName](#$anchor)")
    }
    $tocLines.Add("")
}

# Combine table of contents and main content
$finalLines = New-Object System.Collections.Generic.List[string]
$finalLines.AddRange($tocLines)
$finalLines.Add("---")
$finalLines.Add("")
$finalLines.AddRange($lines)

# Remove old handler and write
[System.AppDomain]::CurrentDomain.remove_AssemblyResolve($handler)
$finalLines | Out-File -FilePath $OutputFile -Encoding utf8
Write-Host "Configuration reference written to $OutputFile"
param(
    [string]$Configuration = "Release",
    [string]$TargetFramework = "net10.0"
)

$RepoRoot = Split-Path -Parent $PSScriptRoot
$OutputFile = Join-Path $RepoRoot "docs\configuration.md"
$Sep = "|"

$AssemblyPaths = @(
    Join-Path $RepoRoot "src\OutboxFlow\bin\$Configuration\$TargetFramework\OutboxFlow.dll"
    Join-Path $RepoRoot "src\OutboxFlow.Postgres\bin\$Configuration\$TargetFramework\OutboxFlow.Postgres.dll"
    Join-Path $RepoRoot "src\OutboxFlow.Kafka\bin\$Configuration\$TargetFramework\OutboxFlow.Kafka.dll"
)

$ResolvePaths = @(
    Join-Path $RepoRoot "src\OutboxFlow\bin\$Configuration\$TargetFramework"
    Join-Path $RepoRoot "src\OutboxFlow.Postgres\bin\$Configuration\$TargetFramework"
    Join-Path $RepoRoot "src\OutboxFlow.Kafka\bin\$Configuration\$TargetFramework"
)

$runtimeDir = [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()

$resolveCache = @{}
$resolveLock = [System.Threading.Thread]::GetDomain().GetData("resolveLock")
if (-not $resolveLock) {
    $resolveLock = [System.Threading.Thread]::GetDomain()
    [System.Threading.Thread]::GetDomain().SetData("resolveLock", [System.Object]::new())
}
$resolveLock = [System.Threading.Thread]::GetDomain().GetData("resolveLock")

$resolver = [System.ResolveEventHandler]{
    param($sender, $args)
    $name = $args.Name.Split(',')[0]
    $sync = [System.Threading.Thread]::GetDomain().GetData("resolveLock")
    $cache = [System.Threading.Thread]::GetDomain().GetData("resolveCache")
    if (-not $cache) { return $null }
    if ($cache.ContainsKey($name)) { return $cache[$name] }
    foreach ($dir in @($ResolvePaths + $runtimeDir)) {
        $path = Join-Path $dir "$name.dll"
        if (Test-Path $path) {
            try {
                $cache[$name] = $null
                $asm = [System.Reflection.Assembly]::LoadFrom($path)
                $cache[$name] = $asm
                return $asm
            } catch {
                $cache[$name] = $null
                return $null
            }
        }
    }
    $cache[$name] = $null
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
[System.Threading.Thread]::GetDomain().SetData("resolveCache", $resolveCache)

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

Write-Host "Loading assemblies..."
$allAssemblies = @()
foreach ($path in $AssemblyPaths) {
    try {
        $allAssemblies += [System.Reflection.Assembly]::LoadFrom($path)
    } catch {
        Write-Host "Warning: Could not load $path - $_"
    }
}

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# Configuration Reference")
$lines.Add("")
$lines.Add("> Auto-generated from compiled assemblies. Run ``scripts\generate-config-docs.ps1`` to regenerate.")
$lines.Add("")

foreach ($a in $allAssemblies) {
    $assemblyName = $a.GetName().Name
    $lines.Add("## $assemblyName")
    $lines.Add("")

    $allTypes = $a.GetExportedTypes() | Where-Object { $_.IsPublic -and -not $_.IsNested } | Sort-Object Name
    $ordinaryTypes = $allTypes | Where-Object { -not ($_.IsSealed -and $_.IsAbstract -and $_.IsPublic) }
    $extensionTypes = $allTypes | Where-Object { $_.IsSealed -and $_.IsAbstract -and $_.IsPublic }

    foreach ($type in $ordinaryTypes) {
        $doc = Get-TypeXmlDoc $type
        $lines.Add("### $($type.Name)")
        if ($type.BaseType -and $type.BaseType.Name -ne "Object" -and $type.BaseType.Name -ne "ValueType") {
            $lines.Add("- **Base type**: $($type.BaseType.FullName)")
        }
        $interfaces = $type.GetInterfaces() | ForEach-Object { $_.Name } | Where-Object { $_ -notin @("IDisposable", "IAsyncDisposable") }
        if ($interfaces) {
            $lines.Add("- **Implements**: $($interfaces -join ', ')")
        }
        if ($doc) { $lines.Add("- $doc") }
        $lines.Add("")

        $methods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static) `
            | Where-Object { -not $_.IsSpecialName } | Sort-Object Name

        if ($methods) {
            $hdr = @(" Method ", " Return Type ", " Parameters ") -join $Sep
            $lines.Add($Sep + $hdr + $Sep)
            $lines.Add(($Sep, "---", "---", "---") -join $Sep)
            foreach ($method in $methods) {
                $params = $method.GetParameters()
                $paramStr = ($params | ForEach-Object { "$(Get-FriendlyTypeName $_.ParameterType) $($_.Name)" }) -join ", "
                if ([string]::IsNullOrEmpty($paramStr)) { $paramStr = "(none)" }
                $returnType = Get-FriendlyTypeName $method.ReturnType
                $methodDoc = Get-XmlDoc $method
                $methodName = $method.Name
                if ($methodDoc) { $methodName = "$methodName - $methodDoc" }
                $row = @(" $methodName ", " $returnType ", " $paramStr ") -join $Sep
                $lines.Add($Sep + $row + $Sep)
            }
            $lines.Add("")
        }

        $properties = $type.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance) | Sort-Object Name
        if ($properties) {
            $hdr = @(" Property ", " Type ") -join $Sep
            $lines.Add($Sep + $hdr + $Sep)
            $lines.Add(($Sep, "---", "---") -join $Sep)
            foreach ($prop in $properties) {
                $propDoc = Get-XmlDoc $prop
                $name = $prop.Name
                if ($propDoc) { $name = "$name - $propDoc" }
                $row = @(" $name ", " $(Get-FriendlyTypeName $prop.PropertyType) ") -join $Sep
                $lines.Add($Sep + $row + $Sep)
            }
            $lines.Add("")
        }
    }

    if ($extensionTypes) {
        $lines.Add("### Extension Methods")
        $lines.Add("")

        foreach ($extType in $extensionTypes) {
            $extMethods = $extType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static) `
                | Sort-Object Name

            if ($extMethods) {
                $hdr = @(" Method ", " Return Type ", " Parameters ") -join $Sep
                $lines.Add($Sep + $hdr + $Sep)
                $lines.Add(($Sep, "---", "---", "---") -join $Sep)
                foreach ($method in $extMethods) {
                    if (-not $method.IsStatic) { continue }
                    $params = $method.GetParameters()
                    $paramStrs = @()
                    foreach ($p in $params) {
                        $tName = Get-FriendlyTypeName $p.ParameterType
                        $paramStrs += "$tName $($p.Name)"
                    }
                    $paramStr = $paramStrs -join ", "
                    $returnType = Get-FriendlyTypeName $method.ReturnType
                    $methodDoc = Get-XmlDoc $method
                    $methodName = $method.Name
                    if ($methodDoc) { $methodName = "$methodName - $methodDoc" }
                    $row = @(" $methodName ", " $returnType ", " $paramStr ") -join $Sep
                    $lines.Add($Sep + $row + $Sep)
                }
                $lines.Add("")
            }
        }
    }
}

$lines | Out-File -FilePath $OutputFile -Encoding utf8
Write-Host "Configuration reference written to $OutputFile"
[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidateSet('Apply', 'Revert')]
    [string] $Action = 'Apply',

    [string] $ProjectPath = (Get-Location).Path,

    [string] $UnityEditorPath,

    [string] $UnityVersion = '6000.3.19f1',

    [string] $PackagePath,

    [string] $DotnetPath = 'dotnet',

    [switch] $AllowPrerelease,

    [switch] $SkipProcessCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-ExistingPath {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [string] $Description = 'path'
    )

    $resolved = Resolve-Path -LiteralPath $Path -ErrorAction SilentlyContinue
    if ($null -eq $resolved) {
        throw "Could not find ${Description}: $Path"
    }

    return $resolved.Path
}

function Find-PatchPackage {
    param(
        [Parameter(Mandatory)]
        [string] $Root
    )

    $embedded = Join-Path $Root 'Packages/com.kandreyc.unity-csharp-patch'
    if (Test-Path -LiteralPath $embedded -PathType Container) {
        return (Resolve-ExistingPath -Path $embedded -Description 'embedded Unity CSharp Patch package')
    }

    $cache = Join-Path $Root 'Library/PackageCache'
    $cachedPackage = Get-ChildItem -LiteralPath $cache -Directory -Filter 'com.kandreyc.unity-csharp-patch@*' -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -ne $cachedPackage) {
        return $cachedPackage.FullName
    }

    throw 'Unity CSharp Patch package was not found. Open the project once so Unity can resolve Packages/manifest.json, or pass -PackagePath explicitly.'
}

function Find-UnityEditor {
    param(
        [Parameter(Mandatory)]
        [string] $Root
    )

    $version = $UnityVersion
    $searchRoots = switch (Get-HostPlatform) {
        'Windows' {
            @(
                (Join-Path ${env:ProgramFiles} 'Unity/Hub/Editor'),
                (Join-Path ${env:LOCALAPPDATA} 'Programs/Unity/Hub/Editor'),
                (Join-Path ${env:LOCALAPPDATA} 'UnityHub/Editor'),
                (Join-Path ${env:APPDATA} 'UnityHub/Editor'),
                (Join-Path ${env:USERPROFILE} 'Unity/Hub/Editor')
            )
        }
        'MacOS' {
            @(
                '/Applications/Unity/Hub/Editor',
                (Join-Path ${env:HOME} 'Unity/Hub/Editor')
            )
        }
        default {
            @(
                (Join-Path ${env:HOME} 'Unity/Hub/Editor'),
                '/opt/Unity/Hub/Editor'
            )
        }
    }

    $searchRoots = @($searchRoots | Where-Object { $_ })
    $searchRoots += Get-UnityHubConfiguredRoots
    $candidates = @(foreach ($rootPath in ($searchRoots | Select-Object -Unique)) {
        if (Test-Path -LiteralPath $rootPath -PathType Container) {
            Join-Path $rootPath $version
        }
    })
    $candidates += Get-UnityHubRegisteredEditorRoots -Version $version

    foreach ($candidate in $candidates) {
        Write-Verbose "Testing Unity editor candidate: $candidate"
        $executable = Get-UnityExecutableCandidates -EditorPath $candidate |
            Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } |
            Select-Object -First 1
        if ($null -ne $executable) {
            return (Resolve-ExistingPath -Path $candidate -Description "Unity $version editor")
        }
    }

    throw "Unity $version was not found in Unity Hub locations or Hub configuration. Pass -UnityEditorPath explicitly, or change -UnityVersion."
}

function Get-UnityHubRegisteredEditorRoots {
    param(
        [Parameter(Mandatory)]
        [string] $Version
    )

    $configFiles = @()
    if ($env:APPDATA) {
        $configFiles += Join-Path $env:APPDATA 'UnityHub/editors.json'
        $configFiles += Join-Path $env:APPDATA 'UnityHub/editors-v2.json'
    }
    if ($env:XDG_CONFIG_HOME) {
        $configFiles += Join-Path $env:XDG_CONFIG_HOME 'UnityHub/editors.json'
        $configFiles += Join-Path $env:XDG_CONFIG_HOME 'UnityHub/editors-v2.json'
    }
    elseif ($env:HOME) {
        $configFiles += Join-Path $env:HOME '.config/UnityHub/editors.json'
        $configFiles += Join-Path $env:HOME '.config/UnityHub/editors-v2.json'
    }

    foreach ($configFile in ($configFiles | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf })) {
        Write-Verbose "Reading Unity Hub editor registry: $configFile"
        try {
            $json = Get-Content -LiteralPath $configFile -Raw | ConvertFrom-Json
            $dataProperty = $json.PSObject.Properties['data']
            $entries = if ($null -ne $dataProperty) {
                @($dataProperty.Value)
            }
            else {
                @($json.PSObject.Properties | ForEach-Object { $_.Value })
            }

            foreach ($entry in $entries) {
                $versionProperty = $entry.PSObject.Properties['version']
                $locationProperty = $entry.PSObject.Properties['location']
                if ($null -eq $versionProperty -or $versionProperty.Value -ne $Version -or $null -eq $locationProperty) {
                    continue
                }

                foreach ($path in @($locationProperty.Value)) {
                    Write-Verbose "Unity Hub registry location for $Version`: $path"
                    if (-not (Test-Path -LiteralPath $path)) {
                        Write-Verbose "Registered Unity location does not exist: $path"
                        continue
                    }

                    $resolved = (Resolve-Path -LiteralPath $path).Path
                    if (Test-Path -LiteralPath $resolved -PathType Leaf) {
                        Write-Verbose "Registered Unity executable found: $resolved"
                        $directory = Split-Path -Parent $resolved
                        if ((Split-Path -Leaf $directory) -eq 'Editor') {
                            Write-Output (Split-Path -Parent $directory)
                        }
                        else {
                            Write-Output $directory
                        }
                    }
                    elseif ((Split-Path -Leaf $resolved) -eq 'Editor') {
                        Write-Output (Split-Path -Parent $resolved)
                    }
                    else {
                        Write-Output $resolved
                    }
                }
            }
        }
        catch {
            Write-Verbose "Could not read Unity Hub editor registry: $configFile"
        }
    }
}

function Get-UnityHubConfiguredRoots {
    $configFiles = @()
    if ($env:APPDATA) {
        $configFiles += Join-Path $env:APPDATA 'UnityHub/secondaryInstallPath.json'
        $configFiles += Join-Path $env:APPDATA 'UnityHub/config.json'
    }
    if ($env:XDG_CONFIG_HOME) {
        $configFiles += Join-Path $env:XDG_CONFIG_HOME 'UnityHub/secondaryInstallPath.json'
    }
    elseif ($env:HOME) {
        $configFiles += Join-Path $env:HOME '.config/UnityHub/secondaryInstallPath.json'
    }

    $configFiles = $configFiles | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf }

    foreach ($configFile in $configFiles) {
        try {
            $json = Get-Content -LiteralPath $configFile -Raw | ConvertFrom-Json
            foreach ($path in (Get-JsonStringValues -Value $json)) {
                if (Test-Path -LiteralPath $path -PathType Container) {
                    Write-Output $path
                    Write-Output (Join-Path $path 'Hub/Editor')
                    Write-Output (Join-Path $path 'Editor')
                }
            }
        }
        catch {
            Write-Verbose "Could not read Unity Hub configuration: $configFile"
        }
    }
}

function Get-JsonStringValues {
    param(
        [Parameter(Mandatory)]
        [object] $Value
    )

    if ($Value -is [string]) {
        return $Value
    }

    if ($Value -is [System.Collections.IEnumerable]) {
        foreach ($item in $Value) {
            Get-JsonStringValues -Value $item
        }
        return
    }

    if ($null -ne $Value.PSObject) {
        foreach ($property in $Value.PSObject.Properties) {
            Get-JsonStringValues -Value $property.Value
        }
    }
}

function Get-HostPlatform {
    $platform = [System.Environment]::OSVersion.Platform
    if ($platform -eq [System.PlatformID]::Win32NT) {
        return 'Windows'
    }

    if ($platform -eq [System.PlatformID]::MacOSX) {
        return 'MacOS'
    }

    if ($env:OSTYPE -like 'darwin*') {
        return 'MacOS'
    }

    return 'Linux'
}

function Get-UnityExecutableCandidates {
    param(
        [Parameter(Mandatory)]
        [string] $EditorPath
    )

    switch (Get-HostPlatform) {
        'Windows' {
            return @(
                (Join-Path $EditorPath 'Editor/Unity.exe'),
                (Join-Path $EditorPath 'Unity.exe')
            )
        }
        'MacOS' {
            return @(
                (Join-Path $EditorPath 'Unity.app/Contents/MacOS/Unity'),
                (Join-Path $EditorPath 'Contents/MacOS/Unity')
            )
        }
        default {
            return @(
                (Join-Path $EditorPath 'Editor/Unity'),
                (Join-Path $EditorPath 'Unity')
            )
        }
    }
}

function Test-DotnetRuntime {
    param(
        [Parameter(Mandatory)]
        [string] $Command
    )

    $dotnetCommand = Get-Command $Command -ErrorAction SilentlyContinue
    if ($null -eq $dotnetCommand) {
        throw "dotnet was not found: $Command"
    }

    $runtimes = & $dotnetCommand.Source --list-runtimes 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Could not query installed dotnet runtimes using $Command."
    }

    if (-not ($runtimes | Where-Object { $_ -match '^Microsoft\.NETCore\.App\s+10\.' })) {
        throw @'
The UnityEditorPatch tool targets net10.0 and Microsoft.NETCore.App 10.x was not found.
Install the .NET 10 SDK or runtime from:
https://dotnet.microsoft.com/download/dotnet/10.0

Then verify with "dotnet --list-runtimes" and run this script again. If dotnet is installed in a custom location, pass that executable with -DotnetPath.
'@
    }
}

function Test-UnityClosed {
    if ($SkipProcessCheck) {
        return
    }

    $unityProcesses = Get-Process -Name Unity -ErrorAction SilentlyContinue
    if ($null -ne $unityProcesses) {
        throw 'A Unity editor process is running. Close Unity before applying or reverting the patch, or pass -SkipProcessCheck.'
    }
}

$root = Resolve-ExistingPath -Path $ProjectPath -Description 'Unity project'
if (-not (Test-Path -LiteralPath (Join-Path $root 'Assets') -PathType Container) -or
    -not (Test-Path -LiteralPath (Join-Path $root 'ProjectSettings') -PathType Container)) {
    throw "The path is not a Unity project: $root"
}

$package = if ($PackagePath) {
    Resolve-ExistingPath -Path $PackagePath -Description 'Unity CSharp Patch package'
} else {
    Find-PatchPackage -Root $root
}

$patchDll = Join-Path $package 'EditorPatch~/UnityEditorPatch.dll'
if (-not (Test-Path -LiteralPath $patchDll -PathType Leaf)) {
    throw "UnityEditorPatch.dll was not found in the package: $patchDll"
}

$editor = if ($UnityEditorPath) {
    Resolve-ExistingPath -Path $UnityEditorPath -Description 'Unity editor'
} else {
    Find-UnityEditor -Root $root
}

$unityExecutable = Get-UnityExecutableCandidates -EditorPath $editor |
    Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } |
    Select-Object -First 1
if ($null -eq $unityExecutable) {
    throw "The Unity editor executable was not found under: $editor"
}

Test-DotnetRuntime -Command $DotnetPath
Test-UnityClosed

$verb = $Action.ToLowerInvariant()
$arguments = @(
    $patchDll,
    $verb,
    '--editor',
    $editor
)
if ($AllowPrerelease -and $Action -eq 'Apply') {
    $arguments += '--allow-prerelease'
}

Write-Host "Unity CSharp Patch: $verb"
Write-Host "Editor: $editor"
Write-Host "Package: $package"

if ($PSCmdlet.ShouldProcess($editor, "$verb Unity CSharp Patch")) {
    Push-Location (Split-Path -Parent $patchDll)
    try {
        & $DotnetPath @arguments
        if ($LASTEXITCODE -ne 0) {
            throw "UnityEditorPatch exited with code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

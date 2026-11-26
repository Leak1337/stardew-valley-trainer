param(
    [string]$GamePath = 'D:\Games\Stardew-Valley\Stardew Valley',
    [string]$SmapiAssembly = (Join-Path $GamePath 'StardewModdingAPI.dll')
)

$ErrorActionPreference = 'Stop'

$sdkEntry = dotnet --list-sdks | Select-Object -Last 1
if ($sdkEntry -notmatch '^([^ ]+) \[(.+)\]$') {
    throw 'A .NET SDK is required to build the mod.'
}
$compiler = Join-Path $Matches[2] "$($Matches[1])\Roslyn\bincore\csc.dll"

# Compile against the game's bundled .NET 6 assemblies, even with a newer SDK.
$references = @(
    'System.Private.CoreLib.dll'
    'System.Runtime.dll'
    'System.Linq.dll'
    'System.Collections.dll'
    'netstandard.dll'
    'Stardew Valley.dll'
    'StardewValley.GameData.dll'
    'MonoGame.Framework.dll'
    'xTile.dll'
) | ForEach-Object { Join-Path $GamePath $_ }
$references += $SmapiAssembly
foreach ($reference in $references) {
    if (-not (Test-Path -LiteralPath $reference -PathType Leaf)) {
        throw "Missing build reference: $reference"
    }
}

$destination = Join-Path $PSScriptRoot 'dist\StardewValleyTrainer'
New-Item -ItemType Directory -Force -Path $destination | Out-Null
$assembly = Join-Path $destination 'StardewValleyTrainer.dll'
$referenceArgs = $references | ForEach-Object { "/reference:$_" }
$sources = Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' -File | ForEach-Object FullName
dotnet $compiler /nologo /noconfig /nostdlib+ /target:library /optimize+ /nullable:enable /codepage:65001 "/out:$assembly" @referenceArgs @sources
if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE."
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'manifest.json') -Destination $destination
$translations = Join-Path $destination 'i18n'
New-Item -ItemType Directory -Force -Path $translations | Out-Null
Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'i18n') -Filter '*.json' -File | Copy-Item -Destination $translations
Write-Output $destination

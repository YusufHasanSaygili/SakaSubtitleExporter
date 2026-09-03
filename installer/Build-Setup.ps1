[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repositoryRoot 'SakaSubtitleExporter.sln'
$testProject = Join-Path $repositoryRoot 'tests\SakaSubtitleExporter.Tests\SakaSubtitleExporter.Tests.csproj'
$sourceExe = Join-Path $repositoryRoot "src\SakaSubtitleExporter\bin\$Configuration\net48\SakaSubtitleExporter.exe"
$setupDirectory = Join-Path $repositoryRoot 'artifacts\setup'
$setupScript = Join-Path $PSScriptRoot 'SakaSubtitleExporter.iss'

dotnet build $solutionPath -c $Configuration
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }

dotnet run --project $testProject -c $Configuration --no-build
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }

New-Item -ItemType Directory -Force -Path $setupDirectory | Out-Null

$compilerCandidates = @(
    (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'),
    (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
    (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe')
)
$compiler = $compilerCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $compiler) {
    throw 'Inno Setup 6 was not found. Install it with: winget install JRSoftware.InnoSetup'
}

& $compiler "/DSourceExe=$sourceExe" "/DOutputDir=$setupDirectory" $setupScript
if ($LASTEXITCODE -ne 0) { throw 'The setup package could not be created.' }

Write-Host "Ready: $(Join-Path $setupDirectory 'SakaSubtitleExporterSetup.exe')"

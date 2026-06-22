$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot '..\src\JsonEditor.App\JsonEditor.App.csproj'
$output = Join-Path $PSScriptRoot '..\dist\JsonEditor-win-x64'
$launcher = Join-Path $PSScriptRoot '..\JsonEditor.exe'

Write-Host "Publishing JsonEditor.App to $output ..."

dotnet publish $project `
  -c Release `
  -r win-x64 `
  -p:PublishSingleFile=true `
  -p:SelfContained=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o $output

Copy-Item -Force (Join-Path $output 'JsonEditor.App.exe') $launcher

Write-Host "Done. Launch with: $launcher"

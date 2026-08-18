# ==============================================================================
# Focus Dimmer: Microsoft Store Package Build Script (Local)
# ==============================================================================
# 実行方法:
#   PowerShell で .\build-store-package.ps1 を実行
# 出力先:
#   .\AppPackages\ 配下に .msixupload ファイルが生成されます
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " Focus Dimmer Store Package Builder" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. 出力先ディレクトリの準備
$OutputDir = Join-Path $PSScriptRoot "AppPackages"
if (Test-Path $OutputDir) {
    Write-Host "[1/3] 出力ディレクトリをクリーンアップしています..." -ForegroundColor Yellow
    Remove-Item -Path $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# 2. Visual Studio / MSBuild の検出
Write-Host "[2/3] MSBuild を検出しています..." -ForegroundColor Yellow
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) {
    Write-Error "vswhere.exe が見つかりませんでした。Visual Studio が正しくインストールされているか確認してください。"
}

$msbuildPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
if (-not $msbuildPath) {
    Write-Error "MSBuild.exe が見つかりませんでした。"
}
Write-Host "  -> MSBuild を検出: $msbuildPath" -ForegroundColor Gray

# 3. Store パッケージのビルド
Write-Host "[3/3] Microsoft Store 用パッケージ (.msixupload) をビルド中..." -ForegroundColor Yellow
$wapproj = Join-Path $PSScriptRoot "FocusDimmer.Package\FocusDimmer.Package.wapproj"

& $msbuildPath $wapproj `
    /p:Configuration=Release `
    /p:Platform=x64 `
    /p:UapAppxPackageBuildMode=StoreUpload `
    /p:AppxBundlePlatforms="x64" `
    /p:AppxBundle=Always `
    /p:AppxPackageDir="$OutputDir\" `
    /p:GenerateAppxPackageOnBuild=true `
    /v:m

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host " ビルド成功！" -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Green
    
    $packages = Get-ChildItem -Path $OutputDir -Filter "*.msixupload" -Recurse
    if ($packages) {
        Write-Host "生成された Store アップロード用パッケージ:" -ForegroundColor Cyan
        foreach ($pkg in $packages) {
            Write-Host "  📦 $($pkg.FullName)" -ForegroundColor White
        }
        Write-Host ""
        Write-Host "Partner Center の申請画面に上記ファイルをドラッグ＆ドロップしてください。" -ForegroundColor Green
    }
} else {
    Write-Error "パッケージのビルドに失敗しました。"
}

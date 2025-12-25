# 読み込み対象の拡張子
$extensions = @(".cs", ".xaml", ".xml", ".appxmanifest")
# 除外するフォルダ
$excludeDirs = @("bin", "obj", ".vs", "Properties", "Assets")

$output = ""
Get-ChildItem -Recurse | Where-Object { 
    $ext = $_.Extension
    $dir = $_.DirectoryName
    # 除外フォルダに含まれていない、かつ指定拡張子である
    ($extensions -contains $ext) -and !($excludeDirs | Where-Object { $dir -match "\\$_\\" }) 
} | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $output += "`n`n--- FILE: $($_.Name) ---`n$content"
}

# クリップボードにコピー
Set-Clipboard $output
Write-Host "ソースコードをクリップボードにコピーしました。Geminiに貼り付けてください。"
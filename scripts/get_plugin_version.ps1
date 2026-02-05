# 功能说明：从 plugin.json 读取 Version 并写入指定文件（供 MSBuild 使用）

param(
    [Parameter(Mandatory = $true)]
    [string]$PluginJsonPath,
    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$json = Get-Content -Path $PluginJsonPath -Raw | ConvertFrom-Json
$json.Version.Trim() | Set-Content -Path $OutputPath -Encoding utf8 -NoNewline

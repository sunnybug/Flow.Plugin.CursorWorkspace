# 功能说明：检查 Flow Launcher 日志中 Cursor Workspaces 插件与 SSH 发现相关条目

$logPath = Join-Path $env:APPDATA "FlowLauncher\Logs"

if (-not (Test-Path $logPath)) {
    Write-Host "Log directory not found: $logPath"
    exit 1
}

$latestVersionDir = Get-ChildItem $logPath -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $latestVersionDir) {
    Write-Host "No version directory found under: $logPath"
    exit 1
}

$latestLog = Get-ChildItem -Path $latestVersionDir.FullName -Filter '*.txt' -Recurse -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($latestLog) {
    Write-Host "Latest log file: $($latestLog.FullName)"
    Write-Host "Modified time: $($latestLog.LastWriteTime)"
    Write-Host ""
    Write-Host "=== CursorWorkspaces and SSH Related Logs ==="
    Write-Host ""

    $content = Get-Content $latestLog.FullName -Tail 500

    $relevantLogs = $content | Where-Object {
        $_ -match 'CursorWorkspaces|VSCodeRemoteMachines|SSH|远程机器'
    }

    if ($relevantLogs) {
        $relevantLogs | ForEach-Object { Write-Host $_ }
    } else {
        Write-Host "No relevant logs found"
        Write-Host ""
        Write-Host "=== All Log Content (Last 100 Lines) ==="
        Get-Content $latestLog.FullName -Tail 100 | ForEach-Object { Write-Host $_ }
    }
} else {
    Write-Host "No log files found"
}

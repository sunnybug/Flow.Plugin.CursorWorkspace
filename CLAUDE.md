# CLAUDE.md

本文件为在本地仓库中编写代码时的 Claude Code (claude.ai/code) 提供指引。

## 项目概述

这是一个 Flow Launcher 插件，用于快速打开 Cursor 中最近使用的工作区。支持：
- 本地工作区（文件夹）
- 远程工作区（WSL/SSH/Codespaces）
- 自定义工作区

## 构建命令

```bash
# 还原依赖
dotnet restore

# Debug 构建
dotnet build -c Debug

# Release 构建
dotnet build -c Release

# 发布插件（打包成 zip）
dotnet publish Flow.Plugin.CursorWorkspaces.csproj -r win-x64 -c Release -o "Flow.Plugin.CursorWorkspaces-<version>"
```

构建输出位置：
- Debug: `Output\Debug\CursorWorkspaces\`
- Release: `Output\Release\CursorWorkspaces\`

项目要求：
- .NET 8.0 Windows 目标框架 (`net8.0-windows`)
- x64 平台
- 警告视为错误 (`TreatWarningsAsErrors=true`)

## 代码架构

### 核心入口点
- **[Main.cs](Main.cs)** - 实现 Flow Launcher 的插件接口（`IPlugin`, `IPluginI18n`, `ISettingProvider`, `IContextMenu`）
  - `Query()` - 根据用户输入搜索工作区
  - `Init()` - 初始化插件，加载 Cursor/VSCode 实例
  - `LoadContextMenus()` - 为本地工作区提供右键菜单（打开文件夹）

### 主要组件

#### [VSCodeHelper/VSCodeInstances.cs](VSCodeHelper/VSCodeInstances.cs)
- 扫描系统 PATH 等查找 Cursor 使用的 VSCode 兼容实例
- 图标与 folder/monitor 合成
- 每个实例包含：`ExecutablePath`, `AppData`, `WorkspaceIcon`, `RemoteIcon`

#### [WorkspacesHelper/VSCodeWorkspacesApi.cs](WorkspacesHelper/VSCodeWorkspacesApi.cs)（类名 CursorWorkspacesApi）
- 从 Cursor 的 storage.json / state.vscdb 读取最近打开的工作区
- 解析 VSCode URI 格式：`file:///`, `vscode-remote://`, `vscode-local://`
- 支持用户自定义工作区（设置中的 URI 列表）

#### [RemoteMachinesHelper/VSCodeRemoteMachinesApi.cs](RemoteMachinesHelper/VSCodeRemoteMachinesApi.cs)
- 读取 SSH 配置（如 `remote.SSH.configFile`）
- 使用 [SshConfigParser](SshConfigParser/) 解析 SSH 主机
- 为每个 SSH 主机创建远程工作区结果（通过 Cursor 的 `cursor` 命令行连接）

#### [SshConfigParser/SshConfig.cs](SshConfigParser/SshConfig.cs)
- 解析标准 SSH 配置（Host, HostName, User 等）

### 数据模型

- **[VSCodeWorkspace.cs](WorkspacesHelper/VSCodeWorkspace.cs)**（CursorWorkspace）- 工作区信息（路径、类型、关联实例）
- **[VSCodeInstance.cs](VSCodeHelper/VSCodeInstance.cs)** - 编辑器实例信息
- **[VSCodeRemoteMachine.cs](RemoteMachinesHelper/VSCodeRemoteMachine.cs)** - 远程机器信息

### 设置系统

- **[Settings.cs](Settings.cs)** - 用户配置：
  - `DiscoverWorkspaces` - 自动发现工作区
  - `DiscoverMachines` - 自动发现远程机器
  - `CursorExecutablePath` - Cursor 可执行文件路径（留空则使用 PATH 中的 Cursor.exe）
  - `CustomWorkspaces` - 用户自定义工作区 URI 列表
- **[SettingsView.xaml](SettingsView.xaml)** + **[SettingsView.xaml.cs](SettingsView.xaml.cs)** - WPF 设置界面

## 开发注意事项

1. **版本同步**：修改功能后需同步更新 [plugin.json](plugin.json) 中的 `Version` 与 [README.md](README.md)。

2. **错误处理**：配置文件解析失败时通过 `PluginLogger` 或 `Context.API.LogException()` 记录，不抛出异常中断插件。

3. **图标资源**：图标在 [Images/](Images/) 目录，构建时复制到输出目录。

4. **国际化**：字符串在 [Properties/Resources.resx](Properties/Resources.resx)，通过 `IPluginI18n` 支持多语言。

5. **去重**：工作区结果使用 `Distinct()` 去重。

6. **Ctrl+点击**：本地工作区按住 Ctrl 点击会打开文件夹而不是 Cursor。

7. **警告即错误**：项目开启 `TreatWarningsAsErrors=true`，需无警告才能通过编译。

## 源代码来源

逻辑参考 [Flow.Plugin.VSCodeWorkspace](https://github.com/sunnybug/Flow.Plugin.VSCodeWorkspace) 与 [Microsoft PowerToys VSCodeWorkspaces](https://github.com/microsoft/PowerToys/tree/main/src/modules/launcher/Plugins/Community.PowerToys.Run.Plugin.VSCodeWorkspaces)，已适配 Flow Launcher API 与 Cursor 命令行（`cursor`）。

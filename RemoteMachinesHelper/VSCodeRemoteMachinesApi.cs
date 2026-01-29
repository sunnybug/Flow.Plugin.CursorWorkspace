// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Flow.Plugin.CursorWorkspaces.SshConfigParser;
using Flow.Plugin.CursorWorkspaces.VSCodeHelper;

namespace Flow.Plugin.CursorWorkspaces.RemoteMachinesHelper
{
    public class VSCodeRemoteMachinesApi
    {
        /// <summary>
        /// 将配置中的路径展开：~ 或 ~/ 转为用户目录，%VAR% 展开为环境变量。
        /// </summary>
        private static string ExpandSshConfigPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;
            path = path.Trim();
            if (path.StartsWith("~/", StringComparison.Ordinal) || path == "~")
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path.Length > 1 ? path.Substring(2) : "");
            else if (path.StartsWith("~\\", StringComparison.Ordinal))
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path.Substring(2));
            return Environment.ExpandEnvironmentVariables(path);
        }

        /// <summary>
        /// 默认 SSH config 路径（与 OpenSSH / Cursor 默认一致）。
        /// </summary>
        private static string GetDefaultSshConfigPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "config");
        }

        public VSCodeRemoteMachinesApi()
        {
        }

        public List<VSCodeRemoteMachine> Machines
        {
            get
            {
                var results = new List<VSCodeRemoteMachine>();

                foreach (var vscodeInstance in VSCodeInstances.Instances)
                {
                    var vscode_settings = Path.Combine(vscodeInstance.AppData, "User\\settings.json");

                    if (!File.Exists(vscode_settings))
                    {
                        PluginLogger.Log($"[SSH] 跳过（无 settings）: {vscode_settings}");
                        continue;
                    }

                    var fileContent = File.ReadAllText(vscode_settings);
                    string configPath = null;

                    try
                    {
                        var vscodeSettingsFile = JsonSerializer.Deserialize<JsonElement>(fileContent, new JsonSerializerOptions
                        {
                            AllowTrailingCommas = true,
                            ReadCommentHandling = JsonCommentHandling.Skip,
                        });
                        if (vscodeSettingsFile.TryGetProperty("remote.SSH.configFile", out var pathElement))
                        {
                            var raw = pathElement.GetString();
                            if (!string.IsNullOrWhiteSpace(raw))
                                configPath = ExpandSshConfigPath(raw);
                        }
                    }
                    catch (Exception ex)
                    {
                        Main._context?.API.LogException("CursorWorkSpaces", $"Failed to deserialize {vscode_settings}", ex);
                        continue;
                    }

                    if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                    {
                        configPath = GetDefaultSshConfigPath();
                        PluginLogger.Log($"[SSH] 使用默认 config 路径: {configPath}, 存在: {File.Exists(configPath)}");
                    }
                    else
                    {
                        PluginLogger.Log($"[SSH] 使用 settings 中的 config: {configPath}");
                    }

                    if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                        continue;

                    try
                    {
                        var countBefore = results.Count;
                        foreach (var h in SshConfig.ParseFile(configPath))
                        {
                            if (string.IsNullOrEmpty(h.Host))
                                continue;
                            var machine = new VSCodeRemoteMachine();
                            machine.Host = h.Host;
                            machine.VSCodeInstance = vscodeInstance;
                            machine.HostName = h.HostName ?? string.Empty;
                            machine.User = h.User ?? string.Empty;
                            results.Add(machine);
                        }
                        PluginLogger.Log($"[SSH] 从 {configPath} 解析到 {results.Count - countBefore} 台主机");
                    }
                    catch (Exception ex)
                    {
                        Main._context?.API.LogException("CursorWorkSpaces", $"Failed to parse SSH config: {configPath}", ex);
                        PluginLogger.Log($"[SSH] 解析失败: {configPath}, {ex.Message}");
                    }
                }

                return results;
            }
        }
    }
}
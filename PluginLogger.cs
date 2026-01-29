// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Flow.Plugin.CursorWorkspaces
{
    /// <summary>
    /// 仅 Debug 构建时写入 Flow 日志，Release 不输出。全项目仅此处使用 #if DEBUG。
    /// </summary>
    internal static class PluginLogger
    {
        public static void Log(string message)
        {
#if DEBUG
            Main._context?.API.LogInfo("CursorWorkspaces", message);
#endif
        }
    }
}

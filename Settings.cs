using System.Collections.ObjectModel;

namespace Flow.Plugin.CursorWorkspaces
{
    public class Settings
    {
        public bool DiscoverWorkspaces { get; set; } = true;

        public bool DiscoverMachines { get; set; } = true;

        /// <summary>
        /// 用户配置的 Cursor 可执行文件路径。为空时使用 "Cursor.exe"（从 PATH 查找）。
        /// </summary>
        public string CursorExecutablePath { get; set; } = string.Empty;

        public ObservableCollection<string> CustomWorkspaces { get; set; } = new();
    }
}

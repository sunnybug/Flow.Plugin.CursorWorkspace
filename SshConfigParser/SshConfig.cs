// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Flow.Plugin.CursorWorkspaces.SshConfigParser
{
    public class SshConfig
    {
        private static readonly Regex _sshConfig = new Regex(@"^(\w[\s\S]*?\w)$(?=(?:\s+^\w|\z))", RegexOptions.Multiline);
        private static readonly Regex _keyValue = new Regex(@"(\w+\s\S+)", RegexOptions.Multiline);

        public static IEnumerable<SshHost> ParseFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return [];
            return Parse(File.ReadAllText(path));
        }

        public static IEnumerable<SshHost> Parse(string str)
        {
            if (string.IsNullOrEmpty(str))
                return [];
            str = str.Replace('\r', '\0');
            var list = new List<SshHost>();
            foreach (Match match in _sshConfig.Matches(str))
            {
                var sshHost = new SshHost();
                string content = match.Groups.Values.ToList()[0].Value;
                foreach (Match match1 in _keyValue.Matches(content))
                {
                    var part = match1.Value;
                    var spaceIndex = part.IndexOf(' ');
                    if (spaceIndex <= 0)
                        continue;
                    var key = part.Substring(0, spaceIndex);
                    var value = part.Substring(spaceIndex + 1).Trim();
                    if (!string.IsNullOrEmpty(key))
                        sshHost.Properties[key] = value;
                }

                list.Add(sshHost);
            }

            return list;
        }
    }
}

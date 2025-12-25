using System.Collections.Generic;
using System.Diagnostics;

namespace FocusDimmer.Helpers
{
    public static class ProcessInfoHelper
    {
        private static Dictionary<uint, string> _pidCache = new Dictionary<uint, string>();
        public static string GetProcessName(uint pid)
        {
            if (pid == 0) return "";
            if (_pidCache.TryGetValue(pid, out string cachedName)) return cachedName;
            
            // Limit cache size
            if (_pidCache.Count > 1000) _pidCache.Clear();

            try { using (var proc = Process.GetProcessById((int)pid)) { string name = proc.ProcessName.ToLower(); _pidCache[pid] = name; return name; } } catch { return ""; }
        }

        public static void ClearCache()
        {
            _pidCache.Clear();
        }
    }
}

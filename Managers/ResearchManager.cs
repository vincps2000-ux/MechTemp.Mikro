using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mechapp
{
    public static class ResearchManager
    {
        private static readonly string _file = "Research.txt"; // JSON array of strings (part names)
        private static readonly string _defaultsFile = Path.Combine("Config", "DefaultResearch.txt");
        private static HashSet<string>? _cache;

        private static HashSet<string> Load()
        {
            if (_cache != null) return _cache;
            try
            {
                if (!File.Exists(_file))
                {
                    _cache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    return _cache;
                }
                var json = File.ReadAllText(_file);
                var arr = JsonNode.Parse(json) as JsonArray;
                var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (arr != null)
                {
                    foreach (var item in arr)
                    {
                        var name = item?.ToString();
                        if (!string.IsNullOrWhiteSpace(name)) set.Add(name!);
                    }
                }
                _cache = set;
                return _cache;
            }
            catch
            {
                _cache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                return _cache;
            }
        }

        private static void Save()
        {
            var arr = new JsonArray();
            foreach (var name in Load())
            {
                arr.Add(name);
            }
            File.WriteAllText(_file, arr.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }

        public static bool IsResearched(string partName)
        {
            return Load().Contains(partName);
        }

        public static void Research(string partName)
        {
            if (string.IsNullOrWhiteSpace(partName)) return;
            var set = Load();
            if (set.Add(partName))
            {
                Save();
            }
        }

        public static void Unresearch(string partName)
        {
            if (string.IsNullOrWhiteSpace(partName)) return;
            var set = Load();
            if (set.Remove(partName))
            {
                Save();
            }
        }

        public static List<string> GetResearched()
        {
            return Load().OrderBy(s => s).ToList();
        }

        public static List<string> GetUnresearchedParts()
        {
            var all = PartManager.GetAvailableParts();
            var researched = Load();
            return all.Where(p => !researched.Contains(p)).OrderBy(p => p).ToList();
        }

        public static List<string> GetUnresearchedByCategory(string category)
        {
            var list = PartManager.GetPartsByCategory(category);
            var researched = Load();
            return list.Where(p => !researched.Contains(p)).OrderBy(p => p).ToList();
        }

        private static List<string> ReadDefaults()
        {
            // Fallback per requirements: Exosuit-Frame, Gun, all extremities except Core
            var fallback = new List<string>
            {
                "Exosuit-Frame",
                "Gun",
                "Siege-Mount",
                "Joint",
                "Connector",
                "Hand",
                "Foot",
                "Turret"
            };

            try
            {
                if (!File.Exists(_defaultsFile))
                {
                    return fallback;
                }
                var json = File.ReadAllText(_defaultsFile);
                var arr = JsonNode.Parse(json) as JsonArray;
                if (arr == null) return fallback;
                var list = new List<string>();
                foreach (var item in arr)
                {
                    var name = item?.ToString();
                    if (!string.IsNullOrWhiteSpace(name)) list.Add(name!);
                }
                return list.Count > 0 ? list : fallback;
            }
            catch
            {
                return fallback;
            }
        }

        // Call once at app startup to reset researched parts to defaults (non-persistent across restarts)
        public static void ApplyDefaultsOnStart()
        {
            var defaults = ReadDefaults();
            // Overwrite any existing research with defaults on each startup
            _cache = new HashSet<string>(defaults, StringComparer.OrdinalIgnoreCase);
            Save();
        }
    }
}

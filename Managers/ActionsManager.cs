using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace Mechapp
{
    /// <summary>
    /// Loads and provides access to action definitions from Config/Actions.txt
    /// </summary>
    public static class ActionsManager
    {
        private static readonly string ActionsFilePath = Path.Combine("Config", "Actions.txt");
        private static Dictionary<string, string>? _cache; // name -> description

        private static void EnsureLoaded()
        {
            if (_cache != null) return;
            _cache = new Dictionary<string, string>();
            if (!File.Exists(ActionsFilePath)) return;

            var json = File.ReadAllText(ActionsFilePath);
            var arr = JsonNode.Parse(json)?.AsArray();
            if (arr == null) return;
            foreach (var node in arr)
            {
                var name = node?["name"]?.ToString();
                var desc = node?["description"]?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _cache[name!] = desc;
                }
            }
        }

        public static IReadOnlyDictionary<string, string> GetAll()
        {
            EnsureLoaded();
            return _cache!;
        }

        public static string? GetDescription(string actionName)
        {
            EnsureLoaded();
            if (_cache != null && _cache.TryGetValue(actionName, out var desc))
                return desc;
            return null;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Mechapp
{
    /// <summary>
    /// Loads and provides access to tag definitions from Config/Tags.txt
    /// Supports tags with values, e.g., "Energy(40)" or "distance(300)".
    /// Descriptions are resolved by base tag name (value stripped).
    /// </summary>
    public static class TagsManager
    {
        private static readonly string TagsFilePath = Path.Combine("Config", "Tags.txt");
        private static Dictionary<string, string>? _cache; // baseName -> description (case-insensitive by key lower)

        private static void EnsureLoaded()
        {
            if (_cache != null) return;
            _cache = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(TagsFilePath)) return;

            var json = File.ReadAllText(TagsFilePath);
            var arr = JsonNode.Parse(json)?.AsArray();
            if (arr == null) return;
            foreach (var node in arr)
            {
                var name = node?["name"]?.ToString();
                var desc = node?["description"]?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _cache[name!] = desc;
                }
            }
        }

        /// <summary>
        /// Returns the human-readable description for a tag. If the tag includes a value in parentheses,
        /// the description is looked up by the base tag name (e.g., Energy for Energy(40)).
        /// </summary>
        public static string? GetDescription(string tag)
        {
            EnsureLoaded();
            var baseName = GetBaseName(tag);
            if (_cache != null && _cache.TryGetValue(baseName, out var desc))
                return desc;
            return null;
        }

        /// <summary>
        /// Returns the base tag name, stripping any value in parentheses. Example: "Energy(40)" -> "Energy".
        /// </summary>
        public static string GetBaseName(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return tag;
            var m = Regex.Match(tag, @"^\s*([^\(]+)\s*(?:\((.*?)\))?\s*$");
            if (m.Success)
            {
                return m.Groups[1].Value.Trim();
            }
            return tag.Trim();
        }

        /// <summary>
        /// Tries to extract a numeric value from a tag formatted like Name(123).
        /// Returns null if no value is present or parsing fails.
        /// </summary>
        public static double? GetNumericValue(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return null;
            var m = Regex.Match(tag, @"\(([-+]?[0-9]*\.?[0-9]+)\)");
            if (m.Success && double.TryParse(m.Groups[1].Value, out var v))
                return v;
            return null;
        }
    }
}

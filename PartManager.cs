using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;

namespace Mechapp
{
    public static class PartManager
    {
        private static List<string>? _parts;
        private static string _partsFile = "Parts.txt";

        // Returns all part names of a given type (case-insensitive)
        public static List<string> GetPartsByType(string type)
        {
            if (!File.Exists(_partsFile))
                return new List<string>();
            string json = File.ReadAllText(_partsFile);
            var arr = JsonNode.Parse(json)?.AsArray();
            var result = new List<string>();
            if (arr != null)
            {
                foreach (var item in arr)
                {
                    var itemType = item?["type"]?.ToString();
                    if (itemType != null && string.Equals(itemType, type, System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (item?["name"] != null)
                            result.Add(item["name"]!.ToString());
                    }
                }
            }
            return result;
        }

        // Returns all part names of a given category (case-insensitive)
        public static List<string> GetPartsByCategory(string category)
        {
            if (!File.Exists(_partsFile))
                return new List<string>();
            string json = File.ReadAllText(_partsFile);
            var arr = JsonNode.Parse(json)?.AsArray();
            var result = new List<string>();
            if (arr != null)
            {
                foreach (var item in arr)
                {
                    var itemCategory = item?["type"]?.ToString();
                    if (itemCategory != null && string.Equals(itemCategory, category, System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (item?["name"] != null)
                            result.Add(item["name"]!.ToString());
                    }
                }
            }
            return result;
        }

        // Returns the distinct list of categories/types present in Parts.txt (preserves original spelling)
        public static List<string> GetAllCategories()
        {
            if (!File.Exists(_partsFile))
                return new List<string>();
            string json = File.ReadAllText(_partsFile);
            var arr = JsonNode.Parse(json)?.AsArray();
            var set = new HashSet<string>();
            if (arr != null)
            {
                foreach (var item in arr)
                {
                    var itemCategory = item?["type"]?.ToString();
                    if (!string.IsNullOrEmpty(itemCategory))
                        set.Add(itemCategory);
                }
            }
            return set.ToList();
        }

        // Returns the type/category for a given part name (uses Parts.txt). Returns null if not found.
        public static string? GetTypeForName(string partName)
        {
            if (string.IsNullOrEmpty(partName) || !File.Exists(_partsFile))
                return null;
            string json = File.ReadAllText(_partsFile);
            var arr = JsonNode.Parse(json)?.AsArray();
            if (arr != null)
            {
                foreach (var item in arr)
                {
                    var name = item?["name"]?.ToString();
                    var type = item?["type"]?.ToString();
                    if (name != null && string.Equals(name, partName, System.StringComparison.OrdinalIgnoreCase))
                        return type;
                }
            }
            return null;
        }

        public static List<string> GetAvailableParts()
        {
            if (_parts != null)
                return _parts;

            if (!File.Exists(_partsFile))
                return new List<string>();

            string json = File.ReadAllText(_partsFile);
            var arr = JsonNode.Parse(json)?.AsArray();
            var result = new List<string>();
            if (arr != null)
            {
                foreach (var item in arr)
                {
                    if (item?["name"] != null)
                        result.Add(item["name"]!.ToString());
                }
            }
            _parts = result;
            return _parts;
        }
    }
}

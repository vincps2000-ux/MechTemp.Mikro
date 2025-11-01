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
        private static readonly string[] ValidScales = new[] { "Personal(1)", "Vehicle(2)", "House(3)", "Building(4)" };

    public static string? ChooseScale(string? parentScale = null, int? minScaleLevel = null, int? maxScaleLevel = null)
        {
            // If parent scale exists, only show scales of same or smaller size
            var availableScales = ValidScales
        .Where(s => parentScale == null || GetScaleLevel(s) <= GetScaleLevel(parentScale))
        .Where(s => !minScaleLevel.HasValue || GetScaleLevel(s) >= minScaleLevel.Value)
        .Where(s => !maxScaleLevel.HasValue || GetScaleLevel(s) <= maxScaleLevel.Value)
                .ToArray();

            Console.WriteLine("\nAvailable Scales:");
            for (int i = 0; i < availableScales.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {availableScales[i]}");
            }

            Console.Write("Choose scale (enter number): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= availableScales.Length)
            {
                return availableScales[choice - 1];
            }
            return null;
        }

        public static int GetScaleLevel(string scale)
        {
            // Extract the number from the scale string (e.g., "Personal(1)" -> 1)
            var match = System.Text.RegularExpressions.Regex.Match(scale, @"\((\d+)\)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int level))
            {
                return level;
            }
            return 0;
        }

        /// <summary>
        /// Returns MinScale level (1..N) for a part name, if specified in Parts.txt.
        /// Accepts either numeric ("3") or descriptor ("House(3)") formats.
        /// </summary>
        public static int? GetMinScaleLevelForPart(string partName)
        {
            var def = GetPartDefinition(partName);
            if (def == null) return null;

            var minScaleNode = def["MinScale"];
            if (minScaleNode == null) return null;

            var raw = minScaleNode.ToString();
            if (string.IsNullOrWhiteSpace(raw)) return null;

            // Try direct int first
            if (int.TryParse(raw, out int lvl)) return lvl;

            // Try descriptor parsing e.g., "House(3)" or any string that includes (number)
            var match = System.Text.RegularExpressions.Regex.Match(raw, @"\((\d+)\)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int lvl2)) return lvl2;

            // As a last resort, if the raw looks like a known scale label, use GetScaleLevel
            int lvl3 = GetScaleLevel(raw);
            return lvl3 > 0 ? lvl3 : null;
        }

        /// <summary>
        /// Returns MaxScale level (1..N) for a part name, if specified in Parts.txt.
        /// If not specified, defaults to 4 as the global maximum.
        /// Accepts either numeric ("4") or descriptor ("Building(4)") formats.
        /// </summary>
        public static int GetMaxScaleLevelForPart(string partName)
        {
            var def = GetPartDefinition(partName);
            if (def == null)
            {
                return 4; // default global max
            }

            var maxScaleNode = def["MaxScale"];
            if (maxScaleNode == null)
            {
                return 4; // default if missing
            }

            var raw = maxScaleNode.ToString();
            if (string.IsNullOrWhiteSpace(raw)) return 4;

            if (int.TryParse(raw, out int lvl)) return lvl;

            var match = System.Text.RegularExpressions.Regex.Match(raw, @"\((\d+)\)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int lvl2)) return lvl2;

            int lvl3 = GetScaleLevel(raw);
            return lvl3 > 0 ? lvl3 : 4;
        }

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

        // Get the part definition from Parts.txt by name
        public static JsonNode? GetPartDefinition(string partName)
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
                    if (name != null && string.Equals(name, partName, System.StringComparison.OrdinalIgnoreCase))
                        return item;
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

        // Returns the list of properties that should be displayed for a given part type
        public static List<string> GetPartProperties(string partType)
        {
            var properties = new List<string>();
            
            // Scale is a common property for all parts
            properties.Add("Scale");
            // Include MinScale when present in definitions
            properties.Add("MinScale");
            // Include MaxScale for visibility (will default logically to 4 if omitted)
            properties.Add("MaxScale");
            
            // Add additional specific properties based on part type
            switch (partType.ToLower())
            {
                case "frame":
                    // Frame might have additional specific properties in the future
                    break;
                // Add cases for other part types here
            }

            return properties;
        }
    }
}

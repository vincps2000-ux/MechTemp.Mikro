using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace Mechapp
{
    // Simple stat calculator for template-derived stats
    public static class StatCalc
    {
        public class MechAction
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
        }

        public class MechStats
        {
            public string Scale { get; set; } = ""; // overall mech scale (from frame)
            public double Weight { get; set; } // total current weight
            public int WeightLimit { get; set; } // capacity from frame
            public List<MechAction> Actions { get; set; } = new List<MechAction>();
        }

        // For now, only calculate a weight limit based on the root frame type.
        // Rules (example):
        // - Exosuit-Frame -> 2000
        // - DemiMech-Frame -> 1500
        // - Light-Frame -> 800
        // - Medium-Frame -> 1200
        // - Heavy-Frame -> 1800
        // - Colossal-Frame -> 3000
        // Default limit if no frame found: 0

        public static int GetWeightLimit(JsonObject template)
        {
            if (template == null)
                return 0;

            // Look for top-level frame (Part_* entries)
            foreach (var prop in template)
            {
                if (prop.Value is JsonObject obj)
                {
                    string? name = obj["name"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        // Prefer authoritative type/category from Parts.txt over name substring matching
                        var type = PartManager.GetTypeForName(name);
                        if (type != null && type.Equals("Frame", System.StringComparison.OrdinalIgnoreCase))
                        {
                            // Try lookup from Parts.txt definition first (source of truth)
                            var def = PartManager.GetPartDefinition(name);
                            if (def != null)
                            {
                                var wlStr = def["WeightLimit"]?.ToString();
                                if (int.TryParse(wlStr, out int limitFromParts))
                                {
                                    return limitFromParts;
                                }
                            }
                        }
                    }
                }
            }

            // No frame found
            return 0;
        }

        /// <summary>
        /// Calculates the total weight of all parts in the template.
        /// Recursively sums weights of all parts, using their actual scale and weight formulas.
        /// </summary>
        /// <param name="template">The template to calculate weight for</param>
        /// <returns>Total weight as a double</returns>
        public static double CalculateTotalWeight(JsonObject template)
        {
            if (template == null)
                return 0;

            double totalWeight = 0;

            // Process all top-level parts
            foreach (var prop in template)
            {
                if (prop.Value is JsonObject obj)
                {
                    totalWeight += CalculatePartWeight(obj);
                }
            }

            return totalWeight;
        }

        /// <summary>
        /// Recursively calculates the weight of a part and all its children.
        /// Frames are excluded from weight calculation as they define the weight capacity.
        /// </summary>
        /// <param name="part">The part object to calculate weight for</param>
        /// <returns>Weight of this part plus all children</returns>
        private static double CalculatePartWeight(JsonObject part)
        {
            double weight = 0;

            // Get part name and scale
            string? partName = part["name"]?.ToString();
            string? scale = part["Scale"]?.ToString();

            if (!string.IsNullOrEmpty(partName) && !string.IsNullOrEmpty(scale))
            {
                // Skip Frames - they define weight capacity, not consume it
                string? partType = PartManager.GetTypeForName(partName);
                if (partType == null || !partType.Equals("Frame", System.StringComparison.OrdinalIgnoreCase))
                {
                    // Calculate weight for this part using PartManager
                    weight = PartManager.GetWeightForPart(partName, scale);
                }
            }

            // Process children recursively
            if (part["children"] is JsonArray children)
            {
                foreach (var child in children)
                {
                    if (child is JsonObject childObj)
                    {
                        weight += CalculatePartWeight(childObj);
                    }
                }
            }

            return weight;
        }

        /// <summary>
        /// Compute overall mech stats: scale (from frame), weight/limit, and list of actions from parts.
        /// </summary>
        public static MechStats ComputeMechStats(JsonObject template)
        {
            var stats = new MechStats();
            if (template == null) return stats;

            // Weight/limit
            stats.WeightLimit = GetWeightLimit(template);
            stats.Weight = CalculateTotalWeight(template);

            // Scale from frame
            var frame = FindFirstFramePart(template);
            if (frame != null)
            {
                var sc = frame["Scale"]?.ToString();
                if (!string.IsNullOrEmpty(sc)) stats.Scale = sc!;
            }

            // Actions: collect from each part definition; repeat per part instance
            var allParts = EnumerateAllParts(template);
            foreach (var part in allParts)
            {
                var name = part["name"]?.ToString();
                if (string.IsNullOrEmpty(name)) continue;

                var actions = PartManager.GetActionsForPartName(name!);
                foreach (var a in actions)
                {
                    stats.Actions.Add(new MechAction
                    {
                        Name = a,
                        Description = ActionsManager.GetDescription(a)
                    });
                }
            }

            return stats;
        }

        private static IEnumerable<JsonObject> EnumerateAllParts(JsonObject root)
        {
            // parts may be stored as Part_* properties or under children arrays; traverse all JsonObjects with PartID or with name
            foreach (var prop in root)
            {
                if (prop.Value is JsonObject obj)
                {
                    foreach (var inner in EnumerateAllParts(obj))
                        yield return inner;
                }
                else if (prop.Value is JsonArray arr)
                {
                    foreach (var item in arr)
                    {
                        if (item is JsonObject ch)
                        {
                            foreach (var inner in EnumerateAllParts(ch))
                                yield return inner;
                        }
                    }
                }
            }

            // If this node itself looks like a part (has PartID or name), yield it
            if (root["name"] != null && root["PartID"] != null)
            {
                yield return root;
            }
        }

        private static JsonObject? FindFirstFramePart(JsonObject template)
        {
            // Scan top-level first for performance
            foreach (var prop in template)
            {
                if (prop.Value is JsonObject obj)
                {
                    string? name = obj["name"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        var type = PartManager.GetTypeForName(name);
                        if (type != null && type.Equals("Frame", System.StringComparison.OrdinalIgnoreCase))
                        {
                            return obj;
                        }
                    }
                }
            }
            // fallback recursive search
            foreach (var prop in template)
            {
                if (prop.Value is JsonObject obj)
                {
                    var found = FindFirstFramePart(obj);
                    if (found != null) return found;
                }
                else if (prop.Value is JsonArray arr)
                {
                    foreach (var item in arr)
                    {
                        if (item is JsonObject ch)
                        {
                            var found = FindFirstFramePart(ch);
                            if (found != null) return found;
                        }
                    }
                }
            }
            return null;
        }

    }
}

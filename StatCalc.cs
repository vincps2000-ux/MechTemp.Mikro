using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace Mechapp
{
    // Simple stat calculator for template-derived stats
    public static class StatCalc
    {
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

    }
}

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

    }
}

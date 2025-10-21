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
                    if (name != null && name.ToLower().Contains("frame"))
                    {
                        return WeightForFrame(name);
                    }
                }
            }

            // No frame found
            return 0;
        }

        static int WeightForFrame(string frameName)
        {
            var n = frameName.ToLower();
            if (n.Contains("exosuit")) return 2000;
            if (n.Contains("demimech")) return 1500;
            if (n.Contains("light")) return 800;
            if (n.Contains("medium")) return 1200;
            if (n.Contains("heavy")) return 1800;
            if (n.Contains("colossal")) return 3000;
            return 1000; // fallback
        }
    }
}

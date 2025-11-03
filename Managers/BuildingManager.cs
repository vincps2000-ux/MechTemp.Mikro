using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mechapp
{
    public static class BuildingManager
    {
        private static readonly string _file = "Buildings.txt";
        private static readonly List<string> _fallback = new()
        {
            "Design Bureau",
            "Mech Factory",
            "Research Lab",
            "Sales Office",
            "Operations Office"
        };

        public static List<string> GetBuildings()
        {
            try
            {
                if (!File.Exists(_file))
                {
                    return new List<string>(_fallback);
                }
                var json = File.ReadAllText(_file);
                var arr = JsonNode.Parse(json) as JsonArray;
                if (arr == null) return new List<string>(_fallback);
                var result = new List<string>();
                foreach (var item in arr)
                {
                    var name = item?.ToString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        result.Add(name!);
                    }
                }
                return result.Count > 0 ? result : new List<string>(_fallback);
            }
            catch
            {
                return new List<string>(_fallback);
            }
        }
    }
}

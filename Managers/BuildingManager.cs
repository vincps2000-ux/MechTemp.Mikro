using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mechapp
{
    public class BuildingInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public static class BuildingManager
    {
        private static readonly string _file = "Buildings.txt";
        private static readonly string _descriptionsFile = "Config/BuildingDescriptions.txt";
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

        public static List<BuildingInfo> GetBuildingsWithDescriptions()
        {
            try
            {
                if (!File.Exists(_descriptionsFile))
                {
                    // Fallback to basic list without descriptions
                    var buildings = GetBuildings();
                    return buildings.ConvertAll(b => new BuildingInfo { Name = b, Description = "" });
                }

                var json = File.ReadAllText(_descriptionsFile);
                var buildingList = JsonSerializer.Deserialize<List<BuildingInfo>>(json);
                
                if (buildingList != null && buildingList.Count > 0)
                {
                    return buildingList;
                }

                // Fallback if deserialization fails
                var fallbackBuildings = GetBuildings();
                return fallbackBuildings.ConvertAll(b => new BuildingInfo { Name = b, Description = "" });
            }
            catch
            {
                // Fallback on error
                var fallbackBuildings = GetBuildings();
                return fallbackBuildings.ConvertAll(b => new BuildingInfo { Name = b, Description = "" });
            }
        }
    }
}

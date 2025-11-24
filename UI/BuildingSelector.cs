using System;
using System.Collections.Generic;

namespace Mechapp.UI
{
    public static class BuildingSelector
    {
        public static string? Show()
        {
            while (true)
            {
                Console.Clear();
                ResourceManager.DisplayResourceBar();
                Console.WriteLine("=== District Directory ===");
                Console.WriteLine("Select a building (or 'q' to quit):\n");

                var buildings = BuildingManager.GetBuildingsWithDescriptions();
                for (int i = 0; i < buildings.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {buildings[i].Name}");
                    if (!string.IsNullOrWhiteSpace(buildings[i].Description))
                    {
                        Console.WriteLine($"   {buildings[i].Description}");
                    }
                    Console.WriteLine(); // Add blank line for readability
                }

                Console.Write("Enter choice: ");
                var input = Console.ReadLine() ?? string.Empty;
                if (input.Trim().Equals("q", StringComparison.OrdinalIgnoreCase))
                {
                    return null; // user chose to quit
                }
                if (int.TryParse(input, out int idx) && idx >= 1 && idx <= buildings.Count)
                {
                    return buildings[idx - 1].Name;
                }

                Console.WriteLine("Invalid selection. Press any key to try again...");
                Console.ReadKey(true);
            }
        }
    }
}

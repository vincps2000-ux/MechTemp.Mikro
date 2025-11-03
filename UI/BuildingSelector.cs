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
                Console.WriteLine("=== District Directory ===");
                Console.WriteLine("Select a building (or 'q' to quit):\n");

                var buildings = BuildingManager.GetBuildings();
                for (int i = 0; i < buildings.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {buildings[i]}");
                }

                Console.Write("\nEnter choice: ");
                var input = Console.ReadLine() ?? string.Empty;
                if (input.Trim().Equals("q", StringComparison.OrdinalIgnoreCase))
                {
                    return null; // user chose to quit
                }
                if (int.TryParse(input, out int idx) && idx >= 1 && idx <= buildings.Count)
                {
                    return buildings[idx - 1];
                }

                Console.WriteLine("Invalid selection. Press any key to try again...");
                Console.ReadKey(true);
            }
        }
    }
}

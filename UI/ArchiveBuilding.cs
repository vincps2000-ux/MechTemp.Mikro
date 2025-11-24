using System;
using System.Collections.Generic;
using Mechapp;

namespace Mechapp.UI
{
    public static class ArchiveBuilding
    {
        public static void Show()
        {
            Console.Clear();
            ResourceManager.DisplayResourceBar();
            Console.WriteLine("=== Archive Building ===\n");
            var researched = ResearchManager.GetResearched();
            if (researched.Count == 0)
            {
                Console.WriteLine("No researched parts available.");
                Console.WriteLine("Press any key to return...");
                Console.ReadKey(true);
                return;
            }
            for (int i = 0; i < researched.Count; i++)
            {
                var def = PartManager.GetPartDefinition(researched[i]);
                string desc = def?["Description"]?.ToString() ?? "No description.";
                Console.WriteLine($"{i + 1}. {researched[i]}");
                Console.WriteLine($"   {desc}\n");
            }
            Console.WriteLine("Press any key to return...");
            Console.ReadKey(true);
        }
    }
}

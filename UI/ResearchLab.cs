using System;
using System.Collections.Generic;
using System.Linq;

namespace Mechapp.UI
{
    public static class ResearchLab
    {
        public static void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Research Lab ===\n");
                Console.WriteLine("1. Research by Category");
                Console.WriteLine("2. Research from full list");
                Console.WriteLine("3. View researched parts");
                Console.WriteLine("q. Back to District Directory\n");
                Console.Write("Enter choice: ");
                var input = Console.ReadLine() ?? string.Empty;

                if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
                    return;

                switch (input)
                {
                    case "1":
                        ResearchByCategory();
                        break;
                    case "2":
                        ResearchFromFullList();
                        break;
                    case "3":
                        ShowResearched();
                        break;
                    default:
                        Console.WriteLine("Invalid selection. Press any key...");
                        Console.ReadKey(true);
                        break;
                }
            }
        }

        private static void ResearchByCategory()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("== Research by Category ==\n");
                var categories = PartManager.GetAllCategories();
                if (categories.Count == 0)
                {
                    Console.WriteLine("No categories found.");
                    Console.WriteLine("Press any key to return...");
                    Console.ReadKey(true);
                    return;
                }

                for (int i = 0; i < categories.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {categories[i]}");
                }
                Console.WriteLine("0. Back\n");
                Console.Write("Select category: ");
                var input = Console.ReadLine() ?? string.Empty;
                if (input == "0") return;
                if (!int.TryParse(input, out int idx) || idx < 1 || idx > categories.Count)
                {
                    Console.WriteLine("Invalid selection. Press any key...");
                    Console.ReadKey(true);
                    continue;
                }
                var category = categories[idx - 1];
                var list = ResearchManager.GetUnresearchedByCategory(category);
                if (list.Count == 0)
                {
                    Console.WriteLine($"No unresearched parts in {category}.");
                    Console.WriteLine("Press any key...");
                    Console.ReadKey(true);
                    continue;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {list[i]}");
                }
                Console.WriteLine("0. Back\n");
                Console.Write("Select part to research: ");
                var pinput = Console.ReadLine() ?? string.Empty;
                if (pinput == "0") continue;
                if (int.TryParse(pinput, out int pidx) && pidx >= 1 && pidx <= list.Count)
                {
                    var name = list[pidx - 1];
                    ResearchManager.Research(name);
                    Console.WriteLine($"Researched: {name}");
                    Console.WriteLine("Press any key...");
                    Console.ReadKey(true);
                }
                else
                {
                    Console.WriteLine("Invalid selection. Press any key...");
                    Console.ReadKey(true);
                }
            }
        }

        private static void ResearchFromFullList()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("== Research — All Unresearched Parts ==\n");
                var list = ResearchManager.GetUnresearchedParts();
                if (list.Count == 0)
                {
                    Console.WriteLine("All parts are researched or no parts available.");
                    Console.WriteLine("Press any key to return...");
                    Console.ReadKey(true);
                    return;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {list[i]}");
                }
                Console.WriteLine("0. Back\n");
                Console.Write("Select part to research: ");
                var input = Console.ReadLine() ?? string.Empty;
                if (input == "0") return;
                if (int.TryParse(input, out int idx) && idx >= 1 && idx <= list.Count)
                {
                    var name = list[idx - 1];
                    ResearchManager.Research(name);
                    Console.WriteLine($"Researched: {name}");
                    Console.WriteLine("Press any key...");
                    Console.ReadKey(true);
                }
                else
                {
                    Console.WriteLine("Invalid selection. Press any key...");
                    Console.ReadKey(true);
                }
            }
        }

        private static void ShowResearched()
        {
            Console.Clear();
            Console.WriteLine("== Researched Parts ==\n");
            var list = ResearchManager.GetResearched();
            if (list.Count == 0)
            {
                Console.WriteLine("No parts researched yet.");
            }
            else
            {
                foreach (var n in list) Console.WriteLine("- " + n);
            }
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
        }
    }
}

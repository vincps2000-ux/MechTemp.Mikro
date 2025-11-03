// See https://aka.ms/new-console-template for more information
using System.Text.Json.Nodes;
using Templates;
using System.Collections.Generic;
using System.Linq;
using Mechapp.UI;
namespace Mechapp
{
    class Program
    {
        // Static reference to the template manager for NavigationStep
        static Templatemanager? _manager = null;
        // Configurable action keys
        public const char GoUpKey = 'U'; // Change this letter to change the Go Up key
        public const char AddPartKey = 'A'; // Change this letter to change the Add New Part key
        public const char SaveKey = 'S'; // Key for saving template
        public const char LoadKey = 'L'; // Key for loading template
        
        static void Main(string[] args)
        {
            // New entry flow: Building selection first, then Design Bureau opens the template builder
            RunFromDistrictDirectory();
        }

        static void RunFromDistrictDirectory()
        {
            while (true)
            {
                var selected = BuildingSelector.Show();
                if (selected == null)
                {
                    // Quit selected
                    break;
                }

                if (selected.Equals("Design Bureau", System.StringComparison.OrdinalIgnoreCase))
                {
                    // Enter the existing Template Builder flow
                    ConsoleMechTemplateBuilder();
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine($"{selected} — Feature to be added later.");
                    Console.WriteLine("Press any key to return to the District Directory...");
                    Console.ReadKey(true);
                }
            }
        }

        static bool ConsoleMechTemplateBuilder()
        {
            Templatemanager manager = new Templatemanager();
            _manager = manager;
            Console.WriteLine("Welcome in TemplateBuilder:");
            
            // Ask user if they want to load an existing template
            Console.WriteLine("\nDo you want to:");
            Console.WriteLine("1. Create a new template");
            Console.WriteLine("2. Load an existing template");
            Console.Write("Enter choice (1 or 2): ");
            string choice = Console.ReadLine() ?? "1";
            
            if (choice == "2")
            {
                var loadedTemplate = ShowLoadTemplateMenu();
                if (loadedTemplate != null)
                {
                    manager.LoadTemplate(loadedTemplate);
                }
                else
                {
                    Console.WriteLine("Starting with blank template instead.");
                    manager.CreateBlankTemplate();
                }
            }
            else
            {
                manager.CreateBlankTemplate();
            }
            
            NavigateJSON(manager.GetTemplate());
            return true;
        }

        static bool OutputCurrentTemplate(JsonObject Input)
        {
            Console.WriteLine("Current Template");
            Console.WriteLine(Input.ToString());
            return true;
        }
        
        static void NavigateJSON(JsonObject rootObject)
        {
            JsonObject currentObject = rootObject;
            Stack<JsonObject> navigationStack = new Stack<JsonObject>();
            while (true)
            {
                // Allow the JSON to change between steps
                var result = NavigationStep(currentObject, navigationStack, rootObject);
                if (result == null)
                {
                    break;
                }
                currentObject = result.Item1;
                navigationStack = result.Item2;
            }
        }

        // Performs a single navigation step. Returns null to quit, or a tuple of (currentObject, navigationStack)
        static Tuple<JsonObject, Stack<JsonObject>>? NavigationStep(JsonObject currentObject, Stack<JsonObject> navigationStack, JsonObject rootObject)
        {
            Console.Clear();
            // Recalculate stats and display weight limit
            int weightLimit = 0;
            string currentScale = "";
            if (_manager != null)
            {
                weightLimit = StatCalc.GetWeightLimit(_manager.GetTemplate());
                // Get scale of current object if it exists
                var scale = currentObject["Scale"]?.ToString();
                if (!string.IsNullOrEmpty(scale))
                {
                    currentScale = scale;
                }
            }
            Console.WriteLine($"=== JSON Navigator ===    Weight limit: {weightLimit}    Scale: {currentScale}");

            // Get all parts at current level
            List<JsonObject> parts = GetChildParts(currentObject);

            // Display options
            if (navigationStack.Count > 0)
            {
                Console.WriteLine($"{GoUpKey}. [Go Up]");
            }

            for (int i = 0; i < parts.Count; i++)
            {
                string name = parts[i]["name"]?.ToString() ?? "Unnamed";
                string partID = parts[i]["PartID"]?.ToString() ?? "?";
                string displayText = $"{i + 1}. {name} (PartID: {partID})";

                // Get and display the part's properties
                string? partType = PartManager.GetTypeForName(name);
                if (partType != null)
                {
                    var properties = PartManager.GetPartProperties(partType);
                    foreach (var prop in properties)
                    {
                        var propValue = parts[i][prop]?.ToString() ?? "N/A";
                        displayText += $" | {prop}: {propValue}";
                    }
                }
                
                Console.WriteLine(displayText);
            }

            // Only show Add Part option if allowed
            string? parentPartID = currentObject["PartID"]?.ToString();
            bool canAdd = _manager != null && _manager.CanAdd(parentPartID);
            if (canAdd)
            {
                Console.WriteLine($"{AddPartKey}. [Add New Part]");
            }

            // Show Save and Load options (only at root level)
            if (navigationStack.Count == 0)
            {
                Console.WriteLine($"{SaveKey}. [Save Template]");
                Console.WriteLine($"{LoadKey}. [Load Template]");
            }

            Console.WriteLine("\nEnter number to navigate" + (canAdd ? ", letter for action" : "") + ", or 'q' to quit: ");
            string input = Console.ReadLine()!;

            // Quit
            if (input?.ToLower() == "q")
            {
                return null;
            }

            // Handle Go Up
            if (navigationStack.Count > 0 && input != null && input.Length == 1 && char.ToUpper(input[0]) == GoUpKey)
            {
                currentObject = navigationStack.Pop();
            }
            // Handle Save Template (only at root)
            else if (navigationStack.Count == 0 && input != null && input.Length == 1 && char.ToUpper(input[0]) == SaveKey)
            {
                if (_manager != null)
                {
                    ShowSaveTemplateMenu();
                }
            }
            // Handle Load Template (only at root)
            else if (navigationStack.Count == 0 && input != null && input.Length == 1 && char.ToUpper(input[0]) == LoadKey)
            {
                if (_manager != null)
                {
                    var loadedTemplate = ShowLoadTemplateMenu();
                    if (loadedTemplate != null)
                    {
                        _manager.LoadTemplate(loadedTemplate);
                        // Reset to root after loading
                        currentObject = _manager.GetTemplate();
                        navigationStack.Clear();
                    }
                }
            }
            // Handle Add New Part only if allowed
            else if (canAdd && input != null && input.Length == 1 && char.ToUpper(input[0]) == AddPartKey)
            {
                if (_manager == null)
                {
                    Console.WriteLine("Error: Template manager not available.");
                    Console.ReadKey();
                }
                else
                {
                    List<string> availableParts;
                    if (parentPartID == null)
                    {
                        availableParts = PartManager.GetPartsByType("Frame");
                    }
                    else
                    {
                        // Populate categories from Parts.txt so names match exactly
                        Console.WriteLine("Select category to add:");
                        var categories = PartManager.GetAllCategories();
                        if (categories == null || categories.Count == 0)
                        {
                            Console.WriteLine("No categories available in Parts.txt!");
                            Console.ReadKey();
                            return Tuple.Create(currentObject, navigationStack);
                        }
                        // When adding under a parent, disallow adding Frame parts here (only allow frames at root)
                        var filtered = categories.Where(c => !c.Equals("Frame", System.StringComparison.OrdinalIgnoreCase)).ToList();
                        if (filtered.Count == 0)
                        {
                            Console.WriteLine("No child categories available (frames are root-only).");
                            Console.ReadKey();
                            return Tuple.Create(currentObject, navigationStack);
                        }
                        for (int i = 0; i < filtered.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {filtered[i]}");
                        }
                        Console.Write("Enter category number: ");
                        string catInput = Console.ReadLine() ?? "";
                        int catIndex = 0;
                        if (!int.TryParse(catInput, out catIndex) || catIndex < 1 || catIndex > filtered.Count)
                        {
                            Console.WriteLine("Invalid category selection!");
                            Console.ReadKey();
                            return Tuple.Create(currentObject, navigationStack);
                        }
                        string selectedCategory = filtered[catIndex - 1];
                        availableParts = PartManager.GetPartsByCategory(selectedCategory);
                    }
                    if (availableParts.Count == 0)
                    {
                        Console.WriteLine("No parts available in Parts.txt!");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Select a part to add:");
                        for (int i = 0; i < availableParts.Count; i++)
                        {
                            string partName = availableParts[i];
                            var partDef = PartManager.GetPartDefinition(partName);
                            string displayText = $"{i + 1}. {partName}";
                            
                            if (partDef != null)
                            {
                                string? partType = partDef["type"]?.ToString();
                                if (partType != null)
                                {
                                    var properties = PartManager.GetPartProperties(partType);
                                    foreach (var prop in properties)
                                    {
                                        var propValue = partDef[prop]?.ToString() ?? "N/A";
                                        displayText += $" | {prop}: {propValue}";
                                    }
                                }
                            }
                            
                            Console.WriteLine(displayText);
                        }
                        Console.Write("Enter number: ");
                        string partInput = Console.ReadLine() ?? "";
                        if (int.TryParse(partInput, out int partIndex) && partIndex > 0 && partIndex <= availableParts.Count)
                        {
                            string selectedName = availableParts[partIndex - 1];
                            var newPart = new JsonObject
                            {
                                ["name"] = selectedName
                            };
                            _manager.AddPart(newPart, parentPartID);
                        }
                        else
                        {
                            Console.WriteLine("Invalid part selection!");
                            Console.ReadKey();
                        }
                    }
                }
            }
            // Handle part navigation by number
            else if (input != null && int.TryParse(input, out int selection) && selection > 0 && selection <= parts.Count)
            {
                navigationStack.Push(currentObject);
                currentObject = parts[selection - 1];
            }
            else
            {
                Console.WriteLine("Invalid input!");
                Console.ReadKey();
            }
            return Tuple.Create(currentObject, navigationStack);
        }
        // ...existing code...
        // GetChildParts method moved below

        static List<JsonObject> GetChildParts(JsonObject obj)
        {
            List<JsonObject> parts = new List<JsonObject>();
            // Check direct properties (like Part_1, Part_2)
            foreach (var property in obj)
            {
                if (property.Value is JsonObject childObj && childObj["PartID"] != null)
                {
                    parts.Add(childObj);
                }
            }
            // Check children array
            if (obj["children"] is JsonArray childrenArray)
            {
                foreach (var item in childrenArray)
                {
                    if (item is JsonObject childObj)
                    {
                        parts.Add(childObj);
                    }
                }
            }
            return parts;
        }

        static void ShowSaveTemplateMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Save Template ===");
            Console.Write("Enter filename (without .json extension): ");
            string fileName = Console.ReadLine() ?? "";
            
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Console.WriteLine("Invalid filename!");
                Console.ReadKey();
                return;
            }

            if (_manager != null)
            {
                bool success = PersistencyManager.SaveTemplate(_manager.GetTemplate(), fileName);
                if (success)
                {
                    Console.WriteLine("Template saved successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to save template.");
                }
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static JsonObject? ShowLoadTemplateMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Load Template ===");
            
            var savedTemplates = PersistencyManager.GetSavedTemplates();
            
            if (savedTemplates.Length == 0)
            {
                Console.WriteLine("No saved templates found.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return null;
            }

            Console.WriteLine("Available templates:");
            for (int i = 0; i < savedTemplates.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {savedTemplates[i]}");
            }
            
            Console.Write("\nEnter template number to load (or 0 to cancel): ");
            string input = Console.ReadLine() ?? "";
            
            if (int.TryParse(input, out int choice) && choice > 0 && choice <= savedTemplates.Length)
            {
                var loaded = PersistencyManager.LoadTemplate(savedTemplates[choice - 1]);
                if (loaded != null)
                {
                    Console.WriteLine("Template loaded successfully!");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return loaded;
                }
                else
                {
                    Console.WriteLine("Failed to load template.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
            
            return null;
        }
    }
}



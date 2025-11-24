using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mechapp
{
    /// <summary>
    /// Manages game resources like Gold. Persists state to Resources.txt.
    /// </summary>
    public static class ResourceManager
    {
        private static readonly string _resourcesFile = "Resources.txt";
        private static readonly int _startingGold = 1000;
        private static int? _goldCache = null;

        /// <summary>
        /// Initializes resources on game start. Creates Resources.txt with starting values if it doesn't exist.
        /// </summary>
        public static void Initialize()
        {
            if (!File.Exists(_resourcesFile))
            {
                // First time setup - start with 1000 Gold
                SetGold(_startingGold);
                Console.WriteLine($"Resources initialized! Starting with {_startingGold} Gold.");
            }
            else
            {
                // Load existing resources
                _goldCache = null; // Reset cache to force reload
                Console.WriteLine($"Resources loaded. Current Gold: {GetGold()}");
            }
        }

        /// <summary>
        /// Gets the current amount of Gold.
        /// </summary>
        /// <returns>Current Gold amount</returns>
        public static int GetGold()
        {
            if (_goldCache.HasValue)
                return _goldCache.Value;

            try
            {
                if (!File.Exists(_resourcesFile))
                {
                    _goldCache = _startingGold;
                    return _goldCache.Value;
                }

                string json = File.ReadAllText(_resourcesFile);
                var obj = JsonNode.Parse(json)?.AsObject();
                
                if (obj != null && obj["Gold"] != null)
                {
                    if (int.TryParse(obj["Gold"]?.ToString(), out int gold))
                    {
                        _goldCache = gold;
                        return gold;
                    }
                }

                // Default if parsing fails
                _goldCache = _startingGold;
                return _goldCache.Value;
            }
            catch
            {
                _goldCache = _startingGold;
                return _goldCache.Value;
            }
        }

        /// <summary>
        /// Sets the Gold amount to a specific value.
        /// </summary>
        /// <param name="amount">The new Gold amount</param>
        public static void SetGold(int amount)
        {
            try
            {
                var obj = new JsonObject
                {
                    ["Gold"] = amount
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonString = obj.ToJsonString(options);
                File.WriteAllText(_resourcesFile, jsonString);
                
                _goldCache = amount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving resources: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds Gold to the current amount.
        /// </summary>
        /// <param name="amount">Amount to add (can be negative to subtract)</param>
        /// <returns>True if successful, false if result would be negative</returns>
        public static bool AddGold(int amount)
        {
            int current = GetGold();
            int newAmount = current + amount;

            if (newAmount < 0)
            {
                Console.WriteLine($"Insufficient Gold! Need {-amount}, but only have {current}.");
                return false;
            }

            SetGold(newAmount);
            return true;
        }

        /// <summary>
        /// Checks if the player has at least the specified amount of Gold.
        /// </summary>
        /// <param name="amount">Amount to check</param>
        /// <returns>True if player has enough Gold</returns>
        public static bool HasGold(int amount)
        {
            return GetGold() >= amount;
        }

        /// <summary>
        /// Resets all resources to starting values.
        /// </summary>
        public static void Reset()
        {
            SetGold(_startingGold);
            Console.WriteLine($"Resources reset to starting values. Gold: {_startingGold}");
        }

        /// <summary>
        /// Displays the resource bar at the top of the console window.
        /// Only shows resources that are not zero.
        /// </summary>
        public static void DisplayResourceBar()
        {
            var resources = new List<string>();
            
            int gold = GetGold();
            if (gold != 0)
            {
                resources.Add($"Gold: {gold}");
            }

            if (resources.Count > 0)
            {
                Console.WriteLine("=== Resources: " + string.Join(" | ", resources) + " ===");
            }
        }
    }
}

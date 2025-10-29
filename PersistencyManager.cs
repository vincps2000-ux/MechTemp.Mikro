using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mechapp
{
    public static class PersistencyManager
    {
        private static readonly string _templatesFolder = "Templates";

        /// <summary>
        /// Ensures the templates folder exists
        /// </summary>
        private static void EnsureTemplatesFolderExists()
        {
            if (!Directory.Exists(_templatesFolder))
            {
                Directory.CreateDirectory(_templatesFolder);
            }
        }

        /// <summary>
        /// Saves a template to a file in the Templates folder
        /// </summary>
        /// <param name="template">The JsonObject template to save</param>
        /// <param name="fileName">The name of the file (without path)</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool SaveTemplate(JsonObject template, string fileName)
        {
            try
            {
                EnsureTemplatesFolderExists();

                // Ensure .json extension
                if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".json";
                }

                string filePath = Path.Combine(_templatesFolder, fileName);

                // Serialize with pretty printing
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonString = template.ToJsonString(options);
                File.WriteAllText(filePath, jsonString);

                Console.WriteLine($"Template saved to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving template: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads a template from a file in the Templates folder
        /// </summary>
        /// <param name="fileName">The name of the file (without path)</param>
        /// <returns>The loaded JsonObject template, or null if failed</returns>
        public static JsonObject? LoadTemplate(string fileName)
        {
            try
            {
                // Ensure .json extension
                if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".json";
                }

                string filePath = Path.Combine(_templatesFolder, fileName);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Template file not found: {filePath}");
                    return null;
                }

                string jsonString = File.ReadAllText(filePath);
                var template = JsonNode.Parse(jsonString)?.AsObject();

                if (template != null)
                {
                    Console.WriteLine($"Template loaded from: {filePath}");
                }

                return template;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading template: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lists all saved templates in the Templates folder
        /// </summary>
        /// <returns>Array of template file names (without .json extension)</returns>
        public static string[] GetSavedTemplates()
        {
            try
            {
                EnsureTemplatesFolderExists();

                var files = Directory.GetFiles(_templatesFolder, "*.json");
                var fileNames = new string[files.Length];

                for (int i = 0; i < files.Length; i++)
                {
                    // Get filename without path and without .json extension
                    fileNames[i] = Path.GetFileNameWithoutExtension(files[i]);
                }

                return fileNames;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing templates: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Deletes a template file from the Templates folder
        /// </summary>
        /// <param name="fileName">The name of the file to delete (without path)</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool DeleteTemplate(string fileName)
        {
            try
            {
                // Ensure .json extension
                if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".json";
                }

                string filePath = Path.Combine(_templatesFolder, fileName);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Template file not found: {filePath}");
                    return false;
                }

                File.Delete(filePath);
                Console.WriteLine($"Template deleted: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting template: {ex.Message}");
                return false;
            }
        }
    }
}

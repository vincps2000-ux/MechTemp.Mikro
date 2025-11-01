using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Mechapp;
namespace Templates
{
    class Templatemanager
    {
        JsonObject managedTemplate = new JsonObject();
        Int32 IDCounter = 0;
        /// <summary>
        /// Creates the Blank base Template
        /// </summary>
        /// <returns></returns>
        public JsonObject CreateBlankTemplate()
        {
            managedTemplate = new JsonObject();
            return managedTemplate;
        }

        /// <summary>
        /// Returns the managed Template
        /// </summary>
        /// <returns></returns>
        public JsonObject GetTemplate()
        {
            return managedTemplate;
        }

        /// <summary>
        /// Loads a template and updates the IDCounter based on existing PartIDs
        /// </summary>
        /// <param name="template">The template to load</param>
        public void LoadTemplate(JsonObject template)
        {
            managedTemplate = template;
            // Update IDCounter to be higher than any existing PartID
            IDCounter = GetMaxPartID(managedTemplate);
        }

        /// <summary>
        /// Recursively finds the maximum PartID in the template
        /// </summary>
        /// <param name="obj">The JsonObject to search</param>
        /// <returns>The maximum PartID found</returns>
        private int GetMaxPartID(JsonObject obj)
        {
            int maxID = 0;

            // Check current object's PartID
            if (obj["PartID"] != null && int.TryParse(obj["PartID"]?.ToString(), out int currentID))
            {
                if (currentID > maxID)
                    maxID = currentID;
            }

            // Search through all properties
            foreach (var property in obj)
            {
                if (property.Value is JsonObject childObj)
                {
                    int childMax = GetMaxPartID(childObj);
                    if (childMax > maxID)
                        maxID = childMax;
                }
                else if (property.Value is JsonArray array)
                {
                    foreach (var item in array)
                    {
                        if (item is JsonObject arrayObj)
                        {
                            int arrayMax = GetMaxPartID(arrayObj);
                            if (arrayMax > maxID)
                                maxID = arrayMax;
                        }
                    }
                }
            }

            return maxID;
        }

        /// <summary>
        /// Returns true if a part can be added to the specified layer (by PartID). For root, only one frame allowed.
        /// </summary>
        /// <param name="parentPartID">null for root, or PartID for child layer</param>
        /// <returns></returns>
        public bool CanAdd(string? parentPartID = null)
        {
            if (parentPartID == null)
            {
                // Root layer: only allow one frame part
                int frameCount = 0;
                foreach (var property in managedTemplate)
                {
                    if (property.Value is JsonObject partObj && partObj["name"] != null)
                    {
                        string? name = partObj["name"]?.ToString();
                        if (name != null && name.ToLower().Contains("frame"))
                        {
                            frameCount++;
                        }
                    }
                }
                return frameCount < 1;
            }
            else
            {
                // For other layers, always allow adding (customize rules here if needed)
                return true;
            }
        }

        /// <summary>
        /// Adds a part in JSONObject Form onto the Template, at specified PartID Place
        /// </summary>
        /// <param name="partToAdd"></param>
        /// <param name="parentPartID"></param>
        /// <returns></returns>
    public Int32 AddPart(JsonObject partToAdd, string? parentPartID = null)
        {
            // Resolve parent scale (if any)
            string? parentScale = null;
            if (parentPartID != null)
            {
                var parent = FindPartByID(managedTemplate, parentPartID);
                if (parent != null)
                {
                    parentScale = parent["Scale"]?.ToString();
                }
            }

            // Load definition and pre-populate known properties (but handle Scale with rules below)
            string? partName = partToAdd["name"]?.ToString();
            var definition = partName != null ? PartManager.GetPartDefinition(partName) : null;

            // Determine Min/Max Scale constraints (if present)
            int? minScaleLevel = null;
            int maxScaleLevel = 4; // default global max
            if (!string.IsNullOrEmpty(partName))
            {
                minScaleLevel = PartManager.GetMinScaleLevelForPart(partName!);
                maxScaleLevel = PartManager.GetMaxScaleLevelForPart(partName!);
            }

            // Determine desired scale:
            // - If definition provides Scale (e.g., Frame), use it without prompting
            // - Else prompt the user to choose a valid scale (filtered by parent)
            string? desiredScale = definition? ["Scale"]?.ToString();
            if (string.IsNullOrEmpty(desiredScale))
            {
                desiredScale = PartManager.ChooseScale(parentScale, minScaleLevel, maxScaleLevel);
                if (string.IsNullOrEmpty(desiredScale))
                {
                    Console.WriteLine("Invalid scale selection!");
                    return -1;
                }
            }

            // Enforce: part scale must be <= parent scale (if parent exists)
            if (!string.IsNullOrEmpty(parentScale))
            {
                int childLevel = PartManager.GetScaleLevel(desiredScale!);
                int parentLevel = PartManager.GetScaleLevel(parentScale!);
                if (childLevel > parentLevel)
                {
                    Console.WriteLine($"Cannot add part with Scale {desiredScale} larger than parent Scale {parentScale}.");
                    return -1;
                }
            }

            // Enforce: part scale must be >= MinScale (if defined)
            if (minScaleLevel.HasValue)
            {
                int childLevel = PartManager.GetScaleLevel(desiredScale!);
                if (childLevel < minScaleLevel.Value)
                {
                    Console.WriteLine($"Cannot add part with Scale {desiredScale} below its MinScale ({minScaleLevel.Value}).");
                    return -1;
                }
            }

            // Enforce: part scale must be <= MaxScale
            if (!string.IsNullOrEmpty(desiredScale))
            {
                int childLevel = PartManager.GetScaleLevel(desiredScale!);
                if (childLevel > maxScaleLevel)
                {
                    Console.WriteLine($"Cannot add part with Scale {desiredScale} above its MaxScale ({maxScaleLevel}).");
                    return -1;
                }
            }

            // Set the chosen/defined scale on the part
            partToAdd["Scale"] = JsonValue.Create(desiredScale);
            // Store MinScale/MaxScale on part for visibility (optional)
            if (minScaleLevel.HasValue)
                partToAdd["MinScale"] = JsonValue.Create(minScaleLevel.Value.ToString());
            if (maxScaleLevel > 0)
                partToAdd["MaxScale"] = JsonValue.Create(maxScaleLevel.ToString());

            // Increment the counter and assign PartID
            IDCounter++;
            partToAdd["PartID"] = IDCounter;

            // Copy additional properties from definition (won't override chosen Scale if definition lacks it)
            if (definition != null)
            {
                foreach (var prop in PartManager.GetPartProperties(definition["type"]?.ToString() ?? ""))
                {
                    if (definition[prop] != null)
                    {
                        // Create a new JsonNode with the value instead of reusing the existing one
                        string valueStr = definition[prop]!.ToString();
                        partToAdd[prop] = JsonValue.Create(valueStr);
                    }
                }
            }

            // If no parentPartID specified, add directly to root
            if (parentPartID == null)
            {
                string key = $"Part_{IDCounter}";
                managedTemplate[key] = partToAdd;
                return (IDCounter);
            }

            // Find the parent part by PartID
            JsonObject? targetParent = FindPartByID(managedTemplate, parentPartID);

            if (targetParent != null)
            {
                // Create "children" array if it doesn't exist
                if (targetParent["children"] == null)
                {
                    targetParent["children"] = new JsonArray();
                }

                var children = targetParent["children"]?.AsArray();
                if (children != null)
                {
                    children.Add(partToAdd);
                }
            }
            else
            {
                Console.WriteLine($"Parent with PartID '{parentPartID}' not found!");
            }
            return (IDCounter);
        }
        
        /// <summary>
        /// Finds a part by ID
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="partID"></param>
        /// <returns></returns>
        private JsonObject? FindPartByID(JsonObject obj, string partID)
        {
            // Check if current object has the PartID
            if (obj["PartID"]?.ToString() == partID)
            {
                return obj;
            }

            // Search through all properties
            foreach (var property in obj)
            {
                if (property.Value is JsonObject childObj)
                {
                    var result = FindPartByID(childObj, partID);
                    if (result != null) return result;
                }
                else if (property.Value is JsonArray array)
                {
                    foreach (var item in array)
                    {
                        if (item is JsonObject arrayObj)
                        {
                            var result = FindPartByID(arrayObj, partID);
                            if (result != null) return result;
                        }
                    }
                }
            }
            return null;
        }
    }
}

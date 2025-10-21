using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
namespace Templates
{
    class Templatemanager
    {
        JsonObject managedTemplate;
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
            // Increment the counter and assign PartID
            IDCounter++;
            partToAdd["PartID"] = IDCounter;

            // If no parentPartID specified, add directly to root
            if (parentPartID == null)
            {
                string key = $"Part_{IDCounter}";
                managedTemplate[key] = partToAdd;
                return (IDCounter);
            }

            // Find the parent part by PartID
            JsonObject targetParent = FindPartByID(managedTemplate, parentPartID);

            if (targetParent != null)
            {
                // Create "children" array if it doesn't exist
                if (targetParent["children"] == null)
                {
                    targetParent["children"] = new JsonArray();
                }

                // Add to children array
                targetParent["children"].AsArray().Add(partToAdd);
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
        private JsonObject FindPartByID(JsonObject obj, string partID)
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

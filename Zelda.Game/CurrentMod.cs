using System.Collections.Generic;

namespace Zelda.Game
{
    static class CurrentMod
    {
        internal static ModResources Resources { get; private set; }
    
        public static void Initialize()
        {
            Resources = new ModResources();
            Resources.ImportFromModFile("project_db.xml");
        }

        public static void Quit()
        {
            Resources.Clear();
        }

        public static bool ResourceExists(ResourceType resourceType, string id)
        {
            return Resources.Exists(resourceType, id);
        }

        public static IReadOnlyDictionary<string, string> GetResources(ResourceType resourceType)
        {
            return Resources.GetElements(resourceType);
        }
    }
}


using System.Collections.Generic;
namespace Zelda.Game
{
    static class CurrentMod
    {
        static ModResources _resources;
        internal static ModResources Resources
        {
            get { return _resources; }
        }

        public static void Initialize()
        {
            _resources = new ModResources();
            _resources.ImportFromModFile("project_db.xml");
        }

        public static void Quit()
        {
            _resources.Clear();
        }

        public static bool ResourceExists(ResourceType resourceType, string id)
        {
            return _resources.Exists(resourceType, id);
        }

        public static IReadOnlyDictionary<string, string> GetResources(ResourceType resourceType)
        {
            return _resources.GetElements(resourceType);
        }
    }
}

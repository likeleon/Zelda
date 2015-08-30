using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Modules.MainMenu.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class ModFileContextMenuBuilder
    {
        [CommandDefinition]
        public class AddToModAsCommandDefinition : CommandDefinition
        {
            public const string CommandName = "ResourceBrowser.AddTomodAs";
            public override string Name { get { return CommandName; } }
            public override string Text { get {  return "Add to mod as {0}".F()} }
        }

        readonly IModFile _modFile;

        public static IEnumerable<CommandMenuItem> Build(IModFile modFile)
        {
            yield return BuildOpenMenu();
        }

        ModFileContextMenuBuilder(IModFile modFile)
        {
            _modFile = modFile;
        }

        IEnumerable<CommandMenuItem> BuildOpenMenu()
        {
            var mod = _modFile.Mod;
            var resources = mod.Resources;

            var resourceType = ResourceType.Map;
            var elementId = "";
            var isPotentialResourceElement = mod.IsPotentialResourceElement(_modFile.Path, ref resourceType, ref elementId);
            var isDeclaredResourceElement = isPotentialResourceElement && resources.Exists(resourceType, elementId);
            var isDir = mod.IsDirectory(_modFile.Path);

            Command newResourceElementCommand = null;

            if (isPotentialResourceElement)
            {
                if (isDeclaredResourceElement)
                    return Enumerable.Empty<CommandMenuItem>();

                var resourceTypeFriendlyName = resources.GetFriendlyName(resourceType);
                var resourceTypeName = resources.GetTypeName(resourceType);

                newResourceElementCommand = new Command(CommandDefinition)
                {
                }
            }
        }
    }
}

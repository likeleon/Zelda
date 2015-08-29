using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;
using Zelda.Editor.Modules.MainMenu.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser.ViewModels
{
    [Export(typeof(IResourceBrowser))]
    class ResourceBrowserViewModel : Tool, IResourceBrowser
    {
        readonly IModService _modService;
        readonly ICommandService _commandService;

        public override PaneLocation PreferredLocation { get { return PaneLocation.Left; } }
        public override double PreferredWidth { get { return 400.0; } }
        public IEnumerable<IModFile> ModRootFiles { get; private set; }
        public IModFile SelectedModFile { get; set; }
        public IEnumerable<CommandMenuItem> SelectedModFileContextMenuItems { get { return GetSelectedModFileContextMenuItems(); } }

        [ImportingConstructor]
        public ResourceBrowserViewModel(IModService modService, ICommandService commandService)
        {
            _modService = modService;
            _commandService = commandService;

            DisplayName = "Resource Browser";

            _modService.Loaded += ModService_Loaded;
            _modService.Unloaded += ModService_Unloaded;
        }

        void ModService_Loaded(object sender, EventArgs e)
        {
            ModRootFiles = ModFileBuilder.Build(_modService.Mod).Yield();
            NotifyOfPropertyChange(() => ModRootFiles);
        }

        void ModService_Unloaded(object sender, EventArgs e)
        {
            ModRootFiles = null;
            NotifyOfPropertyChange(() => ModRootFiles);
        }

        IEnumerable<CommandMenuItem> GetSelectedModFileContextMenuItems()
        {
            var commandDefinition = _commandService.GetCommandDefinition(typeof(TestCommandDefinition));
            var command = _commandService.GetCommand(commandDefinition);
            return new CommandMenuItem(command).Yield();
        }
    }

    [CommandDefinition]
    public class TestCommandDefinition : CommandDefinition
    {
        public const string CommandName = "ResourceBrowser.ContextMenu.TestCommand";

        public override string Name { get { return CommandName; } }

        public override string Text
        {
            get { return "_Test"; }
        }

        public override string ToolTip
        {
            get { return "Test Command"; }
        }

        public override Uri IconSource
        {
            get
            {
                return new Uri("/Resources/Icons/BuildErrorList_7237.png", UriKind.Relative);
            }
        }
    }

    [CommandHandler]
    public class TestCommandHandler : CommandHandlerBase<TestCommandDefinition>
    {
        [ImportingConstructor]
        public TestCommandHandler()
        {
        }

        public override Task Run(Command command)
        {
            MessageBox.Show("TestCommand", "Test", MessageBoxButton.OK);
            return TaskUtility.Completed;
        }
    }
}

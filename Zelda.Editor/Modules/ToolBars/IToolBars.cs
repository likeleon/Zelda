using Caliburn.Micro;

namespace Zelda.Editor.Modules.ToolBars
{
    interface IToolBars
    {
        IObservableCollection<IToolBar> Items { get; }
        bool Visible { get; set; }
    }
}

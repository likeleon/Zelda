using Caliburn.Micro;
using System;
using System.Windows.Input;

namespace Zelda.Editor.Core
{
    public interface ILayoutItem : IScreen
    {
        Guid Id { get; }
        string ContentId { get; }
        ICommand CloseCommand { get; }
        Uri IconSource { get; }
        bool IsSelected { get; set; }
    }
}

namespace Zelda.Editor.Modules.UndoRedo
{
    interface IUndoableAction
    {
        string Name { get; }

        void Execute();
        void Undo();
    }
}
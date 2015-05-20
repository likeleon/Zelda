using RawItem = Zelda.Game.EquipmentItem;

namespace Zelda.Game.Script
{
    public abstract class Item
    {
        RawItem _rawItem;

        internal void NotifyOnCreated(RawItem rawItem)
        {
            _rawItem = rawItem;

            ScriptTools.ExceptionBoundaryHandle(() => { OnCreated(); });
        }

        protected virtual void OnCreated()
        {
        }

        internal void NotifyOnStarted()
        {
            ScriptTools.ExceptionBoundaryHandle(() => { OnStarted(); });
        }

        protected virtual void OnStarted()
        {
        }

        internal void NotifyOnFinished()
        {
            ScriptTools.ExceptionBoundaryHandle(() => { OnFinished(); });
        }

        protected virtual void OnFinished()
        {
        }
    }
}

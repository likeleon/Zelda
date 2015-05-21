using RawItem = Zelda.Game.EquipmentItem;

namespace Zelda.Game.Script
{
    public abstract class Item
    {
        internal RawItem RawItem { get; private set; }

        public Game Game
        {
            get { return ScriptTools.ExceptionBoundaryHandle<Game>(() => RawItem.Savegame.ScriptGame); }
        }

        public string SavegameVariable
        {
            get
            {
                return ScriptTools.ExceptionBoundaryHandle<string>(() => { return RawItem.SavegameVariable; });
            }
            set
            {
                ScriptTools.ExceptionBoundaryHandle(() => { RawItem.SavegameVariable = value; });
            }
        }

        public bool IsAssignable
        {
            get
            {
                return ScriptTools.ExceptionBoundaryHandle<bool>(() => { return RawItem.IsAssignable; });
            }
            set
            {
                ScriptTools.ExceptionBoundaryHandle(() => { RawItem.IsAssignable = value; });
            }
        }

        internal void NotifyOnCreated(RawItem rawItem)
        {
            RawItem = rawItem;

            ScriptTools.ExceptionBoundaryHandle(() => { OnCreated(); });
        }

        protected virtual void OnCreated()
        {
        }

        internal void NotifyStarted()
        {
            ScriptTools.ExceptionBoundaryHandle(() => { OnStarted(); });
        }

        protected virtual void OnStarted()
        {
        }

        internal void NotifyFinished()
        {
            ScriptTools.ExceptionBoundaryHandle(() => { OnFinished(); });
        }

        protected virtual void OnFinished()
        {
        }

        internal void NotifyObtaining(int variant, string savegameVariable)
        {
            ScriptTools.ExceptionBoundaryHandle(() => { OnObtaining(variant, savegameVariable); });
        }

        protected virtual void OnObtaining(int variant, string savegameVariable)
        {
        }

        internal void NotifyUsing()
        {
            ScriptTools.ExceptionBoundaryHandle(() => { OnUsing(); });
        }

        protected virtual void OnUsing()
        {
        }
    }
}

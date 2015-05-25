using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public abstract class ScriptItem
    {
        internal EquipmentItem _item { get; private set; }

        public ScriptGame Game
        {
            get { return ScriptTools.ExceptionBoundaryHandle<ScriptGame>(() => _item.Savegame.ScriptGame); }
        }

        public ScriptMap Map
        {
            get { return ScriptTools.ExceptionBoundaryHandle<ScriptMap>(() => _item.Game.CurrentMap.ScriptMap); }
        }

        public string SavegameVariable
        {
            get
            {
                return ScriptTools.ExceptionBoundaryHandle<string>(() => { return _item.SavegameVariable; });
            }
            set
            {
                ScriptTools.ExceptionBoundaryHandle(() => { _item.SavegameVariable = value; });
            }
        }

        public bool IsAssignable
        {
            get
            {
                return ScriptTools.ExceptionBoundaryHandle<bool>(() => { return _item.IsAssignable; });
            }
            set
            {
                ScriptTools.ExceptionBoundaryHandle(() => { _item.IsAssignable = value; });
            }
        }

        public void SetFinished()
        {
            Hero hero = _item.Game.Hero;
            if (hero.IsUsingItem)
                hero.ItemBeingUsed.IsFinished = true;
        }

        internal void NotifyCreated(EquipmentItem item)
        {
            _item = item;

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

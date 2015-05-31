using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public abstract class ScriptItem
    {
        internal EquipmentItem _item { get; private set; }

        public ScriptGame Game { get { return _item.Savegame.ScriptGame; } }
        public ScriptMap Map { get { return _item.Game.CurrentMap.ScriptMap; } }

        public string SavegameVariable
        {
            get { return _item.SavegameVariable; }
            set { _item.SavegameVariable = value; }
        }

        public bool IsAssignable
        {
            get { return _item.IsAssignable; }
            set { _item.IsAssignable = value; }
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

            CoreToScript.Call(OnCreated);
        }

        protected virtual void OnCreated()
        {
        }

        internal void NotifyStarted()
        {
            CoreToScript.Call(OnStarted);
        }

        protected virtual void OnStarted()
        {
        }

        internal void NotifyFinished()
        {
            CoreToScript.Call(OnFinished);
        }

        protected virtual void OnFinished()
        {
        }

        internal void NotifyObtaining(int variant, string savegameVariable)
        {
            CoreToScript.Call(() => OnObtaining(variant, savegameVariable));
        }

        protected virtual void OnObtaining(int variant, string savegameVariable)
        {
        }

        internal void NotifyUsing()
        {
            CoreToScript.Call(OnUsing);
        }

        protected virtual void OnUsing()
        {
        }

        internal void NotifyAbilityUsed(Ability ability)
        {
            CoreToScript.Call(() => OnAbilityUsed(ability));
        }

        protected virtual void OnAbilityUsed(Ability ability)
        {
        }
    }
}

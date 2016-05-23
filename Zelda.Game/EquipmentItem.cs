using System;
using Zelda.Game.Script;

namespace Zelda.Game
{
    public class EquipmentItem : ITimerContext, IMenuContext
    {
        public Savegame Savegame => Equipment.Savegame;
        public Map Map => Game.CurrentMap;

        public string Name { get; }
        public string SavegameVariable { get; set; } = "";
        public string AmountSavegameVariable { get; set; }
        public bool HasAmount => !AmountSavegameVariable.IsNullOrEmpty();
        public bool IsObtainable { get; set; } = true;
        public bool IsAssignable { get; set; }
        public string SoundWhenBrandished { get; set; } = "treasure";

        public int Variant
        {
            get
            {
                if (!IsSaved)
                    throw new InvalidOperationException("The item '{0}' is not saved".F(Name));
                return Savegame.GetInteger(SavegameVariable);
            }
        }

        public int Amount
        {
            get
            {
                if (!HasAmount)
                    throw new InvalidOperationException("The item '{0}' has no amount".F(Name));
                return Savegame.GetInteger(AmountSavegameVariable);
            }
        }

        internal Game Game => Equipment.Game;
        internal Equipment Equipment { get; }
        internal bool IsSaved => !SavegameVariable.IsNullOrEmpty();

        public EquipmentItem(Equipment equipment, string name)
        {
            Equipment = equipment;
            Name = name;
        }

        public void SetVariant(int variant)
        {
            Debug.CheckAssertion(IsSaved, "The item '{0}' is not saved".F(Name));

            Savegame.SetInteger(SavegameVariable, variant);
        }

        internal void Start()
        {
            OnStarted();
        }

        protected virtual void OnStarted() { }

        internal void Finish()
        {
            OnFinished();
            Timer.RemoveTimers(this);
            ScriptMenu.RemoveMenus(this);
        }

        protected virtual void OnFinished() { }

        internal void NotifyUsing()
        {
            OnUsing();
        }

        protected virtual void OnUsing() { }

        internal void NotifyAbilityUsed(Ability ability)
        {
            OnAbilityUsed();
        }

        protected virtual void OnAbilityUsed() { }

        public virtual void OnObtaining(int variant, string savegameVariable) { }
        public virtual void OnObtained(int variant, string savegameVariable) { }

        public void SetFinished()
        {
            if (Game.Hero.IsUsingItem)
                Game.Hero.ItemBeingUsed.IsFinished = true;
        }
    }
}

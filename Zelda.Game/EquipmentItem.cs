using System;
using Zelda.Game.Script;

namespace Zelda.Game
{
    class EquipmentItem
    {
        readonly Equipment _equipment;
        ScriptItem _scriptItem;

        public Equipment Equipment { get { return _equipment; } }
        public Game Game { get { return _equipment.Game; } }
        public Savegame Savegame { get { return _equipment.Savegame; } }

        public string Name { get; set; }
        
        public bool IsSaved { get { return !String.IsNullOrEmpty(SavegameVariable); } }
        public string SavegameVariable { get; set; }
        
        public string AmountSavegameVariable { get; set; }
        public bool HasAmount { get { return !String.IsNullOrEmpty(AmountSavegameVariable); } }

        public bool IsObtainable { get; set; }
        public bool IsAssignable { get; set; }

        public string SoundWhenBrandished { get; set; }

        public int Variant
        {
            get
            {
                Debug.CheckAssertion(IsSaved, "The item '{0}' is not saved".F(Name));
                return Savegame.GetInteger(SavegameVariable);
            }
        }

        public int Amount
        {
            get
            {
                Debug.CheckAssertion(HasAmount, "The item '{0}' has no amount".F(Name));
                return Savegame.GetInteger(AmountSavegameVariable);
            }
        }

        public EquipmentItem(Equipment equipment)
        {
            _equipment = equipment;
            Name = String.Empty;
            SavegameVariable = String.Empty;
            IsObtainable = true;
            SoundWhenBrandished = "treasure";
        }

        public void SetVariant(int variant)
        {
            Debug.CheckAssertion(IsSaved, "The item '{0}' is not saved".F(Name));

            Savegame.SetInteger(SavegameVariable, variant);
        }

        public void Initialize()
        {
            _scriptItem = ScriptContext.RunItem(this);
        }

        public void Start()
        {
            if (_scriptItem != null)
                _scriptItem.NotifyStarted();
        }

        public void Exit()
        {
            if (_scriptItem != null)
                _scriptItem.NotifyFinished();
        }

        public void NotifyUsing()
        {
            if (_scriptItem != null)
                _scriptItem.NotifyUsing();
        }

        public void NotifyAbilityUsed(Ability ability)
        {
            if (_scriptItem != null)
                _scriptItem.NotifyAbilityUsed(ability);
        }

        public void NotifyObtaining(Treasure treasure)
        {
            if (_scriptItem != null)
                _scriptItem.NotifyObtaining(treasure.Variant, IsSaved ? SavegameVariable : null);
        }
    }
}

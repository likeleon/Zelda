using Zelda.Game.Script;

namespace Zelda.Game
{
    class EquipmentItem
    {
        readonly Equipment _equipment;

        public Equipment Equipment { get { return _equipment; } }
        public Game Game { get { return _equipment.Game; } }
        public Savegame Savegame { get { return _equipment.Savegame; } }

        public string Name { get; set; }

        public bool IsSaved { get { return !SavegameVariable.IsNullOrEmpty(); } }
        public string SavegameVariable { get; set; }

        public string AmountSavegameVariable { get; set; }
        public bool HasAmount { get { return !AmountSavegameVariable.IsNullOrEmpty(); } }

        public bool IsObtainable { get; set; }
        public bool IsAssignable { get; set; }

        public string SoundWhenBrandished { get; set; }

        public ScriptItem ScriptItem { get; private set; }

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
            Name = "";
            SavegameVariable = "";
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
            ScriptItem = ScriptContext.RunItem(this);
        }

        public void Start()
        {
            ScriptItem?.NotifyStarted();
        }

        public void Exit()
        {
            ScriptItem?.NotifyFinished();
        }

        public void NotifyUsing()
        {
            ScriptItem?.NotifyUsing();
        }

        public void NotifyAbilityUsed(Ability ability)
        {
            ScriptItem?.NotifyAbilityUsed(ability);
        }

        public void NotifyObtaining(Treasure treasure)
        {
            ScriptItem?.NotifyObtaining(treasure.Variant, IsSaved ? SavegameVariable : null);
        }
    }
}

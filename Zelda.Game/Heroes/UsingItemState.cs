using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class UsingItemState : State
    {
        readonly EquipmentItemUsage _itemUsage;

        public override bool IsUsingItem
        {
            get { return true; }
        }

        public override EquipmentItemUsage ItemBeingUsed
        {
            get { return _itemUsage; }
        }

        public UsingItemState(Hero hero, EquipmentItem item)
            : base(hero, "using item")
        {
            _itemUsage = new EquipmentItemUsage(item);
        }

        public override void Start(State previousState)
        {
            base.Start(previousState);

            bool interaction = false;
            Detector facingEntity = Hero.FacingEntity;
            if (facingEntity != null && !facingEntity.IsBeingRemoved)
                interaction = facingEntity.NotifyInteractionWithItem(_itemUsage.Item);

            if (!interaction)
                _itemUsage.Start();
        }

        public override void Update()
        {
            base.Update();

            if (_itemUsage.IsFinished && IsCurrentState)
                Hero.SetState(new FreeState(Hero));
        }
    }
}

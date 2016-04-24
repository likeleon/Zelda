
namespace Zelda.Game
{
    class EquipmentItemUsage
    {
        readonly int _variant;

        public EquipmentItem Item { get; }
        public bool IsFinished { get; set; }

        public EquipmentItemUsage(EquipmentItem item)
        {
            Item = item;
            _variant = item.Variant;
            IsFinished = true;
        }

        public void Start()
        {
            Debug.CheckAssertion(_variant > 0, "Attempt to use equipment item '{0}' without having it".F(Item.Name));

            IsFinished = false;
            Item.NotifyUsing();
        }
    }
}

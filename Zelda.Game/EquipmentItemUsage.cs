
namespace Zelda.Game
{
    class EquipmentItemUsage
    {
        readonly EquipmentItem _item;
        public EquipmentItem Item
        {
            get { return _item; }
        }

        readonly int _variant;

        public bool IsFinished { get; set; }

        public EquipmentItemUsage(EquipmentItem item)
        {
            _item = item;
            _variant = item.Variant;
            IsFinished = true;
        }

        public void Start()
        {
            Debug.CheckAssertion(_variant > 0, "Attempt to use equipment item '{0}' without having it".F(_item.Name));

            IsFinished = false;
            _item.NotifyUsing();
        }
    }
}

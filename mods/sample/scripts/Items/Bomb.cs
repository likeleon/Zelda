using Zelda.Game;
using Zelda.Game.Entities;
using Zelda.Game.Script;

namespace Sample.Items
{
    [Id("bomb")]
    class Bomb : EquipmentItem
    {
        public Bomb(Equipment equipment, string name)
        : base(equipment, name)
        {
            SavegameVariable = "bomb";
            IsAssignable = true;
        }

        public override void OnObtaining(int variant, string savegameVariable)
        {
            Savegame.SetItemAssigned(1, this);
        }

        protected override void OnUsing()
        {
            var hero = Map.GetEntity<Hero>("hero");

            var xy = hero.XY;
            var direction = hero.AnimationDirection;
            if (direction == Direction4.Right)
                xy.X += 16;
            else if (direction == Direction4.Up)
                xy.Y -= 16;
            else if (direction == Direction4.Left)
                xy.X -= 16;
            else if (direction == Direction4.Down)
                xy.Y += 16;

            Map.CreateBomb(new BombData()
            {
                Layer = hero.Layer,
                XY = xy
            });
            SetFinished();
        }
    }
}

﻿using Zelda.Game;
using Zelda.Game.Engine;
using Zelda.Game.Entities;
using Zelda.Game.Script;

namespace Sample.Items
{
    [Id("bomb")]
    class Bomb : ScriptItem
    {
        protected override void OnCreated()
        {
            SavegameVariable = "bomb";
            IsAssignable = true;
        }

        protected override void OnObtaining(int variant, string savegameVariable)
        {
            Game.SetItemAssigned(1, this);
        }

        protected override void OnUsing()
        {
            ScriptHero hero = Map.GetEntity("hero") as ScriptHero;
            
            Point xy = hero.Position;
            Direction4 direction = hero.Direction;
            if (direction == Direction4.Right)
                xy.X += 16;
            else if (direction == Direction4.Up)
                xy.Y -= 16;
            else if (direction == Direction4.Left)
                xy.X -= 16;
            else if (direction == Direction4.Down)
                xy.Y += 16;

            ScriptMap.CreateBomb(Map, new BombData()
            {
                Layer = hero.Layer,
                XY = xy
            });
            SetFinished();
        }
    }
}

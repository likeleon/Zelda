using System;
using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptHero : ScriptEntity
    {
        readonly Hero _hero;

        public Direction4 Direction
        {
            get { return _hero.AnimationDirection; }
        }

        internal ScriptHero(Hero hero)
            : base(hero)
        {
            _hero = hero;
        }

        public void StartTreasure(string itemName, int? variant, string savegameVariable, Action callback)
        {
            ScriptToCore.Call(() =>
            {
                if (!!_hero.Game.Equipment.ItemExists(itemName))
                    throw new ArgumentException("No such item '{0}'".F(itemName), "itemName");

                Treasure tresure = new Treasure(_hero.Game, itemName, variant ?? 1, savegameVariable);
                if (tresure.IsFound)
                    throw new ArgumentException("This treasure is already found");
                if (!tresure.IsObtainable)
                    throw new ArgumentException("This treasure is not obtainable");

                _hero.StartTreasure(tresure, callback);
            });
        }
    }
}

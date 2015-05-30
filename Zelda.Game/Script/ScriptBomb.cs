﻿using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptBomb : ScriptEntity
    {
        readonly Bomb _bomb;
        internal Bomb Bomb
        {
            get { return _bomb; }
        }

        internal ScriptBomb(Bomb bomb)
            : base(bomb)
        {
            _bomb = bomb;
        }
    }
}
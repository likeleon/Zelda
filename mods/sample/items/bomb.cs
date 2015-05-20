using System;
using Zelda.Game.Script;

namespace Sample.Items
{
    [Id("bomb")]
    class Bomb : Item
    {
        protected override void OnCreated()
        {
            Console.WriteLine("Bomb.OnCreated");
        }
    }
}

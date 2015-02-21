using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public abstract class Main
    {
        private readonly MainLoop _mainLoop;

        protected Main(MainLoop mainLoop)
        {
            _mainLoop = mainLoop;
        }

        internal protected virtual void OnStarted()
        {
        }

        internal protected virtual void OnFinished()
        {
        }

        internal protected virtual void OnUpdate()
        {
        }

        internal protected virtual void OnDraw(Surface dstSurface)
        {
        }

        internal protected virtual bool OnInput(Input.Event inputEvent)
        {
            return false;
        }
    }
}

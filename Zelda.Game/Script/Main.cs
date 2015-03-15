
namespace Zelda.Game.Script
{
    public abstract class Main : IInputEventHandler
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

        public virtual bool OnKeyPressed(string key, bool shift, bool control, bool alt)
        {
            return false;
        }

        public virtual bool OnKeyReleased(string key)
        {
            return false;
        }

        public static void Exit()
        {
            ScriptContext.MainLoop.Exiting = true;
        }
    }
}

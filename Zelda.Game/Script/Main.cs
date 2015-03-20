
namespace Zelda.Game.Script
{
    public abstract class Main : IInputEventHandler
    {
        static Main _current;
        public static Main Current
        {
            get { return _current; }
            internal set { _current = value; }
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

        public void Exit()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                ScriptContext.MainLoop.Exiting = true;
            });
        }

        public void Reset()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                ScriptContext.MainLoop.SetResetting();
            });
        }
    }
}

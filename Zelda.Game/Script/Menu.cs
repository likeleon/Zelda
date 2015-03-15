using System;
using System.Linq;

namespace Zelda.Game.Script
{
    public abstract class Menu : IInputEventHandler
    {
        public event EventHandler Started;
        public event EventHandler Finished;

        public static void Start(object context, Menu menu, bool onTop = true)
        {
            ScriptContext.AddMenu(menu, context, onTop);
        }

        protected virtual void OnStarted()
        {
        }
        
        protected internal virtual void OnDraw(Surface dstSurface)
        {
        }

        protected virtual void OnFinished()
        {
        }

        public bool IsStarted()
        {
            return ScriptContext.IsStarted(this);
        }

        public void Stop()
        {
            ScriptContext.Stop(this);
        }

        internal void NotifyStarted()
        {
            OnStarted();
            if (Started != null)
                Started(this, EventArgs.Empty);
        }

        internal void NotifyFinished()
        {
            OnFinished();
            if (Finished != null)
                Finished(this, EventArgs.Empty);
        }

        public virtual bool OnKeyPressed(string key, bool shift, bool control, bool alt)
        {
            return false;
        }

        public virtual bool OnKeyReleased(string key)
        {
            return false;
        }
    }
}

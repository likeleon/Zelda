
namespace Zelda.Game.Script
{
    public abstract class Menu
    {
        public static void Start(object context, Menu menu, bool onTop = true)
        {
            ScriptContext.AddMenu(menu, context, onTop);
        }

        protected internal virtual void OnStarted()
        {
        }
        
        protected internal virtual void OnDraw(Surface dstSurface)
        {
        }
    }
}

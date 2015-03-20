using System;

namespace Zelda.Game.Script
{
    class ScriptTools
    {
        public static T ExceptionBoundaryHandle<T>(Func<T> func)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception ex)
            {
                Debug.Error(ex.ToString());
                return default(T);
            }
        }

        public static void ExceptionBoundaryHandle(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Debug.Error(ex.ToString());
            }
        }
    }
}

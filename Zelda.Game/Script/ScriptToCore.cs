using System;

namespace Zelda.Game.Script
{
    public static class ScriptToCore
    {
        public static T Call<T>(Func<T> func)
        {
            try
            {
                return func.Invoke();
            }
            catch (ScriptException ex)
            {
                Debug.Error(ex.ToString());
            }
            catch (ZeldaFatalException ex)
            {
                Debug.Error("Internal error: " + ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.Error("Unexpected exception: " + ex.ToString());
            }
            return default(T);
        }

        public static void Call(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (ScriptException ex)
            {
                Debug.Error(ex.ToString());
            }
            catch (ZeldaFatalException ex)
            {
                Debug.Error("Internal error: " + ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.Error("Unexpected exception: " + ex.ToString());
            }
        }
    }
}

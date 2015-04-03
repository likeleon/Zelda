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

        public static void ExceptionBoundaryHandle(Action action)
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

        public static void ArgError(string arg_name, string message)
        {
            string msg = "bad argument '{0}' ({1})".F(arg_name, message);
            Error(msg);
        }

        public static void Error(string message)
        {
            throw new ScriptException(message);
        }
    }
}

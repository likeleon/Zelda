using System;

namespace Zelda.Game.Script
{
    class CoreToScript
    {
        public static T Call<T>(Func<T> func)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception ex)
            {
                Debug.Error("Exception: " + ex.ToString());
            }
            return default(T);
        }

        public static void Call(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Debug.Error("Exception: " + ex.ToString());
            }
        }
    }
}

using Caliburn.Micro;
using System.Runtime.CompilerServices;

namespace Zelda.Editor.Core
{
    public static class PropertyChangedBaseExtensions
    {
        public static bool SetProperty<T>(this PropertyChangedBase propertyChangedBase, ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value))
                return false;

            storage = value;
            propertyChangedBase.NotifyOfPropertyChange(propertyName);

            return true;
        }
    }
}

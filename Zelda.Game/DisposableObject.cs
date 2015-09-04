using System;

namespace Zelda.Game
{
    public abstract class DisposableObject : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DisposableObject()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            OnDispose(disposing);
            IsDisposed = true;
        }

        protected abstract void OnDispose(bool disposing);
    }
}

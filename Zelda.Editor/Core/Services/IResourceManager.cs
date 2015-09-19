using System.IO;
using System.Windows.Media.Imaging;

namespace Zelda.Editor.Core.Services
{
    interface IResourceManager
    {
		Stream GetStream(string relativeUri, string assemblyName);
        BitmapImage GetBitmap(string relativeUri, string assemblyName);
        BitmapImage GetBitmap(string relativeUri);
    }
}

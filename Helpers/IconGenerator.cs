using System.Windows.Media.Imaging;
using System.IO;

namespace FPSOptimizer.Helpers
{
    public static class IconGenerator
    {
        /// <summary>
        /// Returns path to SVG icon embedded in resources.
        /// Call this from MainWindow.xaml.cs to set window icon.
        /// </summary>
        public static BitmapImage GenerateIcon()
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();

            // Pack URI to SVG (relative to app root)
            bitmap.UriSource = new Uri("pack://application:,,,/Assets/AppIcon.svg");
            bitmap.CacheOption = BitmapCacheOption.OnLoad;

            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
    }
}

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DiagramDesigner.Platform;
using Invert.Core.GraphDesigner;
using UnityEngine;

namespace DiagramDesigner
{
    public static class DocumentationUtility
    {
        public static BitmapSource ToImage(this IGraphItem item)
        {
            return ViewModelToImage(InvertGraphEditor.Container.CreateViewModel(item));
        }
        public static BitmapSource ViewModelToImage(this ViewModel viewModel, Vector2? size = null)
        {
            var graphViewModel = viewModel as GraphItemViewModel;
            if (graphViewModel != null)
            {
                graphViewModel.IsScreenshot = true;
            }
            var viewModelDrawer = InvertGraphEditor.Container.CreateDrawer(viewModel);
            var bounds = viewModelDrawer.Bounds;
            
            viewModelDrawer.Bounds.Set(0f,0f,bounds.width,bounds.height);
            var result = DrawerToImage(viewModelDrawer,size);
            viewModelDrawer.Bounds = bounds;
            if (graphViewModel != null)
            {
                graphViewModel.IsScreenshot = false;
            }
            return result;
        }
        public static BitmapSource DrawerToImage(IDrawer drawer, Vector2? size = null)
        {
            var platformDrawer = new WindowsPlatformDrawer();
            
         
            drawer.Refresh(platformDrawer);
            var width = Mathf.RoundToInt(drawer.Bounds.width) + 15;
            var height = Mathf.RoundToInt(drawer.Bounds.height) + 15;
            DrawingVisual drawingVisual = new DrawingVisual();
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
               size == null ? width : Mathf.RoundToInt(size.Value.x), size == null ? height : Mathf.RoundToInt(size.Value.y), 96, 96, PixelFormats.Default);
            
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                platformDrawer.Context = drawingContext;
               drawer.Draw(platformDrawer,1f);
            }
            bitmap.Render(drawingVisual);
            return bitmap;
        }

        public static void ToFile(this BitmapSource image, string filename)
        {
            var directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }

        }
    }
}

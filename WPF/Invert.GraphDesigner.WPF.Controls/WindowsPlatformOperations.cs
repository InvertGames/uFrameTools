using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Invert.Core.GraphDesigner;

namespace DiagramDesigner.Platform
{
    public class WindowsPlatformOperations : IPlatformOperations
    {
        public void OpenScriptFile(string filePath)
        {
            
        }

        public string GetAssetPath(object graphData)
        {
            return Directory.GetCurrentDirectory();
        }

        public bool MessageBox(string title, string message, string ok)
        {
            var result = System.Windows.MessageBox.Show(title, message, MessageBoxButton.OK);
            return result == MessageBoxResult.OK;
        }

        public bool MessageBox(string title, string message, string ok, string cancel)
        {
            var result = System.Windows.MessageBox.Show(title, message, MessageBoxButton.OKCancel);
            return result == MessageBoxResult.OK;
        }

        public void SaveAssets()
        {
           
        }

        public void RefreshAssets()
        {
           
        }

        public void Progress(float progress, string message)
        {
            
        }
    }
}

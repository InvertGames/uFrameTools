using System;
using Invert.Core.GraphDesigner;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using UnityEngine;

namespace Invert.uFrame.VS
{
    public class VSPlatform : IPlatformOperations
    {
        public IServiceProvider ServiceProvider
        {
            get { return EditorFactory.VSServiceProvider; }
        }

        public VSPlatform()
        {

        }

        public void OpenScriptFile(string filePath)
        {
            VsShellUtilities.OpenDocument(ServiceProvider, filePath);
        }

        public string GetAssetPath(object graphData)
        {
            if (graphData is IGraphData)
            {
                return ((IGraphData)graphData).Path;
            }
            return null;
        }

        public bool MessageBox(string title, string message, string ok)
        {
            return VsShellUtilities.ShowMessageBox(ServiceProvider, message, title, OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST) == VSConstants.S_OK;
        }

        public bool MessageBox(string title, string message, string ok, string cancel)
        {
            return VsShellUtilities.ShowMessageBox(ServiceProvider, message, title, OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND) == VSConstants.S_OK;
        }

        public void SaveAssets()
        {

        }

        public void RefreshAssets()
        {
            foreach (var project in InvertGraphEditor.Projects)
            {
                project.Refresh();
            }
        }

        public void Progress(float progress, string message)
        {
            var statusBar = ServiceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;

            uint cookie = 0;
            statusBar.Progress(ref cookie, Mathf.RoundToInt(progress), message, 100, 100);
        }
    }
}

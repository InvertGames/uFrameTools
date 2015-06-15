using System;
using System.Diagnostics;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.IOC;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using UnityEngine;

namespace Invert.uFrame.VS
{
    public abstract class VSPlugin : DiagramPlugin
    {
        public IServiceProvider ServiceProvider
        {
            get { return EditorFactory.VSServiceProvider; }
        }
        public override void Initialize(UFrameContainer container)
        {
            //ListenFor<IPlatformOperations>();
        }
    }

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
            var projectService = InvertGraphEditor.Container.Resolve<ProjectService>();
            foreach (var project in projectService.Projects)
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

    public class VSDebug : IDebugLogger
    {
        static VSDebug()
        {
            InvertApplication.Logger = new VSDebug();
        }
        public void Log(string message)
        {
            Debug.WriteLine(message);
        }

        public void LogException(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                LogException(ex.InnerException);
            }
        }
    }
}

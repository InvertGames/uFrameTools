using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner.Unity.WindowsPlugin;
using Invert.IOC;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity.WindowsSystem
{
    
    public class WindowSystemTestPlugin : DiagramPlugin, IContextMenuQuery
    {
        public override void Initialize(UFrameContainer container)
        {
            base.Initialize(container);
            container.RegisterDrawer<HelloWorldWindowViewModel,HelloWorldWindowDrawer>();
        }

        [MenuItem("uFrame Dev/Window System Tester/Open Normal Window In The Center")]
        public static void OpenNormalWindowInTheCenter()
        {
            InvertApplication.SignalEvent<IOpenWindow>(w => w.OpenWindow(new HelloWorldWindowViewModel()));
        }

        public void QueryContextMenu(ICommandUI ui, object obj)
        {
            var diagramNode = obj as DiagramNodeViewModel;
            if (diagramNode != null)
            {
                ui.AddCommand(new OpenWinCommand());
            }
        }
    }

    public class HelloWorldWindowViewModel : WindowViewModel
    {
        public string Message
        {
            get { return "Hello world!"; }
        }
    }

    public class OpenWinCommand : EditorCommand<DiagramNodeViewModel>, IDiagramNodeCommand
    {
        public override string Group
        {
            get { return "OpenWinCommand"; }
        }

        public override decimal Order
        {
            get { return -1; }
        }

        public override bool CanProcessMultiple
        {
            get { return false; }
        }

        public override void Perform(DiagramNodeViewModel node)
        {
            InvertApplication.SignalEvent<IOpenWindow>(w => w.OpenWindow(node));
        }

        public override string CanPerform(DiagramNodeViewModel node)
        {
            var selected = node.DataObject as IDiagramNode;
            if (selected == null) return "Invalid argument";
            //if (node.IsExternal) return "Can't rename a node when its not local.";
            //if (selected.Graph.Identifier != InvertGraphEditor.CurrentDiagramViewModel.GraphData.Identifier)
            //  return "Must be local to rename.";
            return null;
        }


    }

    public class HelloWorldWindowDrawer : Drawer<HelloWorldWindowViewModel>
    {

        public HelloWorldWindowDrawer(HelloWorldWindowViewModel viewModelObject) : base(viewModelObject)
        {
        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            base.Draw(platform, scale);
            platform.DrawLabel(new Rect(0,0,100,100),ViewModel.Message,CachedStyles.GraphTitleLabel,DrawingAlignment.MiddleCenter);
        }
    }


}

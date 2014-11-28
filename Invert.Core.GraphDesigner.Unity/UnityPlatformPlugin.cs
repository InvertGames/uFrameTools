using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.uFrame;
using Invert.uFrame.Editor;

namespace Invert.Core.GraphDesigner.Unity
{
    public class UnityPlatformPlugin : CorePlugin
    {
        public override decimal LoadPriority
        {
            get { return -95; }
        }

        public override bool Required
        {
            get { return true; }
        }

        static UnityPlatformPlugin()
        {
            InvertGraphEditor.Prefs = new UnityPlatformPreferences();
        }
        public override bool Enabled { get { return true; } set{} }
        public override void Initialize(uFrameContainer container)
        {
            InvertGraphEditor.Platform = new UnityPlatform();
            InvertGraphEditor.PlatformDrawer = new UnityDrawer();

            // Drawers
            container.RegisterDrawer<ConnectorViewModel, ConnectorDrawer>();
            container.RegisterDrawer<ConnectionViewModel, ConnectionDrawer>();
            container.RegisterDrawer<InputOutputViewModel, SlotDrawer>();


            // Command Drawers
            container.Register<ToolbarUI, UnityToolbar>();
            container.Register<ContextMenuUI, UnityContextMenu>();

            container.RegisterInstance<IGraphEditorSettings>(new UFrameSettings());
            // Where the generated code files are placed
            container.Register<ICodePathStrategy, DefaultCodePathStrategy>("Default");
            container.RegisterInstance<IToolbarCommand>(new DiagramSettingsCommand() { Title = "Settings" }, "SettingsCommand");
            container.RegisterInstance<IWindowManager>(new UnityWindowManager());

           
        }

        public override void Loaded()
        {
           
        }
    }
}

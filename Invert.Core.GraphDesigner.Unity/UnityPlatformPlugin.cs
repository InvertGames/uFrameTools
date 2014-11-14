using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner.Settings;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;

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

        public override bool Enabled { get { return true; } set{} }
        public override void Initialize(uFrameContainer container)
        {
            // Command Drawers
            container.Register<ToolbarUI, ToolbarUI>();
            container.Register<ContextMenuUI, ContextMenuUI>();

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

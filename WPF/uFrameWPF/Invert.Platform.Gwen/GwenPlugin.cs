using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.GraphDesigner.WPF;
using Invert.IOC;

namespace Invert.Platform.Gwen
{
    public class GwenPlugin : DiagramPlugin
    {
        static GwenPlugin()
        {
            InvertGraphEditor.Prefs = new WindowsPrefs();
            InvertApplication.CachedAssemblies.Add(typeof(GwenPlugin).Assembly);
        }

        public override void Initialize(UFrameContainer container)
        {
            container.RegisterInstance<IStyleProvider>(new GwenStyleProvider());
           
            container.RegisterInstance<IGraphEditorSettings>(new DefaultGraphSettings());
            container.Register<ContextMenuUI, WindowsContextMenu>();
        }
    }
}
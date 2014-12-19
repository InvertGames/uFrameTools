using Invert.Core;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.VS
{
    public class VisualStudioPlugin : DiagramPlugin
    {
        public override decimal LoadPriority
        {
            get { return -2; }
        }

        public override bool Required
        {
            get { return true; }
        }

        public override bool Enabled
        {
            get { return true; }
            set
            {

            }
        }
        public VisualStudioPlugin()
        {
            InvertGraphEditor.Prefs = new WindowsPrefs();
        }



        //public static IVsHierarchy CurrentProject
        //{
        //    get
        //    {
        //        VsShellUtilities.GetHierarchy()
        //        CurrentProjects.FirstOrDefault(p=>p.)
        //    }
        //} 
        public override void Initialize(uFrameContainer container)
        {
            InvertGraphEditor.Platform = new VSPlatform();
            container.Register<IAssetManager,VisualStudioAssetManager>();
            container.RegisterInstance<IWindowManager>(new VSWindows());

        }


    }
}

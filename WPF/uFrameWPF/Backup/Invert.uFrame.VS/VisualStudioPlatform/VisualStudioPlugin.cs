using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.IOC;

namespace Invert.uFrame.VS
{
    public class VisualStudioPlugin : DiagramPlugin
    {
        public override decimal LoadPriority
        {
            get { return 2; }
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
        static VisualStudioPlugin()
        {
            InvertGraphEditor.Prefs = new WindowsPrefs();
            InvertGraphEditor.Platform = new VSPlatform();
            InvertApplication.CachedAssemblies.Add(typeof(VisualStudioPlugin).Assembly);
        }



        //public static IVsHierarchy CurrentProject
        //{
        //    get
        //    {
        //        VsShellUtilities.GetHierarchy()
        //        CurrentProjects.FirstOrDefault(p=>p.)
        //    }
        //} 
        public override void Initialize(UFrameContainer container)
        {
    
            container.Register<IAssetManager,VisualStudioAssetManager>();
            container.RegisterInstance<IWindowManager>(new VSWindows());

        }


    }

    public class VisualStudioProjectService : VSPlugin
    {
        
    }
}

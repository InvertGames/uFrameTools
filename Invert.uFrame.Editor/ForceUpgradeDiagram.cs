using System.Linq;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public class ForceUpgradeDiagram : ElementsDiagramToolbarCommand
    {
        public override string Name
        {
            get { return "Force Upgrade"; }
        }

        public override void Perform(DiagramViewModel node)
        {

            var data = node.CurrentRepository.NodeItems.OfType<SubSystemData>();
            foreach (var view in data)
            {
                
                view.Instances.RemoveAll(p => true);
            }
            var views = node.CurrentRepository.NodeItems.OfType<ViewData>();
            foreach (var view in views)
            {
                view.Bindings.RemoveAll(p => true);
            }
            node.UpgradeProject();
        }
    }
}
using Invert.Core.GraphDesigner;

namespace Invert.Core.GraphDesigner
{
    public class ClassPropertyData : GenericTypedChildItem
    {
        public override string FullLabel
        {
            get { return Name; }
        }

        public override string Label
        {
            get { return Name; }
        }

        public override void Remove(IDiagramNode diagramNode)
        {
            base.Remove(diagramNode);

        }
    }
}
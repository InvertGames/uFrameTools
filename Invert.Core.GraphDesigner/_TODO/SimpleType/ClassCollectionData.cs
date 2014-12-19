namespace Invert.Core.GraphDesigner
{
    public class ClassCollectionData : GenericTypedChildItem
    {
        public override string FullLabel
        {
            get { return Name; }
        }

        public override string Label
        {
            get { return Name; }
        }

        public override string RelatedTypeName
        {
            get
            {
                return base.RelatedTypeName;
            }
        }

        public override void Remove(IDiagramNode diagramNode)
        {
            base.Remove(diagramNode);

        }
    }
}
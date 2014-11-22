using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.Core.GraphDesigner
{
    public class ClassNodeViewModel : DiagramNodeViewModel<ClassNodeData>
    {
        public ClassNodeViewModel(ClassNodeData graphItemObject, DiagramViewModel diagramViewModel)
            : base(graphItemObject, diagramViewModel)
        {
        }

        public void AddProperty()
        {
            GraphItem.NodeItems.Add(new ClassPropertyData()
            {
                Name = DiagramViewModel.CurrentRepository.GetUniqueName("NewClassProperty"),
                Node = GraphItem,
                RelatedType = typeof (string).Name

            });
        }

        public void AddCollection()
        {
            GraphItem.NodeItems.Add(new ClassCollectionData()
            {
                Name = DiagramViewModel.CurrentRepository.GetUniqueName("NewClassCollection"),
                Node = GraphItem,
                RelatedType = typeof (string).Name

            });
        }
    }
}
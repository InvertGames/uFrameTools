using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.Core.GraphDesigner
{
    public class ClassPropertyItemViewModel : TypedItemViewModel<ClassPropertyData>
    {

        public ClassPropertyItemViewModel(ClassPropertyData viewModelItem, DiagramNodeViewModel nodeViewModel)
            : base(viewModelItem, nodeViewModel)
        {
        }

        public override string TypeLabel
        {
            get { return Data.RelatedTypeName; }
        }
    }
}
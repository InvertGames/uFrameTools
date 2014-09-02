using System.Linq;
using System.Text;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public abstract class AddItemCommand<TType> : AddNewCommand, IDiagramContextCommand
    {
        public override void Execute(object item)
        {
            base.Execute(item);
            var node = item as DiagramViewModel;
            if (node == null) return;

            var data = node.Data.AllDiagramItems.LastOrDefault();
       
            if (data == null) return;
            data.BeginEditing();
        }

        public override string CanPerform(DiagramViewModel node)
        {
            if (node == null) return "Diagram must be loaded first.";

            if (!node.Data.CurrentFilter.IsAllowed(null, typeof(TType)))
                return "Item is not allowed in this part of the diagram.";

            return null;
        }

        public override string Path
        {
            get { return  "Add New/" + Title; }
        }
    }

    //public class DeleteCommand : EditorCommand<ISelectable>, IDiagramNodeCommand, IDiagramNodeItemCommand
    //{

    //}
}

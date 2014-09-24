using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class SelectViewBaseElement : EditorCommand<ViewNodeViewModel>, IDynamicOptionsCommand, IDiagramNodeCommand
    {
        public override string Group
        {
            get { return "View"; }
        }

        public override void Perform(ViewNodeViewModel node)
        {
            if (node == null) return;
            var view = SelectedOption.Value as ViewData;
            if (view == null)
            {
                node.GraphItem.BaseViewIdentifier = null;
            }
            else
            {
                node.GraphItem.BaseViewIdentifier = view.Identifier;    
            }
            
        }

        public override string CanPerform(ViewNodeViewModel node)
        {
            if (node == null) return "This operation can only be performed on a view.";
            if (!node.IsLocal) return "This node is not local to the current diagram.";
            return null;
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object item)
        {
            var viewNode = item as ViewNodeViewModel;
            if (viewNode != null)
            {
                var view = viewNode.GraphItem;
                if (view == null) yield break;
                var element = view.ViewForElement;
                if (element == null) yield break;

                var baseViews = element.AllBaseTypes.SelectMany(
                    p => view.Project.NodeItems.OfType<ViewData>().Where(x => x.ViewForElement == p));

                yield return new UFContextMenuItem()
                {
                    Name = "Base View/" + element.NameAsViewBase,
                    Value = null,
                    Checked = string.IsNullOrEmpty(viewNode.GraphItem.BaseViewIdentifier)
                };
                foreach (var baseView in baseViews)
                {
                    yield return new UFContextMenuItem()
                    {
                        Name = "Base View/" + baseView.NameAsView,
                        Value = baseView,
                        Checked = baseView.Identifier == viewNode.GraphItem.BaseViewIdentifier
                    };
                }
            }
        }

        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get; private set; }
    }
}
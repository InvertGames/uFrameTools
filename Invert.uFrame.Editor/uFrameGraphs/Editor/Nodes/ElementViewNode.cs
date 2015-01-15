using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor
{
    public class ElementViewNode : ElementViewNodeBase
    {
        public override IEnumerable<global::Invert.uFrame.Editor.IViewBindings> PossibleBindings
        {
            get
            {

                foreach (var item in Element.PersistedItems.OfType<IBindableTypedItem>())
                {
                    foreach (var mapping in uFramePlugin.BindingTypes)
                    {
                        var bindableType = mapping.Instance as uFrameBindingType;
                        if (bindableType == null) continue;
                        if (!bindableType.CanBind(item)) continue;

                        yield return new ViewBindingsReference()
                        {
                            Node = this,
                            SourceIdentifier = item.Identifier,
                            BindingName = mapping.Name,
                            BindingType = bindableType,
                            Name = string.Format(bindableType.DisplayFormat, item.Name)
                        };

                    }
                }
            }
        }

        public override void Validate(List<ErrorInfo> errors)
        {
            base.Validate(errors);
            if (Element == null)
            {
                errors.AddError("This view must have an element.", this.Identifier, () =>
                {
                    var node = Project.NodeItems.OfType<IDiagramFilter>()
                        .FirstOrDefault(p => p.GetContainingNodes(this.Project).Contains(this)) as ElementNode;
                    if (node != null)
                    {
                        Graph.AddConnection(node, ElementInputSlot);
                    }
                    
                });
            }
        }

        public ElementNode Element
        {
            get
            {

                var elementNode = ElementInputSlot.Item as ElementNode;
                if (elementNode == null)
                {
                    var baseView = BaseNode as ElementViewNode;
                    if (baseView != null)
                    {
                        return baseView.Element;
                    }

                }
                else
                {
                    return elementNode;
                }

                return null;
            }
        }

        public IEnumerable<PropertyChildItem> SceneProperties
        {
            get
            {
                return ScenePropertiesInputSlot.InputsFrom<PropertyChildItem>();
            }
        }

    }
}

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
                            Name = item.Name + " " + bindableType.DisplayName
                        };

                    }
                }
            }
        }

        public ElementNode Element
        {
            get
            {
                var elementNode = this.InputFrom<ElementNode>();
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
    }
}

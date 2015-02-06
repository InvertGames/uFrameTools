namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core.GraphDesigner;
    
    
    public class BinderNode : BinderNodeBase {
        [Invert.Core.GraphDesigner.ReferenceSection("Bindings", SectionVisibility.Always, false, false, typeof(IViewBindings), false, OrderIndex = 2, HasPredefinedOptions = true)]
        public virtual System.Collections.Generic.IEnumerable<ViewBindingsReference> Bindings
        {
            get
            {
                return ChildItems.OfType<ViewBindingsReference>();
            }
        }

        //public ElementComponentNode Component
        //{
        //    get { return ComponentInputSlot.Item as ElementComponentNode; }
        //}
        public IEnumerable<global::Invert.uFrame.Editor.IViewBindings> PossibleBindings
        {
            get
            {

                foreach (var item in Project.AllGraphItems.OfType<IBindableTypedItem>())
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


    }
}

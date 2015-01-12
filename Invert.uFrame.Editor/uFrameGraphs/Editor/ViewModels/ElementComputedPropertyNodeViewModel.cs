using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class ElementComputedPropertyNodeViewModel : ElementComputedPropertyNodeViewModelBase {
        
        public ElementComputedPropertyNodeViewModel(ElementComputedPropertyNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }

        protected override void CreateContent()
        {
            base.CreateContent();
            foreach (var item in this.GraphItem.InputsFrom<PropertyChildItem>())
            {
                var relatedNode = item.RelatedTypeNode as ElementNode;
                if (relatedNode != null)
                {
                    var headerVM = new SectionHeaderViewModel()
                    {
                        Name = relatedNode.Name
                    };

                }
            }
        }
    }
}

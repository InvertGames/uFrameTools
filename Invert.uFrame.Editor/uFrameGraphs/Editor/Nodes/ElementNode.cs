using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    public class ElementNode : ElementNodeBase, global::Invert.uFrame.Editor.IRegisteredInstance,IClassTypeNode {
        public override void Validate(List<ErrorInfo> errors)
        {
            base.Validate(errors);
            var ps = ChildItemsWithInherited.ToArray();
            foreach (var p1 in ps)
            {
                foreach (var p2 in ps)
                {
                    if (p1.Name == p2.Name && p1 != p2)
                    {
                        errors.AddError(string.Format("Duplicate \"{0}\"", p1.Name), this.Identifier);
                    }
                }
            }

        }

        public virtual System.Collections.Generic.IEnumerable<ElementComputedPropertyNode> ComputedProperties {
            get
            {
                return this.ChildItems.OfType<PropertyChildItem>()
                    .SelectMany(p => p.OutputsTo<ElementComputedPropertyNode>())
                    .Distinct();
            }
        }

        public IEnumerable<ITypedItem> 
            
            AllProperties
        {
            get
            {
                foreach (var item in ComputedProperties)
                    yield return item;
                foreach (var item in Properties)
                    yield return item;
            }
        }
        public IEnumerable<ITypedItem> AllPropertiesWithInherited
        {
            get
            {
                var baseElement = BaseNode as ElementNode;
                if (baseElement != null)
                {
                    foreach (var property in baseElement.AllProperties)
                    {
                        yield return property;
                    }
                    foreach (var property in baseElement.AllPropertiesWithInherited)
                    {
                        yield return property;
                    }
                }
            }
        }

 
        public virtual System.Collections.Generic.IEnumerable<PropertyChildItem> Properties
        {
            get
            {
                return ChildItems.OfType<PropertyChildItem>();
            }
        }

        public virtual System.Collections.Generic.IEnumerable<CollectionChildItem> Collections
        {
            get
            {
                return ChildItems.OfType<CollectionChildItem>();
            }
        }

 
        public virtual System.Collections.Generic.IEnumerable<CommandChildItem> Commands
        {
            get
            {
                return ChildItems.OfType<CommandChildItem>();
            }
        }


       [Invert.Core.GraphDesigner.Section("Properties", SectionVisibility.Always, OrderIndex = 0)]
        public virtual System.Collections.Generic.IEnumerable<PropertyChildItem> InheritedProperties
        {
            get
            {
                var baseElement = BaseNode as ElementNode;
                if (baseElement != null)
                {
                    foreach (var property in baseElement.InheritedProperties)
                    {
                        yield return property;
                    }
                }
                else
                {

                }
                foreach (var item in Properties)
                {
                    yield return item;
                }
            }
        }

        [Invert.Core.GraphDesigner.Section("Collections", SectionVisibility.Always, OrderIndex = 1)]
        public virtual System.Collections.Generic.IEnumerable<CollectionChildItem> InheritedCollections
        {
            get
            {
                var baseElement = BaseNode as ElementNode;
                if (baseElement != null)
                {
                    foreach (var property in baseElement.InheritedCollections)
                    {
                        yield return property;
                    }
                }
                foreach (var item in Collections)
                {
                    yield return item;
                }
            }
        }
            [Invert.Core.GraphDesigner.Section("Commands", SectionVisibility.Always, OrderIndex = 2)]
        public virtual System.Collections.Generic.IEnumerable<CommandChildItem> InheritedCommands
        {
            get
            {
                var baseElement = BaseNode as ElementNode;
                if (baseElement != null)
                {

                    foreach (var property in baseElement.InheritedCommands)
                    {
                        yield return property;
                    }
                }
                foreach (var item in Commands)
                {
                    yield return item;
                }
            }
        }
        public string ClassName
        {
            get { return this.Name + "ViewModel"; }
        }

        public override bool ValidateInput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
        {
            if (arg1 is GenericTypedChildItem) return true;
            return base.ValidateInput(arg1, arg2);
        }
        public override bool ValidateOutput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
        {
            if (arg1 is GenericTypedChildItem) return true;
            return base.ValidateOutput(arg1, arg2);
        }
    }
}

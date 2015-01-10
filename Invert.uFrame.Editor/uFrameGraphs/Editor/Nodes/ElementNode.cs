using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    public class ElementNode : ElementNodeBase, global::Invert.uFrame.Editor.IRegisteredInstance,IClassTypeNode {
        public override void Validate(List<ErrorInfo> errors)
        {
            base.Validate(errors);
            var ps = ChildItems;
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

        public override System.Collections.Generic.IEnumerable<ElementComputedPropertyNode> ComputedProperties {
            get
            {
                return this.ChildItems.OfType<PropertyChildItem>()
                    .SelectMany(p => p.OutputsTo<ElementComputedPropertyNode>())
                    .Distinct();
            }
        }

        public IEnumerable<ITypedItem> AllProperties
        {
            get
            {
                foreach (var item in ComputedProperties)
                    yield return item;
                foreach (var item in Properties)
                    yield return item;
            }
        }

        [Invert.Core.GraphDesigner.ProxySection("Inherited Properties", SectionVisibility.WhenNodeIsFilter, OrderIndex =1)]
        public virtual System.Collections.Generic.IEnumerable<PropertyChildItem> InheritedProperties
        {
            get
            {
                var baseElement = BaseNode as ElementNode;
                if (baseElement != null)
                {
                    foreach (var property in baseElement.Properties)
                    {
                        yield return property;
                    }
                    foreach (var property in baseElement.InheritedProperties)
                    {
                        yield return property;
                    }
                }
            }
        }
        [Invert.Core.GraphDesigner.ProxySection("Inherited Collections", SectionVisibility.WhenNodeIsFilter, OrderIndex = 1)]
        public virtual System.Collections.Generic.IEnumerable<CollectionChildItem> InheritedCollections
        {
            get
            {
                var baseElement = BaseNode as ElementNode;
                if (baseElement != null)
                {
                    foreach (var property in baseElement.Collections)
                    {
                        yield return property;
                    }
                    foreach (var property in baseElement.InheritedCollections)
                    {
                        yield return property;
                    }
                }
            }
        }
        [Invert.Core.GraphDesigner.ProxySection("Inherited Commands", SectionVisibility.WhenNodeIsFilter, OrderIndex = 1)]
        public virtual System.Collections.Generic.IEnumerable<CommandChildItem> InheritedCommands
        {
            get
            {
                var baseElement = BaseNode as ElementNode;
                if (baseElement != null)
                {
                    foreach (var property in baseElement.Commands)
                    {
                        yield return property;
                    }
                    foreach (var property in baseElement.InheritedCommands)
                    {
                        yield return property;
                    }
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

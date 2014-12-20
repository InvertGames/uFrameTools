using System;
using System.ComponentModel;
using System.Reflection;

namespace Invert.Core.GraphDesigner
{
    public class NodeInputConfig : GraphItemConfiguration
    {
        public bool AllowMultiple { get; set; }
        public ConfigProperty<IDiagramNodeItem, string> Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private ConfigProperty<IDiagramNodeItem, string> _name;

        public NodeInputConfig NameConfig(ConfigProperty<IDiagramNodeItem, string> name)
        {
            Name = name;
            return this;
        }

        public NodeInputConfig NameConfig(string literal)
        {
            Name = new ConfigProperty<IDiagramNodeItem, string>(literal);
            return this;
        }

        public NodeInputConfig NameConfig(Func<IDiagramNodeItem, string> selector)
        {
            Name = new ConfigProperty<IDiagramNodeItem, string>(selector);
            return this;
        }
        //public string Name { get; set; }
        public string OutputName { get; set; }
        public bool IsAlias { get; set; }
        public Func<IDiagramNodeItem, IDiagramNodeItem, bool> Validator { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public Slot AttributeInfo { get; set; }

        public IDiagramNodeItem GetDataObject(GenericNode node)
        {
            if (IsAlias) return node;
            if (PropertyInfo != null)
            {
                var result = PropertyInfo.GetValue(node, null) as GenericSlot;
                if (result == null)
                {
                    var slot = Activator.CreateInstance((Type)PropertyInfo.PropertyType) as GenericSlot;
                    slot.Node = node;
                    slot.Name = AttributeInfo.Name;
                    PropertyInfo.SetValue(node, slot, null);
                    return slot;
                }
                return result;
            }
            return node.GetConnectionReference(ReferenceType);
        }

    }

    public class GraphItemAttribute : Attribute
    {
        public int OrderIndex { get; set; }
    }
    public class Slot : GraphItemAttribute
    {
        
        public string Name { get; set; }
        public Type SourceType { get; set; }
        public bool AllowMultiple { get; set; }
        public SectionVisibility Visibility { get; set; }

        public Slot(string name)
        {
            Name = name;
        }

        public Slot(string name, bool allowMultiple, SectionVisibility visibility)
        {
            Name = name;
            AllowMultiple = allowMultiple;
            Visibility = visibility;
        }

        public Slot(string name, Type sourceType, bool allowMultiple)
        {
            Name = name;
            SourceType = sourceType;
            AllowMultiple = allowMultiple;
        }

        public Slot(string name, Type sourceType, bool allowMultiple, SectionVisibility visibility)
        {
            Name = name;
            SourceType = sourceType;
            AllowMultiple = allowMultiple;
            Visibility = visibility;
        }
    }

    public class InputSlot : Slot
    {
        public InputSlot(string name) : base(name)
        {
        }

        public InputSlot(string name, Type sourceType, bool allowMultiple) : base(name, sourceType, allowMultiple)
        {
        }

        public InputSlot(string name, bool allowMultiple, SectionVisibility visibility) : base(name, allowMultiple, visibility)
        {
        }
    }

    public class OutputSlot : Slot
    {
        public OutputSlot(string name) : base(name)
        {
        }

        public OutputSlot(string name, Type sourceType, bool allowMultiple) : base(name, sourceType, allowMultiple)
        {
        }

        public OutputSlot(string name, bool allowMultiple, SectionVisibility visibility) : base(name, allowMultiple, visibility)
        {
        }
    }


    public class Section : GraphItemAttribute
    {
        public string Name { get; set; }
        public SectionVisibility Visibility { get; set; }

        public Section(string name, SectionVisibility visibility)
        {
            Name = name;
            Visibility = visibility;
        }
    }

    public class ConfigProxy : GraphItemAttribute
    {
        public SectionVisibility Visibility { get; set; }

        public ConfigProxy(SectionVisibility visibility)
        {
            Visibility = visibility;
        }
    }

    public class ProxySection : Section
    {
        public ProxySection(string name, SectionVisibility visibility) : base(name, visibility)
        {
        }
    }
    [Browsable(false)]
    public class ReferenceSection : Section
    {
        private bool _editable = true;

        public ReferenceSection(string name, SectionVisibility visibility, bool allowDuplicates, bool automatic, Type referenceType, bool editable = true)
            : base(name, visibility)
        {
            AllowDuplicates = allowDuplicates;
            Automatic = automatic;
            ReferenceType = referenceType;
            Editable = editable;
        }

        public Type ReferenceType { get; set; }
        public bool AllowDuplicates { get; set; }
        public bool Automatic { get; set; }
        public bool HasPredefinedOptions { get; set; }
        public bool Editable
        {
            get { return _editable; }
            set { _editable = value; }
        }

        public ReferenceSection(string name, SectionVisibility visibility, bool allowDuplicates) : base(name, visibility)
        {
            AllowDuplicates = allowDuplicates;
        }

        public ReferenceSection(string name, SectionVisibility visibility, bool allowDuplicates, bool automatic, bool editable = true) : base(name, visibility)
        {
            AllowDuplicates = allowDuplicates;
            Automatic = automatic;
            Editable = editable;
        }
    }
    public enum SectionVisibility
    {
        Always,
        WhenNodeIsFilter,
        WhenNodeIsNotFilter
    }



}
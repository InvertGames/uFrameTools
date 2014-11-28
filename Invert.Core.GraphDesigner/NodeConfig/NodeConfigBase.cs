using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invert.uFrame;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public abstract class NodeConfigBase
    {
        public Func<IDiagramNodeItem, IDiagramNodeItem, bool> InputValidator { get; set; }
        public Func<IDiagramNodeItem, IDiagramNodeItem, bool> OutputValidator { get; set; }

        protected NodeConfigBase(IUFrameContainer container)
        {
            Container = container;
            InputValidator = (a,b) => true;
            OutputValidator = (a, b) => true;
        }

        public string Name
        {
            get { return _name ?? NodeType.Name; }
            set { _name = value; }
        }

        public Type NodeType
        {
            get { return _nodeType; }
            set
            {
                _nodeType = value;
                LoadByRefelection();
            }
        }

      
        private void LoadByRefelection()
        {
            var inputs = GenericNode.GetInputSlotInfos(NodeType);
            var outputs = GenericNode.GetOutputSlotInfos(NodeType);
            var sections = GenericNode.GetSections(NodeType);

            foreach (var section in sections)
            {
                var sectionConfig = new NodeConfigSectionBase();
                sectionConfig.Name = section.Value.Name;
                sectionConfig.IsProxy = section.Value is ProxySection;
                sectionConfig.Visibility = section.Value.Visibility;
                sectionConfig.ChildType = section.Key.PropertyType.GetGenericParameter();


                var referenceSection = section.Value as ReferenceSection;
                if (referenceSection != null)
                {
                    sectionConfig.AllowDuplicates = referenceSection.AllowDuplicates;
                    sectionConfig.AllowAdding = !referenceSection.Automatic;
                    sectionConfig.ReferenceType = referenceSection.ReferenceType ??
                                                  sectionConfig.ChildType.GetGenericParameter() ?? section.Key.PropertyType.GetGenericParameter();
                    
                    if (sectionConfig.ReferenceType == null)
                    {
                        throw new Exception(string.Format("Reference Section on property {0} doesn't have a valid ReferenceType.",section.Key.Name));
                    }

                    //sectionConfig.GenericSelector = (node) =>
                    //{

                    //};
                }
                if (sectionConfig.IsProxy)
                {
                    
                    KeyValuePair<PropertyInfo, Section> section1 = section;
                    sectionConfig.GenericSelector = (node) =>
                    {
                        var enumerator = section1.Key.GetValue(node, null) as IEnumerable;
                        if (enumerator == null) return null;
                        return enumerator.Cast<IGraphItem>();
                        
                    };
                }
                else if (referenceSection != null)
                {
                    KeyValuePair<PropertyInfo, Section> section1 = section;

                    var possibleSelectorProperty = section1.Key.DeclaringType.GetProperty("Possible" + section1.Key.Name);
                    if (possibleSelectorProperty != null)
                    {
                        sectionConfig.GenericSelector = (node) =>
                        {
                            var enumerator = possibleSelectorProperty.GetValue(node, null) as IEnumerable;
                            if (enumerator == null) return null;
                            return enumerator.Cast<IGraphItem>();

                        };
                    }
                    else
                    {
                        sectionConfig.GenericSelector = (node) =>
                        {
                            return node.Project.AllGraphItems.Where(p=>referenceSection.ReferenceType.IsAssignableFrom(p.GetType()));
                        };
                        
                    }
                    
                }
                Sections.Add(sectionConfig);
            }

            foreach (var item in inputs)
            {
               
                var config = new NodeInputConfig()
                {
                    Name = new ConfigProperty<IDiagramNodeItem, string>(item.Value.Name),
                    AllowMultiple = typeof(IMultiSlot).IsAssignableFrom(item.Key.PropertyType) || item.Value.AllowMultiple,
                    IsInput = true,
                    IsOutput = false,
                    Visibility = item.Value.Visibility,
                    ReferenceType = item.Key.PropertyType,
                    SourceType = item.Value.SourceType ?? item.Key.PropertyType.GetGenericParameter(),
                    PropertyInfo = item.Key,
                    AttributeInfo = item.Value,
                };
                
                Inputs.Add(config);
            }
            foreach (var item in outputs)
            {
                var config = new NodeInputConfig()
                {
                    Name = new ConfigProperty<IDiagramNodeItem, string>(item.Value.Name),
                    AllowMultiple = typeof(IMultiSlot).IsAssignableFrom(item.Key.PropertyType) || item.Value.AllowMultiple,
                    IsInput = false,
                    IsOutput = true,
                    Visibility = item.Value.Visibility,
                    ReferenceType = item.Key.PropertyType,
                    SourceType = item.Value.SourceType ?? item.Key.PropertyType.GetGenericParameter(),
                    PropertyInfo = item.Key,
                    AttributeInfo = item.Value
                };
                Inputs.Add(config);
            }


        }


        private List<NodeConfigSectionBase> _sections = new List<NodeConfigSectionBase>();
        private string _name;

        private List<NodeInputConfig> _inputs;
        private List<NodeOutputConfig> _outputs;
        private List<Func<GenericNode, Refactorer>> _refactorers;
        private List<string> _tags;
        private Type _nodeType;
        private Dictionary<PropertyInfo, Slot> _slots;

        public List<NodeConfigSectionBase> Sections
        {
            get { return _sections; }
            set { _sections = value; }
        }

        public Dictionary<PropertyInfo, Slot> Slots
        {
            get { return _slots ?? (_slots = new Dictionary<PropertyInfo, Slot>()); }
            set { _slots = value; }
        }

        public IEnumerable<KeyValuePair<PropertyInfo, Slot>> InputSlots
        {
            get { return Slots.Where(p => p.Value is InputSlot); }
        }
        public IEnumerable<KeyValuePair<PropertyInfo, Slot>> OutputSlots
        {
            get { return Slots.Where(p => p.Value is OutputSlot); }
        } 

        public List<NodeInputConfig> Inputs
        {
            get { return _inputs ?? (_inputs = new List<NodeInputConfig>()); }
            set { _inputs = value; }
        }

        //public List<NodeOutputConfig> Outputs
        //{
        //    get { return _outputs ??(_outputs = new List<NodeOutputConfig>()); }
        //    set { _outputs = value; }
        //}

        //public NodeColor Color { get; set; }
        public List<Func<GenericNode, Refactorer>> Refactorers
        {
            get { return _refactorers ?? new List<Func<GenericNode, Refactorer>>(); }
            set { _refactorers = value; }
        }


        public IUFrameContainer Container { get; set; }

        public List<string> Tags
        {
            get { return _tags ?? (_tags = new List<string>()); }
            set { _tags = value; }
        }

        public abstract bool IsValid(GenericNode node);

        

    }

}
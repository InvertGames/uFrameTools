using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ElementNodeViewModel : DiagramNodeViewModel<ElementData>
    {
        public override Type ExportGraphType
        {
            get { return typeof(ExternalElementGraph); }
        }

        public ElementNodeViewModel(ElementData data, DiagramViewModel diagramViewModel)
            : base(data, diagramViewModel)
        {

        }

        public ConnectorViewModel InheritanceOutput { get; set; }
        
        protected override void DataObjectChanged()
        {
            IsLocal = uFrameEditor.CurrentProject.CurrentGraph.NodeItems.Contains(GraphItemObject);
            ContentItems.Clear();
            if (GraphItem == uFrameEditor.CurrentProject.CurrentFilter)
            {
                foreach (var item in GraphItem.AllItems)
                {
                    var vm = GetDataViewModel(item);
                    if (vm == null)
                    {
                        Debug.LogError(string.Format("Couldn't find view-model for {0}", item.GetType()));
                        continue;
                    }
                    ContentItems.Add(vm);
                }
            }
            else
            {
                foreach (var item in GraphItem.Items)
                {
                    var vm = GetDataViewModel(item);
                    if (vm == null)
                    {
                        Debug.LogError(string.Format("Couldn't find view-model for {0}", item.GetType()));
                        continue;
                    }
                    ContentItems.Add(vm);
                }
            }
            
            
        }

        public override bool ShowSubtitle
        {
            get { return !string.IsNullOrEmpty(SubTitle); }
        }

        public override bool AllowCollapsing
        {
            get { return true; }
        }

        public bool IsTemplate
        {
            get { return GraphItem.IsTemplate; }
        }

        //public bool IsMultiInstance
        //{
        //    get { return GraphItem.IsMultiInstance; }
        //}

        public IEnumerable<ITypeDiagramItem> ViewModelItems
        {
            get { return GraphItem.ViewModelItems; }
        }

        public ICollection<ViewModelPropertyData> Properties
        {
            get { return GraphItem.Properties; }
        }
        public ICollection<ViewModelCommandData> Commands
        {
            get { return GraphItem.Commands; }
        }
        public ICollection<ViewModelCollectionData> Collections
        {
            get { return GraphItem.Collections; }
        }

        public void AddProperty()
        {
            var property = new ViewModelPropertyData()
            {
                Node = GraphItem,
                DefaultValue = string.Empty,
                Name = GraphItem.Project.GetUniqueName("String1"),
                RelatedType = typeof(string).Name
            };
            this.GraphItem.Properties.Add(property);
        }

        public void AddCommand()
        {
            var property = new ViewModelCommandData()
            {
                Node = GraphItem,
                Name = uFrameEditor.CurrentProject.GetUniqueName("Command"),
            };

            this.GraphItem.Commands.Add(property);
        }
        public void AddCollection()
        {
            var property = new ViewModelCollectionData()
            {
                Node = GraphItem,
                Name = GraphItem.Project.GetUniqueName("Collection"),
                RelatedType = typeof(string).Name
            };
            this.GraphItem.Collections.Add(property);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;
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
            IsLocal = DiagramViewModel.CurrentRepository.NodeItems.Contains(GraphItemObject);
            ContentItems.Clear();
            if (GraphItem == DiagramViewModel.CurrentRepository.CurrentFilter)
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
                foreach (var item in GraphItem.DisplayedItems)
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

        public IEnumerable<IBindableTypedItem> ViewModelItems
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
            
            property.IsEditing = true;
            this.DiagramViewModel.CurrentRepository.AddItem(property);
        }

        public void AddCommand()
        {
            var property = new ViewModelCommandData()
            {
                Node = GraphItem,
                Name = DiagramViewModel.CurrentRepository.GetUniqueName("Command"),
            };

            this.DiagramViewModel.CurrentRepository.AddItem(property);
        }
        public void AddCollection()
        {
            var property = new ViewModelCollectionData()
            {
                Node = GraphItem,
                Name = GraphItem.Project.GetUniqueName("Collection"),
                RelatedType = typeof(string).Name
            };
            this.DiagramViewModel.CurrentRepository.AddItem(property);
        }

    }
}
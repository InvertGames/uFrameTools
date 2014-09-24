using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Invert.Common;
using Invert.MVVM;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{

    public class DiagramNodeViewModel<TData> : DiagramNodeViewModel where TData : IDiagramNode
    {
        protected DiagramNodeViewModel()
        {
        }

        public DiagramNodeViewModel(TData graphItemObject, DiagramViewModel diagramViewModel)
            : base(graphItemObject, diagramViewModel)
        {
        }
        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();
            ContentItems.Clear();
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

        protected GraphItemViewModel GetDataViewModel(IDiagramNodeItem item)
        {
            var vm = uFrameEditor.Container.ResolveRelation<ViewModel>(item.GetType(), item, this) as GraphItemViewModel;
            return vm;
        }


        public TData GraphItem
        {
            get { return (TData)GraphItemObject; }
        }
    }

    public class DiagramNodeViewModel : GraphItemViewModel
    {
        private bool _isSelected = false;

      
        public IDiagramNode GraphItemObject
        {
            get { return DataObject as IDiagramNode; }
            set { DataObject = value; }
        }
        public DiagramViewModel DiagramViewModel { get; set; }
        public DiagramNodeViewModel(IDiagramNode graphItemObject, DiagramViewModel diagramViewModel)
            : this()
        {
            GraphItemObject = graphItemObject;
            DiagramViewModel = diagramViewModel;

        }

        public virtual Type ExportGraphType
        {
            get { return null; }
        }
        public override ConnectorViewModel InputConnector
        {
            get { return base.InputConnector; }
        }

        protected DiagramNodeViewModel()
        {

        }

        public ModelCollection<GraphItemViewModel> PropertyViewModels { get; set; }

        public override Vector2 Position
        {
            get
            {
                return DiagramViewModel.CurrentRepository.GetItemLocation(GraphItemObject);
                //return GraphItemObject.Location;
            }
            set
            {
                DiagramViewModel.CurrentRepository.SetItemLocation(GraphItemObject,value);
            }
        }

        //public bool Dirty { get; set; }
        public override bool IsSelected
        {
            get
            {
                return base.IsSelected;
            }
            set
            {
                if (value == false)
                    IsEditing = false;
                base.IsSelected = value;
            }
        }

        public float Scale
        {
            get { return ElementDesignerStyles.Scale; }
        }

        public bool IsCollapsed
        {
            get
            {
                if (AllowCollapsing)
                    return GraphItemObject.IsCollapsed;
                return true;

            }
            set { GraphItemObject.IsCollapsed = value; }
        }

        public virtual bool ShowSubtitle { get { return false; } }

        public virtual float HeaderSize
        {
            get
            {
                return 27;
            }
        }

        public virtual bool AllowCollapsing
        {
            get { return true; }
        }

        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();
            IsLocal = uFrameEditor.CurrentProject.CurrentGraph.NodeItems.Contains(GraphItemObject);

        }
        public bool IsLocal { get; set; }
        public bool IsEditing
        {
            get { return GraphItemObject.IsEditing; }
            set
            {
                GraphItemObject.IsEditing = value;
                if (value == false)
                    EndEditing();
            }
        }

        public string FullLabel
        {
            get { return GraphItemObject.FullLabel; }
        }

        public IEnumerable<IDiagramNodeItem> Items
        {
            get { return GraphItemObject.Items; }
        }

        public string SubTitle
        {
            get { return GraphItemObject.SubTitle; }
        }

        public override string Name
        {
            get { return GraphItemObject.Name; }
            set
            {
                GraphItemObject.Name = value;
                OnPropertyChanged("Name");
            }
        }

        public IEnumerable<IDiagramNodeItem> ContainedItems
        {
            get { return GraphItemObject.ContainedItems; }
        }

        public string InfoLabel
        {
            get { return GraphItemObject.InfoLabel; }
        }

        public string Label
        {
            get { return Name; }
        }

        //public bool IsSelected
        //{
        //    get { return _isSelected; }
        //    set
        //    {
        //        SetProperty(ref _isSelected, value, IsSelectedProperty);
        //    }
        //}
        public virtual Type CommandsType
        {
            get { return typeof(IDiagramNode); }
        }

        public override void Select()
        {
            if (DiagramViewModel.SelectedGraphItems.Count() < 2)
                DiagramViewModel.DeselectAll();
            base.Select();

        }

        public void Rename(string newText)
        {
            GraphItemObject.Rename(newText);
        }

        public void EndEditing()
        {
            GraphItemObject.EndEditing();
            Dirty = true;
        }

        public bool Dirty { get; set; }

        public bool IsFilter
        {
            get { return uFrameEditor.IsFilter(GraphItemObject.GetType()) && IsLocal; }
        }

        public IEnumerable<CodeGenerator> CodeGenerators
        {
            get
            {
                return DiagramViewModel.CodeGenerators.Where(p => p.ObjectData == DataObject);
            }
        }

        public bool HasFilterItems
        {
            get
            {
                var filter = GraphItemObject as IDiagramFilter;
                if (filter == null)
                {
                    return false;
                }
                return filter.GetContainingNodes(DiagramViewModel.CurrentRepository).Any();
            }
        }



        public void BeginEditing()
        {
            GraphItemObject.BeginEditing();
        }

        public void Remove()
        {
            GraphItemObject.RemoveFromDiagram();
        }

        public void Hide()
        {
            DiagramViewModel.CurrentRepository.HideNode(GraphItemObject.Identifier);
            //DiagramViewModel.Data.CurrentFilter.Locations.Remove(GraphItemObject.Identifier);
        }

       
    }
}
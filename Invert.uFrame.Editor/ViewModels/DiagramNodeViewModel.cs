using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invert.Common;
using Invert.MVVM;
using Invert.uFrame.Editor.Connections;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ViewModel<TData> : ViewModel
    {
        public TData Data { get; set; }
    }

    public abstract class GraphItemViewModel : ViewModel
    {
        public abstract Vector2 Position { get; set; }

        public Rect Bounds
        {
            get { return _bounds; }
            set
            {
                _bounds = value;
                ConnectorBounds = value;
            }
        }

        private bool _isSelected = false;
        private List<ConnectorViewModel> _connectors;
        private ModelCollection<GraphItemViewModel> _contentItems;

        public const string IsSelectedProperty = "IsSelected";

        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                Debug.Log("IsSelected set to " + value);
                SetProperty(ref _isSelected, value, IsSelectedProperty);
            }
        }

        public virtual void Select()
        {
            IsSelected = true;

        }
        public bool IsMouseOver { get; set; }

        public List<ConnectorViewModel> Connectors
        {
            get { return _connectors ?? (_connectors = new List<ConnectorViewModel>()); }
            set { _connectors = value; }
        }

        public ModelCollection<GraphItemViewModel> ContentItems
        {
            get { return _contentItems ?? (_contentItems = new ModelCollection<GraphItemViewModel>()); }
            set { _contentItems = value; }
        }


        private ConnectorViewModel _inputConnector;
        public virtual ConnectorViewModel InputConnector
        {
            get
            {
                return _inputConnector ?? (_inputConnector = new ConnectorViewModel()
                {
                    DataObject = DataObject,
                    Direction = ConnectorDirection.Input,
                    ConnectorFor = this,
                    Side = ConnectorSide.Left,
                    SidePercentage = 0.5f,
                });
            }
        }
        private ConnectorViewModel _outputConnector;
        private Rect _bounds;

        public virtual ConnectorViewModel OutputConnector
        {
            get
            {
                return _outputConnector ?? (_outputConnector = new ConnectorViewModel()
                {
                    DataObject = DataObject,
                    Direction = ConnectorDirection.Output,
                    ConnectorFor = this,
                    Side = ConnectorSide.Right,
                    SidePercentage = 0.5f,
                });
            }
        }

        /// <summary>
        /// This is the bounds used to calculate the position of connectors.  Since it is a struct
        /// setting Bounds automatically sets this value.  If you need a custom Connector Bounds position
        /// you'll need to set this after Bounds is set.
        /// </summary>
        public Rect ConnectorBounds { get; set; }

        public virtual void GetConnectors(List<ConnectorViewModel> list)
        {
            if (InputConnector != null)
                list.Add(InputConnector);
            if (OutputConnector != null)
                list.Add(OutputConnector);
            foreach (var item in ContentItems)
            {
                item.GetConnectors(list);
            }
        }
    }

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
            foreach (var item in GraphItem.ContainedItems)
            {
                var vm = uFrameEditor.Container.ResolveRelation<ViewModel>(item.GetType(), item, this) as GraphItemViewModel;
                if (vm == null)
                {
                    Debug.LogError(string.Format("Couldn't find view-model for {0}", item.GetType()));
                    continue;
                }
                ContentItems.Add(vm);
            }
        }


        public TData GraphItem
        {
            get { return (TData)GraphItemObject; }
        }
    }

    public class ItemViewModel : GraphItemViewModel
    {
        public ItemViewModel(DiagramNodeViewModel nodeViewModel)
        {
            NodeViewModel = nodeViewModel;
        }
        public DiagramNodeViewModel NodeViewModel { get; set; }
        public IDiagramNodeItem NodeItem
        {
            get { return (IDiagramNodeItem)DataObject; }
        }

        public virtual string Name
        {
            get { return NodeItem.Name; }
            set { NodeItem.Name = value; }
        }

        public override Vector2 Position { get; set; }

        public bool IsEditing
        {
            get { return NodeItem.IsEditing; }
            set
            {
                NodeItem.IsEditing = value;
                Debug.Log("Is Editing changed to " + value);
            }
        }

        public override ConnectorViewModel InputConnector
        {
            get
            {
          
                return base.InputConnector;
            }
        }

        public override ConnectorViewModel OutputConnector
        {
            get
            {
                 return base.OutputConnector;
            }
        }

        public bool IsSelectable
        {
            get { return true; }
        }

        public IEditorCommand RemoveItemCommand { get; set; }
        public string Highlighter { get; set; }

        public void Rename(string newName)
        {
            NodeItem.Rename(NodeItem.Node, newName);
        }
        public void Remove()
        {
            NodeItem.Remove(((IDiagramNodeItem)DataObject).Node);
        }

        public override void Select()
        {
            Debug.Log("Selected invoked");
            foreach (var item in NodeViewModel.ContentItems)
            {
                item.IsSelected = false;
            }
            IsSelected = true;
            IsEditing = true;
        }
    }

    public class ElementItemViewModel : ItemViewModel<IViewModelItem>
    {

        public ElementItemViewModel(IViewModelItem viewModelItem, DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {
            DataObject = viewModelItem;
        }
        
        public string RelatedType
        {
            get
            {
                return ElementDataBase.TypeAlias(Data.RelatedType);
            }
            set
            {
                Data.RelatedType = value;
            }
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

        public override ConnectorViewModel InputConnector
        {
            get
            {
                if (this.IsLocal)
                    return base.InputConnector;
                return null;
            }
        }

        protected DiagramNodeViewModel()
        {

        }

        public ModelCollection<GraphItemViewModel> PropertyViewModels { get; set; }

        public override Vector2 Position
        {
            get { return GraphItemObject.Location; }
            set
            {


                GraphItemObject.Location = value;
                DiagramViewModel.MarkDirty();
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
            IsLocal = uFrameEditor.CurrentProject.CurrentDiagram.NodeItems.Contains(GraphItemObject);

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

        public string Name
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
            get { return DataObject is IDiagramFilter; }
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
                if (filter == null) return false;

                return filter.Locations.Keys.Count > 1;
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
            DiagramViewModel.Data.CurrentFilter.Locations.Remove(GraphItemObject.Identifier);
        }
    }
}
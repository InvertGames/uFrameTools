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

    public abstract class GraphItemViewModel<TData> : GraphItemViewModel
    {
        public TData Data
        {
            get { return (TData)DataObject; }
        }
    }
    public abstract class GraphItemViewModel : ViewModel
    {
        public abstract Vector2 Position { get; set; }
        public Rect Bounds { get; set; }
        private bool _isSelected = false;
        private List<ConnectorViewModel> _connectors;

        public const string IsSelectedProperty = "IsSelected";

        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetProperty(ref _isSelected, value, IsSelectedProperty);
            }
        }

        public bool IsMouseOver { get; set; }

        public List<ConnectorViewModel> Connectors
        {
            get { return _connectors ?? (_connectors = new List<ConnectorViewModel>()); }
            set { _connectors = value; }
        }

        //public void GetConnectors(List<ConnectorViewModel> list)
        //{
        //    var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        //    foreach (var property in properties)
        //    {
        //        if (typeof (ConnectorViewModel).IsAssignableFrom(property.PropertyType))
        //        {
        //            var value = property.GetValue(this, null) as ConnectorViewModel;
        //            if (value == null) continue;
        //            list.Add(value);
        //        }
        //    }
        //}
    }
    public class DiagramNodeViewModel<TData> : DiagramNodeViewModel where TData : IDiagramNode
    {
        protected DiagramNodeViewModel()
        {
        }

        public DiagramNodeViewModel(TData graphItemObject, DiagramViewModel diagramViewModel) : base(graphItemObject,diagramViewModel)
        {
        }

        public TData GraphItem
        {
            get { return (TData)GraphItemObject; }
        }
    }

    public class ConnectorViewModel : GraphItemViewModel
    {

        public ConnectorViewModel(IConnectionStrategy strategy)
        {
            Strategy = strategy;
        }

        public IConnectionStrategy Strategy { get; set; }

        public override Vector2 Position { get; set; }

        public Action<ConnectionViewModel> ApplyConnection { get; set; }

        public ConnectorDirection Direction { get; set; }
        
        public GraphItemViewModel ConnectorFor { get; set; }


        public ConnectorSide Side { get; set; }

        /// <summary>
        /// A percentage value from 0-1f on which to calculate the position
        /// </summary>
        public float SidePercentage { get; set; }
    }

    public enum ConnectorSide
    {
        Left,
        Right,
        Top,
        Bottom,
    }
    public enum ConnectorDirection
    {
        Input,
        Output,
        TwoWay
    }
    public interface IConnectionStrategy
    {
        /// <summary>
        /// Gets the connectors for a given graph item.
        /// </summary>
        /// <param name="list">The list this method should append the connectors to.</param>
        /// <param name="graphItem"></param>
        void GetConnectors(List<ConnectorViewModel> list, GraphItemViewModel graphItem);

        /// <summary>
        /// Try and connect a to b.  If it can't connect return null
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The connection created if any. Null if no connection can be made</returns>
        ConnectionViewModel Connect(ConnectorViewModel a, ConnectorViewModel b);

        /// <summary>
        /// Iterate through connectors and find decorate the connections list with any found connections.
        /// </summary>
        /// <param name="connections"></param>
        /// <param name="info"></param>
        void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info);
    }

    public abstract class DefaultConnectionStrategy : IConnectionStrategy
    {
        public virtual void GetConnectors(List<ConnectorViewModel> list, GraphItemViewModel graphItem)
        {
           
        }

        public virtual ConnectionViewModel Connect(ConnectorViewModel a, ConnectorViewModel b)
        {
            return null;
        }

        public virtual void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info)
        {
            
        }
    }

    public class ElementInheritanceConnectionStrategy : DefaultConnectionStrategy
    {
        public override void GetConnectors(List<ConnectorViewModel> list, GraphItemViewModel graphItem)
        {
            base.GetConnectors(list, graphItem);
            var elementViewModel = graphItem as ElementNodeViewModel;
            if (elementViewModel != null)
            {
                list.Add(new ConnectorViewModel(this)
                {
                    DataObject = elementViewModel.GraphItem,
                    Direction = ConnectorDirection.Input,
                    ConnectorFor = elementViewModel,
                    Side = ConnectorSide.Left,
                    SidePercentage = 0.5f,
                });

                list.Add(new ConnectorViewModel(this)
                {
                    Side = ConnectorSide.Right,
                    SidePercentage = 0.5f,
                    DataObject = elementViewModel.GraphItem,
                    Direction = ConnectorDirection.Output,
                    ConnectorFor = elementViewModel
                });
            }
        }

        public override ConnectionViewModel Connect(ConnectorViewModel a, ConnectorViewModel b)
        {
            if (a.ConnectorFor is ElementNodeViewModel && b.ConnectorFor is ElementNodeViewModel)
            {
                if (a.Direction == ConnectorDirection.Output && b.Direction == ConnectorDirection.Input)
                {
                    return new ConnectionViewModel()
                    {
                        ConnectorA = a,
                        ConnectorB = b,
                        Apply = Apply
                    };
                }
                
            }
            return null;
        }

        public override void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info)
        {
            base.GetConnections(connections, info);

            connections.AddRange(
                info.ConnectionsByData<ElementData, ElementData>(
                    Color.green
                    , (o, i) => i.BaseTypeShortName == o.Name
                    , Remove
                    )
                );
           
        }

        private void Apply(ConnectionViewModel connectionViewModel)
        {
            var baseElement = connectionViewModel.ConnectorA.DataObject as ElementData;
            var derivedEelement = connectionViewModel.ConnectorB.DataObject as ElementData;
            if (baseElement != null)
                baseElement.CreateLink(baseElement,derivedEelement);
        }

        private void Remove(ConnectionViewModel connectionViewModel)
        {
            var baseElement = connectionViewModel.ConnectorA.DataObject as ElementData;
            var derivedEelement = connectionViewModel.ConnectorB.DataObject as ElementData;
            if (baseElement != null)
                baseElement.CreateLink(baseElement, derivedEelement);
        }
    }

    public class ConnectorInfo
    {
        private ConnectorViewModel[] _inputs;
        private ConnectorViewModel[] _outputs;

        public ConnectorInfo(ConnectorViewModel[] allConnectors)
        {
            AllConnectors = allConnectors;
        }

        public ConnectorViewModel[] AllConnectors
        {
            get; set;
        }

        public ConnectorViewModel[] Inputs
        {
            get { return _inputs ?? (_inputs = AllConnectors.Where(p=>p.Direction == ConnectorDirection.Input).ToArray()); }
          
        }
        public ConnectorViewModel[] Outputs
        {
            get { return _outputs ?? (_outputs = AllConnectors.Where(p => p.Direction == ConnectorDirection.Output).ToArray()); }
        }

        public IEnumerable<ConnectorViewModel> InputsWith<TData>()
        {
            return Inputs.Where(p => p.DataObject is TData);
        }
        public IEnumerable<ConnectorViewModel> OutputsWith<TData>()
        {
            return Outputs.Where(p => p.DataObject is TData);
        }

        public IEnumerable<ConnectionViewModel> ConnectionsByData<TSource, TTarget>(Color color, Func<TSource, TTarget, bool> isConnected, Action<ConnectionViewModel> remove, Action<ConnectionViewModel> apply = null)
        {
            foreach (var output in OutputsWith<TSource>())
            {
                foreach (var input in InputsWith<TTarget>())
                {
                    if (isConnected((TSource)output.DataObject,(TTarget)input.DataObject))
                    {
                        yield return new ConnectionViewModel()
                        {
                            Color = color,
                            ConnectorA = output,
                            ConnectorB = input,
                            Remove = remove,
                            Apply = apply
                        };
                    }
                }
            }
        }
    }

    public class ConnectionViewModel : GraphItemViewModel
    {
        public ConnectorViewModel ConnectorA { get; set; }
        public ConnectorViewModel ConnectorB { get; set; }



        public Action<ConnectionViewModel> Apply { get; set; }
        public Action<ConnectionViewModel> Remove { get; set; }

        public override Vector2 Position { get; set; }
        public Color Color { get; set; }
    }

    public class ItemViewModel<TData> : ItemViewModel
    {
        public TData Data
        {
            get { return (TData)DataObject; }
            set { DataObject = value; }
        }
    }

    public class ItemViewModel : GraphItemViewModel
    {
        public IDiagramNodeItem NodeItem
        {
            get { return (IDiagramNodeItem) DataObject; }
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
            set { NodeItem.IsEditing = value; }
        }

        public bool IsSelectable
        {
            get { return true; }
        }

        public IEditorCommand RemoveItemCommand { get; set; }
        public string Highlighter { get; set; }

        public void Rename(string newName)
        {
            NodeItem.Rename(NodeItem.Node,newName);
        }
        public void Remove()
        {
            NodeItem.Remove(((IDiagramNodeItem)DataObject).Node);
        }
    }

    public class ElementItemViewModel : ItemViewModel<IViewModelItem>
    {
        public ElementItemViewModel(IViewModelItem viewModelItem)
        {
            DataObject = viewModelItem;
        }

        public string RelatedType
        {
            get
            {
                return Data.RelatedType;
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
        private ModelCollection<GraphItemViewModel> _contentItems;


        public IDiagramNode GraphItemObject
        {
            get { return DataObject as IDiagramNode; }
            set { DataObject = value; }
        }
        public DiagramViewModel DiagramViewModel { get; set; }
        public DiagramNodeViewModel(IDiagramNode graphItemObject, DiagramViewModel diagramViewModel) : this()
        {
            GraphItemObject = graphItemObject;
            DiagramViewModel = diagramViewModel;

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
            set { 
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

        public ModelCollection<GraphItemViewModel> ContentItems
        {
            get { return _contentItems ?? (_contentItems = new ModelCollection<GraphItemViewModel>()); }
            set { _contentItems = value; }
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
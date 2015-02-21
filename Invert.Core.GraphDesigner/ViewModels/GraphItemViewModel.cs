using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{ 
    public abstract class GraphItemViewModel<TData> : GraphItemViewModel
    {
        public TData Data
        {
            get { return (TData)DataObject; }
        }
    }

    public abstract class GraphItemViewModel : ViewModel
    {
        public string Comments
        {
            get { return "TODO"; }
            set
            {
                
            }
        }
        public abstract Vector2 Position { get; set; }
        public abstract string Name { get; set; }

        public DiagramViewModel DiagramViewModel { get; set; }
        public Rect Bounds
        {
            get { return _bounds; }
            set
            {
                _bounds = value;
                
            }
        }

        private bool _isSelected = false;
        private List<ConnectorViewModel> _connectors;
        private ModelCollection<GraphItemViewModel> _contentItems;

        public const string IsSelectedProperty = "IsSelected";

        public virtual bool IsSelected
        {
            get
            {
                
                return _isSelected;
            }
            set
            {
                if (value == false)
                foreach (var item in ContentItems)
                {
                    item.IsSelected = false;
                }
                _isSelected = value;
                //SetProperty(ref _isSelected, value, IsSelectedProperty);
                OnPropertyChanged("IsSelected");
            }
        }

        public virtual void Select()
        {
            
            IsSelected = true;

        }

        public override string ToString()
        {
            return GetHashCode().ToString();
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                OnPropertyChanged("IsDirty");
            }
        }

        public bool IsMouseOver
        {
            get { return _isMouseOver; }
            set
            {
                _isMouseOver = value;
                OnPropertyChanged("IsMouseOver");
            }
        }

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

        public virtual Type InputConnectorType { get; set; }
        public virtual Type OutputConnectorType { get; set; }
        
        private ConnectorViewModel _inputConnector;
        public virtual ConnectorViewModel InputConnector
        {
            get
            {
                if (DataObject == null) return null;
                return _inputConnector ?? (_inputConnector = CreateInputConnector());
            }
            set { throw new NotImplementedException(); }
        }

        protected virtual ConnectorViewModel CreateInputConnector()
        {
            return new ConnectorViewModel()
            {
                DataObject = DataObject,
                Direction = ConnectorDirection.Input,
                ConnectorFor = this,
                DiagramViewModel = DiagramViewModel,
                ConnectorForType = InputConnectorType ?? DataObject.GetType(),
                Side = ConnectorSide.Left,
                SidePercentage = 0.5f,
                Validator = InputValidator
            };
        }

        private ConnectorViewModel _outputConnector;
        private Rect _bounds;
        private Rect _connectorBounds;
        private Func<IDiagramNodeItem, IDiagramNodeItem, bool> _inputValidator;
        private Func<IDiagramNodeItem, IDiagramNodeItem, bool> _outputValidator;
        private bool _isMouseOver;
        private bool _showHelp;
        private bool _isDirty;

        public virtual ConnectorViewModel OutputConnector
        {
            get
            {
                if (DataObject == null) return null;
                return _outputConnector ?? (_outputConnector = CreateOutputConnector());
            }
            set { throw new NotImplementedException(); }
        }

        protected virtual ConnectorViewModel CreateOutputConnector()
        {
            return new ConnectorViewModel()
            {
                DataObject = DataObject,
                Direction = ConnectorDirection.Output,
                ConnectorFor = this,
                DiagramViewModel = DiagramViewModel,
                ConnectorForType = OutputConnectorType ?? DataObject.GetType(),
                Side = ConnectorSide.Right,
                SidePercentage = 0.5f,
                Validator = OutputValidator
            };
        }

        /// <summary>
        /// This is the bounds used to calculate the position of connectors.  Since it is a struct
        /// setting Bounds automatically sets this value.  If you need a custom Connector Bounds position
        /// you'll need to set this after Bounds is set.
        /// </summary>
        public Rect ConnectorBounds
        {
            get
            {
                if (_connectorBounds.width < 1f)
                {
                    return Bounds;
                }
                return _connectorBounds;
            }
            set { _connectorBounds = value; }
        }

        public virtual  Func<IDiagramNodeItem, IDiagramNodeItem, bool> InputValidator
        {
            get { return _inputValidator; }
            set { _inputValidator = value; }
        }

        public virtual Func<IDiagramNodeItem, IDiagramNodeItem, bool> OutputValidator
        {
            get { return _outputValidator; }
            set { _outputValidator = value; }
        }

        public bool ShowHelp
        {
            get
            {
                // TODO return _ShowHelp instead
                return IsScreenshot ||  _showHelp;
            }
            set { _showHelp = value; }
        }

        public bool IsScreenshot { get; set; }

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

}
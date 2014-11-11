using System.Collections.Generic;
using System.Collections.ObjectModel;
using Invert.MVVM;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
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
        public abstract Vector2 Position { get; set; }
        public abstract string Name { get; set; }

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
            get { return _isSelected; }
            set
            {
                if (value == false)
                foreach (var item in ContentItems)
                {
                    item.IsSelected = false;
                }
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
        private Rect _connectorBounds;

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
using System;
using System.Linq;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class ItemViewModel<TData> : ItemViewModel
        where TData : IDiagramNodeItem
    {
        public ItemViewModel(DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {

        }
        public ItemViewModel(TData data, DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {
            Data = data;
        }

        public TData Data
        {
            get { return (TData)DataObject; }
            set { DataObject = value; }
        }

        public override string Label
        {
            get { return Data.Label; }
        }
    }

    public class ItemViewModel : GraphItemViewModel
    {
        private IEditorCommand _removeItemCommand;
        private bool _isEditable = true;

        public ItemViewModel(DiagramNodeViewModel nodeViewModel)
        {
            NodeViewModel = nodeViewModel;
        }
        
        public DiagramNodeViewModel NodeViewModel { get; set; }
        public IDiagramNodeItem NodeItem
        {
            get { return (IDiagramNodeItem)DataObject; }
        }

        public override string Name
        {
            get { return NodeItem.Name; }
            set { NodeItem.Name = value; }
        }

        public override Vector2 Position { get; set; }

        public virtual bool IsEditing
        {
            get { return NodeItem.IsEditing; }
            set
            {
                NodeItem.IsEditing = value;
            }
        }

        //public override Func<IDiagramNodeItem, IDiagramNodeItem, bool> InputValidator
        //{
        //    get
        //    {
        //        var item = DataObject as IDiagramNodeItem;
        //        item.ValidateInput;
        //    }
        //}

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

        public virtual bool IsEditable
        {
            get { return _isEditable; }
            set { _isEditable = value; }
        }

        public IEditorCommand RemoveItemCommand
        {
            get { return _removeItemCommand ?? (_removeItemCommand = new RemoveNodeItemCommand()); }
            set { _removeItemCommand = value; }
        }

        public string Highlighter { get; set; }

        public virtual bool AllowRemoving
        {
            get { return true; }
        }

        public virtual string Label
        {
            get { return Name; }
        }

        public void Rename(string newName)
        {
            NodeItem.Rename(NodeItem.Node, newName);
        }

        public override bool IsSelected
        {
            get
            {
                return base.IsSelected;
            }
            set
            {
                if (!value)
                {
                        IsEditing = false;
                    
                }
                base.IsSelected = value;
            }
        }

        public override void Select()
        {
            NodeViewModel.Select();
            var items = NodeViewModel.DiagramViewModel.SelectedNodeItems.ToArray();
            foreach (var item in items)
                item.IsSelected = false;
#if UNITY_DLL
            GUIUtility.keyboardControl = 0;
#endif
         
            IsSelected = true;
            IsEditing = true;
        }
    }
}
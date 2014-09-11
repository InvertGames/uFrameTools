using Invert.uFrame.Editor.ElementDesigner;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ItemViewModel<TData> : ItemViewModel
    {
        public ItemViewModel(DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {

        }

        public TData Data
        {
            get { return (TData)DataObject; }
            set { DataObject = value; }
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

        public override string Name
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

        public bool IsEditable
        {
            get { return true; }
        }

        public IEditorCommand RemoveItemCommand { get; set; }
        public string Highlighter { get; set; }

        public virtual bool AllowRemoving
        {
            get { return true; }
        }

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
            NodeViewModel.Select();
            foreach (var item in NodeViewModel.ContentItems)
            {
                item.IsSelected = false;
            }
            IsSelected = true;
            IsEditing = true;
        }
    }

    public class BindingViewModel : ItemViewModel
    {
        public BindingViewModel(DiagramNodeViewModel nodeViewModel) : base(nodeViewModel)
        {
        }
    }
}
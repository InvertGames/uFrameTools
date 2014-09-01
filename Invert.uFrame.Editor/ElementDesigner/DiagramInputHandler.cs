//using System;
//using System.Linq;
//using Invert.Common;
//using Invert.uFrame.Editor;
//using Invert.uFrame.Editor.ViewModels;
//using UnityEngine;

//public class DiagramInputHandler
//{

//    public DiagramInputHandler(UFDiagramViewModel diagramViewModel)
//    {
//        DiagramViewModel = diagramViewModel;
//    }

//    public UFDiagramViewModel DiagramViewModel { get; set; }
//    public IDiagramNode CurrentMouseOverNode { get; set; }

//    public Vector2 DragDelta
//    {
//        get
//        {
//            var mp = CurrentMousePosition;

//            var v = new Vector2(Mathf.Round(mp.x / DiagramViewModel.SnapSize) *DiagramViewModel.SnapSize, Mathf.Round(mp.y /DiagramViewModel.SnapSize) *DiagramViewModel.SnapSize);
//            var v2 = new Vector2(Mathf.Round(LastDragPosition.x /DiagramViewModel.SnapSize) *DiagramViewModel.SnapSize, Mathf.Round(LastDragPosition.y /DiagramViewModel.SnapSize) *DiagramViewModel.SnapSize);
//            return (v - v2);
//        }
//    }

//    public bool IsMouseDown { get; set; }
//    public Vector2 LastDragPosition { get; set; }
//    public Vector2 LastMouseDownPosition { get; set; }
//    public Vector2 LastMouseUpPosition { get; set; }

//    public INodeDrawer MouseOverViewData
//    {
//        get
//        {
//            return _elementsDiagram.NodeDrawers.FirstOrDefault(p => p.Bounds.Scale(ElementsDiagram.Scale).Contains(CurrentMousePosition));
//            //return Data.DiagramItems.LastOrDefault(p => p.Position.Contains(CurrentMousePosition));
//        }
//    }

//    public Vector2 CurrentMousePosition
//    {
//        get
//        {

//            return _elementsDiagram.CurrentEvent.mousePosition;
//        }
//    }

//    public void HandleInput2()
//    {
//        if (IsMouseDown && _elementsDiagram.UfDiagramViewModel.SelectedData != null && _elementsDiagram.UfDiagramViewModel.SelectedItem == null &&
//            !_elementsDiagram.CurrentEvent.control)
//        {
//            var newPosition = DragDelta; //CurrentMousePosition - SelectionOffset;
//            var allSelected = DiagramViewModel.AllSelected.ToArray();
//            foreach (var diagramItem in allSelected)
//            {
//                diagramItem.Location += (newPosition*(1f/ElementsDiagram.Scale));

//                diagramItem.Location =
//                    new Vector2(
//                        Mathf.Round((diagramItem.Location.x)/_elementsDiagram.UfDiagramViewModel.SnapSize)*_elementsDiagram.UfDiagramViewModel.SnapSize,
//                        Mathf.Round(diagramItem.Location.y/_elementsDiagram.UfDiagramViewModel.SnapSize)*_elementsDiagram.UfDiagramViewModel.SnapSize);
//            }

//            foreach (var viewModelDrawer in _elementsDiagram.NodeDrawers.Where(p => p.IsSelected))
//            {
//                viewModelDrawer.Refresh(Vector3.zero);
//            }
//            _elementsDiagram.DidDrag = true;
//            LastDragPosition = CurrentMousePosition;
//        }
//        else if (IsMouseDown && _elementsDiagram.UfDiagramViewModel.SelectedData != null && _elementsDiagram.CurrentEvent.control)
//        {
//            if (!_elementsDiagram.UfDiagramViewModel.SelectedData.Position.Scale(ElementsDiagram.Scale).Contains(CurrentMousePosition))
//            {
//                if (_elementsDiagram.UfDiagramViewModel.SelectedItem == null)
//                {
//                    var mouseOver = MouseOverViewData;
//                    var canCreateLink = _elementsDiagram.UfDiagramViewModel.SelectedData.CanCreateLink(mouseOver == null
//                        ? null
//                        : (mouseOver.ViewModelObject as DiagramNodeViewModel).GraphItemObject);

//                    CurrentMouseOverNode = mouseOver == null
//                        ? null
//                        : (mouseOver.ViewModelObject as DiagramNodeViewModel).GraphItemObject;
//                    var beizureLink = new AssociationLink();
//                    beizureLink.DrawBeizure(_elementsDiagram.UfDiagramViewModel.SelectedData.Position.Scale(ElementDesignerStyles.Scale),
//                        mouseOver != null && canCreateLink
//                            ? CurrentMouseOverNode.Position.Scale(ElementDesignerStyles.Scale)
//                            : new Rect(CurrentMousePosition.x, CurrentMousePosition.y, 4, 4), Color.yellow, 6);
//                    beizureLink.DrawPoints(_elementsDiagram);
//                }
//                else
//                {
//                    try
//                    {
//                        var mouseOver = MouseOverViewData;
//                        var canCreateLink = _elementsDiagram.UfDiagramViewModel.SelectedItem.CanCreateLink(mouseOver == null
//                            ? null
//                            : (mouseOver.ViewModelObject as DiagramNodeViewModel).GraphItemObject);

//                        CurrentMouseOverNode = mouseOver == null
//                            ? null
//                            : (mouseOver.ViewModelObject as DiagramNodeViewModel).GraphItemObject;
//                        var beizureLink = new AssociationLink();
//                        beizureLink.DrawBeizure(_elementsDiagram.UfDiagramViewModel.SelectedItem.Position.Scale(ElementDesignerStyles.Scale),
//                            CurrentMouseOverNode != null && canCreateLink
//                                ? CurrentMouseOverNode.Position.Scale(ElementDesignerStyles.Scale)
//                                : new Rect(CurrentMousePosition.x, CurrentMousePosition.y, 4, 4),
//                            Color.green, 6);
//                        beizureLink.DrawPoints(_elementsDiagram);
//                    }
//                    catch (Exception ex)
//                    {
//                        Debug.Log(ex.Message);
//                    }
//                }
//            }
//            else
//            {
//                CurrentMouseOverNode = null;
//            }
//        }
//        else if (IsMouseDown)
//        {
//            var cur = CurrentMousePosition;
//            if (cur.x > LastMouseDownPosition.x)
//            {
//                if (cur.y > LastMouseDownPosition.y)
//                {
//                    _elementsDiagram.UfDiagramViewModel.SelectionRect = new Rect(LastMouseDownPosition.x, LastMouseDownPosition.y,
//                        cur.x - LastMouseDownPosition.x, cur.y - LastMouseDownPosition.y);
//                }
//                else
//                {
//                    _elementsDiagram.UfDiagramViewModel.SelectionRect = new Rect(
//                        LastMouseDownPosition.x, cur.y, cur.x - LastMouseDownPosition.x, LastMouseDownPosition.y - cur.y);
//                    //SelectionRect = new Rect(LastMouseDownPosition.x, LastMouseDownPosition.y,
//                    //    cur.x - LastMouseDownPosition.x, cur.y - LastMouseDownPosition.y);
//                }
//            }
//            else
//            {
//                if (cur.y > LastMouseDownPosition.y)
//                {
//                    // x is less and y is greater
//                    _elementsDiagram.UfDiagramViewModel.SelectionRect = new Rect(
//                        cur.x, LastMouseDownPosition.y, LastMouseDownPosition.x - cur.x, cur.y - LastMouseDownPosition.y);
//                }
//                else
//                {
//                    // both are less
//                    _elementsDiagram.UfDiagramViewModel.SelectionRect = new Rect(
//                        cur.x, cur.y, LastMouseDownPosition.x - cur.x, LastMouseDownPosition.y - cur.y);
//                }
//                //SelectionRect = new Rect(LastMouseDownPosition.x, LastMouseDownPosition.y,
//                //   LastMouseDownPosition.x - cur.x, LastMouseDownPosition.y - cur.y);
//            }
//        }
//        else
//        {
//            _elementsDiagram.UfDiagramViewModel.SelectionRect = new Rect();
//            CurrentMouseOverNode = null;
//        }
//    }

//    private void OnDoubleClick()
//    {

//        if (_elementsDiagram.UfDiagramViewModel.SelectedData != null)
//        {
//            if (_elementsDiagram.UfDiagramViewModel.SelectedItem == null)
//            {
//                if (_elementsDiagram.UfDiagramViewModel.SelectedData is IDiagramFilter)
//                {
//                    if (_elementsDiagram.UfDiagramViewModel.SelectedData == _elementsDiagram.Data.CurrentFilter)
//                    {
//                        _elementsDiagram.Data.PopFilter(null);
//                    }
//                    else
//                    {
//                        _elementsDiagram.Data.PushFilter(_elementsDiagram.UfDiagramViewModel.SelectedData as IDiagramFilter);
//                    }

//                    _elementsDiagram.Refresh(true);
//                    _elementsDiagram.Refresh(true);
//                }
//                else
//                {
//                    _elementsDiagram.UfDiagramViewModel.Selected.DoubleClicked();
//                }
//            }

//        }

//    }

//    private void OnMouseDown()
//    {
//        var selected = MouseOverViewData;
//        if (selected != null)
//        {
//            if (DiagramViewModel.Selected != null)
//            {
//                if (CurrentEvent.shift)
//                {
//                }
//                else
//                {
//                    if (DiagramViewModel.AllSelected.All(p => p != (selected.ViewModelObject as DiagramNodeViewModel).GraphItemObject))
//                    {
//                        _elementsDiagram.DeselectAll();
//                    }
//                }
//            }
//            if (Event.current.button != 2)
//            {
//                DiagramViewModel.SelectionOffset = LastMouseDownPosition - new Vector2(selected.ViewModelObject.Position.x, selected.ViewModelObject.Position.y);
//                LastDragPosition = LastMouseDownPosition;
//            }

//        }
//        else
//        {
//            //if (AllSelected.All(p => p != SelectedData))
//            //{
//            foreach (var diagramItem in DiagramViewModel.AllSelected)
//            {
//                if (diagramItem.IsEditing)
//                {
//                    diagramItem.EndEditing();

//                }
//                diagramItem.IsSelected = false;
//            }
//            //}
//        }

//        _elementsDiagram.UfDiagramViewModel.Selected = selected;
//    }
//    public void HandleInput()
//    {

//        var e = Event.current;

//        if (e.type == EventType.MouseDown && e.button != 2)
//        {
//            CurrentEvent = Event.current;
//            LastMouseDownPosition = e.mousePosition;
//            IsMouseDown = true;
//            OnMouseDown();
//            if (e.clickCount > 1)
//            {
//                OnDoubleClick();
//            }
//            e.Use();
//        }
//        if (CurrentEvent.rawType == EventType.MouseUp && IsMouseDown)
//        {
//            LastMouseUpPosition = e.mousePosition;
//            IsMouseDown = false;
//            OnMouseUp();
//            e.Use();
//        }
//        if (_elementsDiagram.CurrentEvent.keyCode == KeyCode.Return)
//        {
//            if (DiagramViewModel.SelectedData != null && DiagramViewModel.SelectedData.IsEditing)
//            {
//                DiagramViewModel.SelectedData.EndEditing();
//                //e.Use();
//                _elementsDiagram.Dirty = true;
//            }
//        }
//        if (_elementsDiagram.CurrentEvent.keyCode == KeyCode.F2)
//        {
//            if (DiagramViewModel.SelectedData != null)
//            {
//                DiagramViewModel.SelectedData.BeginEditing();
//                e.Use();
//            }
//        }
//    }

//    public Event CurrentEvent { get; set; }

//    private void OnMouseUp()
//    {
//        if (CurrentEvent.button == 1)
//        {
//            if (DiagramViewModel.SelectedItem != null)
//            {
//                _elementsDiagram.ShowItemContextMenu(_elementsDiagram.UfDiagramViewModel.SelectedItem);
//            }
//            else if (_elementsDiagram.UfDiagramViewModel.SelectedData != null)
//            {
//                _elementsDiagram.ShowContextMenu();
//            }
//            else
//            {
//                _elementsDiagram.ShowAddNewContextMenu(true);
//            }
//            IsMouseDown = false;
//            return;
//        }
//        if (CurrentMouseOverNode != null)
//        {
//            if (DiagramViewModel.SelectedItem != null)
//            {
//                if (DiagramViewModel.SelectedItem.CanCreateLink(CurrentMouseOverNode))
//                {
//                    uFrameEditor.DesignerWindow.Diagram.ExecuteCommand(e => _elementsDiagram.UfDiagramViewModel.SelectedItem.CreateLink(_elementsDiagram.UfDiagramViewModel.SelectedData, CurrentMouseOverNode));
//                }
//            }
//            else
//            {
//                if (DiagramViewModel.SelectedData.CanCreateLink(CurrentMouseOverNode))
//                {
//                    uFrameEditor.DesignerWindow.Diagram.ExecuteCommand(e => _elementsDiagram.UfDiagramViewModel.SelectedData.CreateLink(_elementsDiagram.UfDiagramViewModel.SelectedData, CurrentMouseOverNode));
//                }
//            }
//        }
//        else if (CurrentEvent.control)
//        {
//            if (DiagramViewModel.SelectedItem != null)
//            {
//                uFrameEditor.DesignerWindow.Diagram.ExecuteCommand(e => _elementsDiagram.UfDiagramViewModel.SelectedItem.RemoveLink(_elementsDiagram.UfDiagramViewModel.SelectedData));
//            }
//            else if (_elementsDiagram.UfDiagramViewModel.SelectedData != null)
//            {
//                uFrameEditor.DesignerWindow.Diagram.ExecuteCommand(e => _elementsDiagram.UfDiagramViewModel.SelectedData.RemoveLink(null));
//            }
//        }
      
        
//    }
//}
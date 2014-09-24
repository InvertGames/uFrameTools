using System.Collections.Specialized;
using System.ComponentModel;
using Invert.Common;
using Invert.MVVM;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public interface IInputHandler
{
    void OnMouseDoubleClick(MouseEvent mouseEvent);
    void OnMouseDown(MouseEvent mouseEvent);
    void OnMouseMove(MouseEvent e);
    void OnMouseUp(MouseEvent mouseEvent);
    void OnRightClick(MouseEvent mouseEvent);
}


public class ElementsDiagram : Drawer, ICommandHandler, IInputHandler
{
    public delegate void SelectionChangedEventArgs(IDiagramNode oldData, IDiagramNode newData);

    public delegate void ViewModelDataEventHandler(ElementData data);

    public event SelectionChangedEventArgs SelectionChanged;

    private Event _currentEvent;
    private IDrawer _nodeDrawerAtMouse;
    private SelectionRectHandler _selectionRectHandler;



    public static float Scale
    {
        get { return ElementDesignerStyles.Scale; }
    }

    public IEnumerable<object> ContextObjects
    {
        get
        {
            if (CurrentMouseOverNode != null && !CurrentMouseOverNode.IsSelected)
            {
                CurrentMouseOverNode.IsSelected = true;
            }
            if (DiagramViewModel != null)
            {
                yield return DiagramViewModel;
                foreach (var item in DiagramViewModel.SelectedGraphItems)
                {
                    yield return item;

                }
            }

        }
    }

    public Event CurrentEvent
    {
        get { return Event.current ?? _currentEvent; }
        set { _currentEvent = value; }
    }

    public IDiagramNode CurrentMouseOverNode { get; set; }

    public Vector2 CurrentMousePosition
    {
        get
        {
            return CurrentEvent.mousePosition;
        }
    }

    public Rect DiagramSize
    {
        get
        {
            Rect size = new Rect();
            foreach (var diagramItem in Children)
            {
                var rect = diagramItem.Bounds.Scale(Scale);

                if (rect.x < 0)
                    rect.x = 0;
                if (rect.y < 0)
                    rect.y = 0;
                //if (rect.x < size.x)
                //{
                //    size.x = rect.x;
                //}
                //if (rect.y < size.y)
                //{
                //    size.y = rect.y;
                //}
                if (rect.x + rect.width > size.x + size.width)
                {
                    size.width = rect.x + rect.width;
                }
                if (rect.y + rect.height > size.y + size.height)
                {
                    size.height = rect.y + rect.height;
                }
            }
            size.height += 400f;
            size.width += 400f;
            if (size.height < Screen.height)
            {
                size.height = Screen.height;
            }
            if (size.width < Screen.width)
            {
                size.width = Screen.width;
            }
            return size;
        }
    }

    public DiagramViewModel DiagramViewModel
    {
        get { return this.DataContext as DiagramViewModel; }
        set { this.DataContext = value; }
    }

    public bool DidDrag { get; set; }

    public Vector2 DragDelta
    {
        get
        {
            var mp = CurrentMousePosition;

            var v = new Vector2(Mathf.Round(mp.x / SnapSize) * SnapSize, Mathf.Round(mp.y / SnapSize) * SnapSize);
            var v2 = new Vector2(Mathf.Round(LastDragPosition.x / SnapSize) * SnapSize, Mathf.Round(LastDragPosition.y / SnapSize) * SnapSize);
            return (v - v2);
        }
    }

    public bool IsMouseDown { get; set; }

    public Vector2 LastDragPosition { get; set; }

    public Vector2 LastMouseDownPosition { get; set; }

    public Vector2 LastMouseUpPosition { get; set; }

    public Rect Rect { get; set; }

    public Vector2 SelectionOffset { get; set; }

    public Rect SelectionRect { get; set; }

    public float SnapSize
    {
        get { return DiagramViewModel.Settings.SnapSize * Scale; }
    }

    public ElementsDiagram(DiagramViewModel viewModel)
    {
        DiagramViewModel = viewModel;
    }

    public static Rect CreateSelectionRect(Vector2 start, Vector2 current)
    {
        if (current.x > start.x)
        {
            if (current.y > start.y)
            {
                return new Rect(start.x, start.y,
                    current.x - start.x, current.y - start.y);
            }
            else
            {
                return new Rect(
                    start.x, current.y, current.x - start.x, start.y - current.y);
            }
        }
        else
        {
            if (current.y > start.y)
            {
                // x is less and y is greater
                return new Rect(
                    current.x, start.y, start.x - current.x, current.y - start.y);
            }
            else
            {
                // both are less
                return new Rect(
                    current.x, current.y, start.x - current.x, start.y - current.y);
            }
        }
    }

    public void CommandExecuted(IEditorCommand command)
    {
        //DiagramViewModel.MarkDirty();
        //uFrameEditor.Log(command.Title + " Executed");
        this.Refresh();
        //Dirty = true;
    }

    public void CommandExecuting(IEditorCommand command)
    {
        //DiagramViewModel.RecordUndo(command.Title);
    }

    public override void Draw(float scale)
    {
        // Make sure they've upgraded to the latest json format
        if (UpgradeOldProject()) return;
        // Draw the title box
        GUI.Box(new Rect(0, 0f, Rect.width, 30f), DiagramViewModel.Title, ElementDesignerStyles.DiagramTitle);

       
        // Draw all of our drawers
        foreach (var drawer in Children.OrderBy(p => p.ZOrder).ToArray())
        {
            if (drawer.Dirty)
            {
                drawer.Refresh();
                drawer.Dirty = false;
            }
            drawer.Draw(Scale);
            
        }

        EditorGUI.FocusTextInControl("EditingField");
        //var nodeItem = DiagramViewModel.SelectedNodeItem as ItemViewModel;
        //if (nodeItem != null)
        //{

        //    if (GUI.GetNameOfFocusedControl() == string.Empty)
        //        EditorGUI.FocusTextInControl(nodeItem.NodeItem.Identifier);
        //}
        //if (focusItem != null)
        //{
        //    EditorGUI.FocusTextInControl(focusItem);
        //}

        if (IsMouseDown)
        {
            var items = DiagramViewModel.SelectedGraphItems.ToArray();
            var allSelected = items;

            var newPosition = DragDelta;//CurrentMousePosition - SelectionOffset;

            foreach (var diagramItem in allSelected)
            {
                diagramItem.Position += (newPosition * (1f / Scale));
                diagramItem.Position = new Vector2(Mathf.Round((diagramItem.Position.x) / SnapSize) * SnapSize, Mathf.Round(diagramItem.Position.y / SnapSize) * SnapSize);
            }

            foreach (var viewModelDrawer in Children.Where(p => p.IsSelected))
            {
                viewModelDrawer.Refresh();
            }
            DidDrag = true;
            LastDragPosition = CurrentMousePosition;
        }

        DrawErrors();
        DrawHelp();
    }

    public bool HandleKeyEvent(Event evt, ModifierKeyState keyStates)
    {
        var bindings = uFrameEditor.KeyBindings;
        foreach (var keyBinding in bindings)
        {
            if (keyBinding.Key != evt.keyCode)
            {
                continue;
            }
            if (keyBinding.RequireAlt && !keyStates.Alt)
            {
                continue;
            }
            if (keyBinding.RequireShift && !keyStates.Shift)
            {
                continue;
            }
            if (keyBinding.RequireControl && !keyStates.Ctrl)
            {
                continue;
            }

            var command = keyBinding.Command;
            if (command != null)
            {
                var acceptableArguments = ContextObjects.Where(p => command.For.IsAssignableFrom(p.GetType())).ToArray();
                var used = false;
                foreach (var argument in acceptableArguments)
                {

                    if (command.CanPerform(argument) == null)
                    {
                        uFrameEditor.Log("Key Command Executed: " + command.GetType().Name);
                        uFrameEditor.ExecuteCommand(command);
                        used = true;
                    }
                }
                return used;
            }
            return false;
        }
        return false;
    }

    public override void OnMouseDoubleClick(MouseEvent mouseEvent)
    {
        BubbleEvent(d => d.OnMouseDoubleClick(mouseEvent), mouseEvent);
        DiagramViewModel.Navigate();
        Refresh();
        Refresh();
    }

    public override void OnMouseEnter(MouseEvent e)
    {
        base.OnMouseEnter(e);
        BubbleEvent(d => d.OnMouseEnter(e), e);
    }
    public override void OnMouseExit(MouseEvent e)
    {
        base.OnMouseExit(e);
        BubbleEvent(d => d.OnMouseExit(e), e);
    }

    public override void OnMouseDown(MouseEvent mouseEvent)
    {
        base.OnMouseDown(mouseEvent);
        if (DrawersAtMouse == null) return;
        if (!DrawersAtMouse.Any())
        {
            DiagramViewModel.NothingSelected();

            mouseEvent.Begin(SelectionRectHandler);
        }
        else
        {

            BubbleEvent(d => d.OnMouseDown(mouseEvent), mouseEvent);
        }
    }

    public void BubbleEvent(Action<IDrawer> action, MouseEvent e)
    {
        if (DrawersAtMouse == null) return;

        foreach (var item in DrawersAtMouse)
        {
            action(item);
            if (e.NoBubble)
            {
                e.NoBubble = false;
                break;
            }
        }
    }

    public override void OnMouseMove(MouseEvent e)
    {
        base.OnMouseMove(e);

        if (e.IsMouseDown && e.MouseButton == 0)
        {
            foreach (var item in Children.OfType<DiagramNodeDrawer>())
            {
                if (item.ViewModelObject.IsSelected)
                {
                    item.ViewModel.Position += e.MousePositionDelta;
                    if (item.ViewModel.Position.x < 0)
                    {
                        item.ViewModel.Position = new Vector2(0f,item.ViewModel.Position.y);
                    }
                    if (item.ViewModel.Position.y < 0)
                    {
                        item.ViewModel.Position = new Vector2( item.ViewModel.Position.x,0f);
                    }
                    item.Refresh();
                }
            }
        }
        else
        {

            var nodes = GetDrawersAtPosition(this, e.MousePosition).ToArray();

            //NodeDrawerAtMouse = nodes.FirstOrDefault();

            if (DrawersAtMouse != null)
                foreach (var item in nodes)
                {
                    var alreadyInside = DrawersAtMouse.Contains(item);
                    if (!alreadyInside)
                    {
                        item.OnMouseEnter(e);
                    }
                }
            if (DrawersAtMouse != null)
                foreach (var item in DrawersAtMouse)
                {
                    if (!nodes.Contains(item))
                    {
                        item.OnMouseExit(e);
                    }
                }

            DrawersAtMouse = nodes;
            foreach (var node in DrawersAtMouse)
            {
                node.OnMouseMove(e);
            }
        }
    }

    public IDrawer[] DrawersAtMouse { get; set; }

    public IEnumerable<IDrawer> GetDrawersAtPosition(IDrawer parent, Vector2 point)
    {
        foreach (var child in parent.Children)
        {
            if (child.Bounds.Contains(point))
            {
                if (child.Children != null && child.Children.Count > 0)
                {
                    var result = GetDrawersAtPosition(child, point);
                    foreach (var item in result)
                    {
                        yield return item;
                    }
                }
                yield return child;
            }
        }
    }
    public override void OnMouseUp(MouseEvent mouseEvent)
    {

        BubbleEvent(d => d.OnMouseUp(mouseEvent), mouseEvent);

    }

    public override void OnRightClick(MouseEvent mouseEvent)
    {
        BubbleEvent(d => d.OnRightClick(mouseEvent), mouseEvent);
        if (DrawersAtMouse == null)
        {
            ShowAddNewContextMenu(true);
            return;

        }
        var item = DrawersAtMouse.FirstOrDefault();
        if (item != null)
        {
            
            if (item is DiagramNodeDrawer || item is HeaderDrawer)
            {

                ShowContextMenu();
            }
            else if (item is ItemDrawer)
            {
                ShowItemContextMenu(item);
            }
            else if (item is ConnectorDrawer)
            {
                var connector = item.ViewModelObject as ConnectorViewModel;

                var connections =
                    DiagramViewModel.GraphItems.OfType<ConnectionViewModel>()
                        .Where(p => p.ConnectorA == connector || p.ConnectorB == connector).ToArray();
                var a = new GenericMenu();
                foreach (var connection in connections)
                {
                    ConnectionViewModel connection1 = connection;
                    a.AddItem(new GUIContent("Disconnect: " + connection.Name),true, () =>
                    {
                        uFrameEditor.ExecuteCommand((v) => { connection1.Remove(connection1); });
                        
                    } );
                }
                a.ShowAsContext();
            }
        }
        else
        {

            ShowAddNewContextMenu(true);
        }
    
      
    }

    public override void Refresh(Vector2 position)
    {
        base.Refresh(position);
        // Eventually it will all be viewmodels
        if (DiagramViewModel == null) return;
        Children.Clear();
        DiagramViewModel.Load();
        Children.Add(SelectionRectHandler);
        Dirty = true;
    }

    public void Save()
    {
        DiagramViewModel.Save();
    }

    public void ShowAddNewContextMenu(bool addAtMousePosition = false)
    {
        var menu = uFrameEditor.CreateCommandUI<ContextMenuUI>(this, typeof(IDiagramContextCommand));
        menu.Go();
    }

    public void ShowContextMenu()
    {
        var menu = uFrameEditor.CreateCommandUI<ContextMenuUI>(this, typeof(IDiagramNodeCommand), DiagramViewModel.SelectedNode.CommandsType);
        menu.Go();
    }

    public void ShowItemContextMenu(object item)
    {
        var menu = uFrameEditor.CreateCommandUI<ContextMenuUI>(this, typeof(IDiagramNodeItemCommand));
        menu.Go();
    }

    protected override void DataContextChanged()
    {
        base.DataContextChanged();
        DiagramViewModel.GraphItems.CollectionChanged+= GraphItemsOnCollectionChangedWith;
    } 

    protected virtual void OnSelectionChanged(IDiagramNode olddata, IDiagramNode newdata)
    {
        SelectionChangedEventArgs handler = SelectionChanged;
        if (handler != null) handler(olddata, newdata);
    }

    private static void DrawHelp()
    {
        if (uFrameEditor.ShowHelp)
        {
            var rect = new Rect(Screen.width - 275f, 10f, 250f, uFrameEditor.KeyBindings.Length * 20f);
            GUI.Box(rect, string.Empty);

            GUILayout.BeginArea(rect);
            foreach (var keyBinding in uFrameEditor.KeyBindings.Select(p => p.Name + ": " + p.ToString()).Distinct())
            {
                EditorGUILayout.LabelField(keyBinding);
            }
            GUILayout.EndArea();
        }
    }

    private void DrawErrors()
    {
        if (DiagramViewModel.HasErrors)
        {
            GUI.Box(Rect, DiagramViewModel.Errors.Message + Environment.NewLine + DiagramViewModel.Errors.StackTrace);
        }
    }

    private void DrawSelectionRect(Rect selectionRect)
    {
        if (selectionRect.width > 20 && selectionRect.height > 20)
        {
            foreach (var item in Children)
            {
                item.IsSelected = selectionRect.Overlaps(item.Bounds.Scale(Scale));
            }
            ElementDesignerStyles.DrawExpandableBox(selectionRect, ElementDesignerStyles.BoxHighlighter4, string.Empty);
        }
    }

    public SelectionRectHandler SelectionRectHandler
    {
        get { return _selectionRectHandler ?? (_selectionRectHandler = new SelectionRectHandler(DiagramViewModel)); }
        set { _selectionRectHandler = value; }
    }

    //    if (CurrentEvent.keyCode == KeyCode.Return)
    //    {
    //        if (DiagramViewModel.SelectedNode != null && DiagramViewModel.SelectedNode.IsEditing)
    //        {
    //            DiagramViewModel.SelectedNode.EndEditing();
    private void GraphItemsOnCollectionChangedWith(NotifyCollectionChangedEventArgs changeArgs)
    {
        if (changeArgs.NewItems != null)
        foreach (var item in changeArgs.NewItems.OfType<ViewModel>())
        {
            if (item == null) Debug.Log("Graph Item is null");
            var drawer = uFrameEditor.CreateDrawer(item);
            if (drawer == null) Debug.Log("Drawer is null");
            Children.Add(drawer);
            drawer.Refresh();
        }
    }

    private bool UpgradeOldProject()
    {
        if (DiagramViewModel.NeedsUpgrade)
        {
            var rect = new Rect(50f, 50f, 200f, 75f);
            GUI.Box(rect, string.Empty);
            GUILayout.BeginArea(rect);
            GUILayout.Label("You need to upgrade to the new " + Environment.NewLine +
                            "file format for future compatability.");
            if (GUILayout.Button("Upgrade Now"))
            {
                DiagramViewModel.UpgradeProject();
                return true;
            }
            GUILayout.EndArea();
        }
        return false;
    }
}

public class SelectionRectHandler : Drawer, IInputHandler
{
    public override int ZOrder
    {
        get { return 100; }
    }

    public DiagramViewModel ViewModel
    {
        get { return DataContext as DiagramViewModel; }
        set { DataContext = value; }
    }

    public SelectionRectHandler(DiagramViewModel diagram)
    {
        ViewModel = diagram;
    }

    public override void OnMouseDoubleClick(MouseEvent mouseEvent)
    {
        mouseEvent.Cancel();
    }

    public override void OnMouseDown(MouseEvent mouseEvent)
    {
        mouseEvent.Cancel();
    }

    public override void OnMouseMove(MouseEvent e)
    {
        if (e.IsMouseDown && e.MouseButton == 0)
            SelectionRect = CreateSelectionRect(e.MouseDownPosition, e.MousePosition);
    }

    public Rect SelectionRect { get; set; }

    public static Rect CreateSelectionRect(Vector2 start, Vector2 current)
    {
        if (current.x > start.x)
        {
            if (current.y > start.y)
            {
                return new Rect(start.x, start.y,
                    current.x - start.x, current.y - start.y);
            }
            else
            {
                return new Rect(
                    start.x, current.y, current.x - start.x, start.y - current.y);
            }
        }
        else
        {
            if (current.y > start.y)
            {
                // x is less and y is greater
                return new Rect(
                    current.x, start.y, start.x - current.x, current.y - start.y);
            }
            else
            {
                // both are less
                return new Rect(
                    current.x, current.y, start.x - current.x, start.y - current.y);
            }
        }
    }

    public override void OnMouseUp(MouseEvent mouseEvent)
    {
        SelectionRect = new Rect(0f, 0f, 0f, 0f);
        mouseEvent.Cancel();
    }




    public override void Draw(float scale)
    {
        base.Draw(scale);
        if (ViewModel == null) return;
        if (ViewModel.GraphItems == null) return;
        if (SelectionRect.width > 20 && SelectionRect.height > 20)
        {
            foreach (var item in ViewModel.GraphItems.OfType<DiagramNodeViewModel>())
            {
                item.IsSelected = SelectionRect.Scale(scale).Overlaps(item.Bounds.Scale(scale));
            }
            ElementDesignerStyles.DrawExpandableBox(SelectionRect.Scale(scale), ElementDesignerStyles.BoxHighlighter4, string.Empty);
        }
    }
}

public class DiagramInputHander : IInputHandler
{
    public GraphItemViewModel ViewModelAtMouse { get; set; }
    public DiagramViewModel ViewModel { get; set; }

    public DiagramInputHander(DiagramViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public virtual void OnMouseDoubleClick(MouseEvent e)
    {

    }

    public virtual void OnMouseDown(MouseEvent e)
    {

    }

    public virtual void OnMouseMove(MouseEvent e)
    {
        ViewModelAtMouse = ViewModel.GraphItems.FirstOrDefault(p => p.Bounds.Contains(e.MousePosition));


    }

    public virtual void OnMouseUp(MouseEvent e)
    {

    }

    public void OnRightClick(MouseEvent mouseEvent)
    {

    }
}
public class ConnectionHandler : DiagramInputHander
{
    public ConnectorViewModel StartConnector { get; set; }
    public ConnectionViewModel CurrentConnection { get; set; }

    public List<ConnectorViewModel> PossibleConnections { get; set; }

    public ConnectionHandler(DiagramViewModel viewModel, ConnectorViewModel startConnector)
        : base(viewModel)
    {
        StartConnector = startConnector;
        PossibleConnections = new List<ConnectorViewModel>();

        foreach (var connection in viewModel.GraphItems.OfType<ConnectorViewModel>())
        {
            foreach (var strategy in uFrameEditor.ConnectionStrategies)
            {
                if (strategy.Connect(StartConnector, connection) != null)
                {
                    PossibleConnections.Add(connection);
                }
            }
        }
        foreach (var a in PossibleConnections)
        {
            a.IsMouseOver = true;
        }

    }


    public override void OnMouseDown(MouseEvent e)
    {
        foreach (var a in PossibleConnections)
        {
            a.IsMouseOver = false;
        }
        e.Cancel();
    }

    public override void OnMouseMove(MouseEvent e)
    {
        base.OnMouseMove(e);
        var _startPos = StartConnector.Bounds.center;

        var _endPos = e.MousePosition;

        var endViewModel = ViewModelAtMouse as ConnectorViewModel;
        var color = Color.green;



        if (endViewModel == null)
        {
            var nodeAtMouse = ViewModelAtMouse as DiagramNodeViewModel;
            if (nodeAtMouse != null)
            {

                foreach (var connector in nodeAtMouse.Connectors)
                {

                    ConnectionViewModel connection = null;
                    foreach (var strategy in uFrameEditor.ConnectionStrategies)
                    {

                        //try and connect them
                        connection = strategy.Connect(StartConnector, connector);
                        if (connection != null)
                            break;

                    }
                    if (connection != null)
                    {
                        CurrentConnection = connection;
                        _endPos = connector.Bounds.center;
                        connector.HasConnections = true;
                        break;
                    }
                }
                if (CurrentConnection != null)
                {
                    // Grab the default connector
                    var adjustedBounds = new Rect(nodeAtMouse.Bounds.x - 9, nodeAtMouse.Bounds.y + 1, nodeAtMouse.Bounds.width + 19, nodeAtMouse.Bounds.height + 9);
                    ElementDesignerStyles.DrawExpandableBox(adjustedBounds.Scale(ElementDesignerStyles.Scale), ElementDesignerStyles.NodeBackground, string.Empty, 20);
                }
            }

        }
        else
        {

            foreach (var strategy in uFrameEditor.ConnectionStrategies)
            {
                //try and connect them
                var connection = strategy.Connect(StartConnector, endViewModel);
                if (connection != null)
                {
                    CurrentConnection = connection;
                    break;
                }
            }
            if (CurrentConnection == null)
            {
                color = Color.red;
            }
            else
            {
                _endPos = endViewModel.Bounds.center;

            }
        }


        var _startRight = StartConnector.Direction == ConnectorDirection.Output;
        var _endRight = false;

        var startTan = _startPos + (_endRight ? -Vector2.right * 3 : Vector2.right * 3) * 30;

        var endTan = _endPos + (_startRight ? -Vector2.right * 3 : Vector2.right * 3) * 30;

        var shadowCol = new Color(0, 0, 0, 0.1f);

        for (int i = 0; i < 3; i++) // Draw a shadow
            UnityEditor.Handles.DrawBezier(_startPos * ElementDesignerStyles.Scale, _endPos * ElementDesignerStyles.Scale, startTan * ElementDesignerStyles.Scale, endTan * ElementDesignerStyles.Scale, shadowCol, null, (i + 1) * 5);

        UnityEditor.Handles.DrawBezier(_startPos * ElementDesignerStyles.Scale, _endPos * ElementDesignerStyles.Scale, startTan * ElementDesignerStyles.Scale, endTan * ElementDesignerStyles.Scale, color, null, 3);

    }

    public override void OnMouseUp(MouseEvent e)
    {
        base.OnMouseUp(e);
        if (CurrentConnection != null)
        {
            uFrameEditor.ExecuteCommand((v) =>
            {
                CurrentConnection.Apply(CurrentConnection);
            });


        }
        foreach (var a in PossibleConnections)
        {
            a.IsMouseOver = false;
        }
        e.Cancel();
    }

}
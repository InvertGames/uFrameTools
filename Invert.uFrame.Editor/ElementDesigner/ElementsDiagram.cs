using Invert.Common;
using Invert.Common.UI;
using Invert.MVVM;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class ElementsDiagram : Drawer, ICommandHandler
{
    public delegate void SelectionChangedEventArgs(IDiagramNode oldData, IDiagramNode newData);

    public delegate void ViewModelDataEventHandler(ElementData data);

    public event SelectionChangedEventArgs SelectionChanged;

    private IElementDesignerData _data;

    private List<IDrawer> _nodeDrawers = new List<IDrawer>();

    private INodeDrawer _selected;

    private ISelectable _selectedItem;

    private SerializedObject _serializedObject;
    private Event _currentEvent;
    private SerializedObject _o;

    //public IEnumerable<IDiagramNode> AllSelected
    //{
    //    get
    //    {
    //        return Data.GetDiagramItems().Where(p => p.IsSelected);
    //    }
    //}

    public IDiagramNode CurrentMouseOverNode { get; set; }

    //public IElementDesignerData Data
    //{
    //    get { return _data; }
    //    set
    //    {
    //        _data = value;

    //        if (_data != null)
    //        {
    //            _data.Prepare();
    //            //_data.ReloadFilterStack();
    //        }
    //        Refresh();
    //    }
    //}

    public Rect DiagramSize
    {
        get
        {
            Rect size = new Rect();
            foreach (var diagramItem in NodeDrawers)
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

    public bool DidDrag { get; set; }

    public bool Dirty { get; set; }

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

    public List<IDrawer> NodeDrawers
    {
        get { return _nodeDrawers; }
        set { _nodeDrawers = value; }
    }

    public bool IsMouseDown { get; set; }

    public Vector2 LastDragPosition { get; set; }

    public Vector2 LastMouseDownPosition { get; set; }

    public Vector2 LastMouseUpPosition { get; set; }

    public static float Scale
    {
        get { return ElementDesignerStyles.Scale; }
    }

    public IDrawer NodeDrawerAtMouse
    {
        get
        {
            return NodeDrawers.FirstOrDefault(p => p.Bounds.Scale(Scale).Contains(CurrentMousePosition));
            //return Data.DiagramItems.LastOrDefault(p => p.Position.Contains(CurrentMousePosition));
        }
    }

    //protected IElementsDataRepository Repository { get; set; }


    public Vector2 SelectionOffset { get; set; }

    public Rect SelectionRect { get; set; }

    //private SerializedObject SerializedObject
    //{
    //    get
    //    {
    //        if (Data is UnityEngine.Object)
    //        {
    //            return _o ?? (_o = new SerializedObject(Data as UnityEngine.Object));
    //        }
    //        return null;
    //    }
    //    set { _o = value; }
    //}

    public ElementsDiagram(DiagramViewModel viewModel)
    {
        DiagramViewModel = viewModel;


    }
    


    public Vector2 CurrentMousePosition
    {
        get
        {

            return CurrentEvent.mousePosition;
        }
    }

    public float SnapSize
    {
        get { return DiagramViewModel.Settings.SnapSize * Scale; }
    }

    public void Save()
    {
        DiagramViewModel.Save();
       
    }

    public override void Draw(float scale)
    {
        
        // Make sure they've upgraded to the latest json format
        if (UpgradeOldProject()) return;
        // Draw the title box
        GUI.Box(new Rect(0, 0f, Rect.width, 30f), DiagramViewModel.Title, ElementDesignerStyles.DiagramTitle);

        string focusItem = null;

        //var links = Data.Links.ToArray();
        //foreach (var link in links)
        //{
        //    link.Draw(this);
        //}

        // Draw all of our drawers
        foreach (var drawer in NodeDrawers.OrderBy(p => p.IsSelected).ToArray())
        {
            if (drawer.Dirty)
            {
                drawer.Refresh();
                drawer.Dirty = false;
            }
            //drawer.CalculateBounds();
            var shouldFocus = drawer.ShouldFocus;
            if (shouldFocus != null)
                focusItem = shouldFocus;

            drawer.Draw(Scale);
        }

        //foreach (var link in links)
        //{
        //    //link.Draw(this);
        //    link.DrawPoints(this);
        //}

        if (focusItem != null)
        {
            EditorGUI.FocusTextInControl(focusItem);
        }

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

            foreach (var viewModelDrawer in NodeDrawers.Where(p => p.IsSelected))
            {
                viewModelDrawer.Refresh();
            }
            DidDrag = true;
            LastDragPosition = CurrentMousePosition;
        }

        //if (!DiagramViewModel.SelectedGraphItems.Any())
        //{
        //    SelectionRect = IsMouseDown ? CreateSelectionRect(LastMouseDownPosition, CurrentMousePosition) : new Rect(0f, 0f, 0f, 0f);
        //}
            

        //DrawSelectionRect(SelectionRect);

        DrawErrors();
        DrawHelp();

        if (!EditorApplication.isCompiling)
        {
            HandleInput();
        }

    }

    private void DrawSelectionRect(Rect selectionRect)
    {
        if (selectionRect.width > 20 && selectionRect.height > 20)
        {
            foreach (var item in NodeDrawers)
            {
                item.IsSelected = selectionRect.Overlaps(item.Bounds.Scale(Scale));
            }
            ElementDesignerStyles.DrawExpandableBox(selectionRect, ElementDesignerStyles.BoxHighlighter4, string.Empty);
        }
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

    public void HandleInput()
    {

        var e = Event.current;

        if (e.type == EventType.MouseDown && e.button != 2)
        {
            CurrentEvent = Event.current;
            LastMouseDownPosition = e.mousePosition;
            IsMouseDown = true;
            OnMouseDown();
            if (e.clickCount > 1)
            {
                OnDoubleClick();
            }
            e.Use();
        }
        if (CurrentEvent.rawType == EventType.MouseUp && IsMouseDown)
        {
            LastMouseUpPosition = e.mousePosition;
            IsMouseDown = false;
            OnMouseUp();
            e.Use();
        }
        if (CurrentEvent.keyCode == KeyCode.Return)
        {
            if (DiagramViewModel.SelectedNode != null && DiagramViewModel.SelectedNode.IsEditing)
            {
                DiagramViewModel.SelectedNode.EndEditing();
                
            }
        }
        if (CurrentEvent.keyCode == KeyCode.F2)
        {
            if (DiagramViewModel.SelectedNode != null)
            {
                DiagramViewModel.SelectedNode.BeginEditing();
                e.Use();
            }
        }
    }

    public DiagramViewModel DiagramViewModel
    {
        get { return this.DataContext as DiagramViewModel; }
        set { this.DataContext = value; }
    }

    protected override void DataContextChanged()
    {
        base.DataContextChanged();
        DiagramViewModel.GraphItems.CollectionChangedWith += GraphItemsOnCollectionChangedWith;
    }

    private void GraphItemsOnCollectionChangedWith(ModelCollectionChangeEventWith<GraphItemViewModel> changeArgs)
    {
     
        foreach (var item in changeArgs.NewItemsOfT)
        {
            if (item == null) Debug.Log("Graph Item is null");
            var drawer = uFrameEditor.CreateDrawer(item);
            if (drawer == null) Debug.Log("Drawer is null");
            NodeDrawers.Add(drawer);
            drawer.Refresh();
        }
    }

    public void Refresh()
    {
        // Eventually it will all be viewmodels
        if (DiagramViewModel == null) return;
        NodeDrawers.Clear();
        DiagramViewModel.Load();
        Dirty = true;
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

    protected virtual void OnSelectionChanged(IDiagramNode olddata, IDiagramNode newdata)
    {
        SelectionChangedEventArgs handler = SelectionChanged;
        if (handler != null) handler(olddata, newdata);
    }

    private void OnDoubleClick()
    {
        DiagramViewModel.Navigate();
        Refresh();
        Refresh();
    }

    private void OnMouseDown()
    {
        var selected = NodeDrawerAtMouse;

        if (selected == null)
        {
             DiagramViewModel.NothingSelected();
        }
        else
        {
            // Keep up with the drag position offset
            if (Event.current.button != 2)
            {
                SelectionOffset = LastMouseDownPosition - new Vector2(selected.Bounds.x, selected.Bounds.y);
                LastDragPosition = LastMouseDownPosition;
            }

            DiagramViewModel.Select(selected.ViewModelObject);
        }
    }

    public Event CurrentEvent
    {
        get { return Event.current ?? _currentEvent; }
        set { _currentEvent = value; }
    }

    private void OnMouseUp()
    {
        if (CurrentEvent.button == 1)
        {
            OnRightClick();
            return;
        }
        IsMouseDown = false;
    }

    private void OnRightClick()
    {
        if (DiagramViewModel.SelectedNodeItem != null)
        {
            ShowItemContextMenu(DiagramViewModel.SelectedNodeItem);
        }
        else if (DiagramViewModel.SelectedNode != null)
        {
            ShowContextMenu();
        }
        else
        {
            ShowAddNewContextMenu(true);
        }
    }

    

    public void HandleKeyEvent(Event evt, ModifierKeyState keyStates)
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
#if DEBUG
                Debug.Log("Skipping because of alt");
#endif
                continue;
            }
            if (keyBinding.RequireShift && !keyStates.Shift)
            {
#if DEBUG
                Debug.Log("Skipping because of shift");
#endif
                continue;
            }
            if (keyBinding.RequireControl && !keyStates.Ctrl)
            {
#if DEBUG
                Debug.Log("Skipping because of ctrl");
#endif
                continue;
            }

            var command = keyBinding.Command;
            if (command != null)
            {
                var acceptableArguments = ContextObjects.Where(p => command.For.IsAssignableFrom(p.GetType())).ToArray();
                foreach (var argument in acceptableArguments)
                {
                    if (command.CanPerform(argument) == null)
                    {
#if DEBUG
                        UnityEngine.Debug.Log("Key Command Executed: " + command.GetType().Name);
#endif
                        this.ExecuteCommand(command);
                        return;
                    }
                }

            }
        }
    }

    public IEnumerable<object> ContextObjects
    {
        get
        {
            yield return this;



            //yield return SelectedItem;
            //var selectedItem = SelectedItem;
            //if (selectedItem != null)
            //{
            //    yield return selectedItem;
            //}
            //else
            //{
            //    //if (CurrentMousePosition == null)

            //}
            //if (Data != null)
            //{
            //    yield return Data;
            //}
            yield return DiagramViewModel;
            foreach (var item in DiagramViewModel.GraphItems)
            {
                yield return item;
            }
            //var allSelected = AllSelected.ToArray();

            //foreach (var diagramItem in allSelected)
            //{
            //    yield return diagramItem;
            //    if (diagramItem.Data != null)
            //    {
            //        yield return diagramItem.Data;
            //    }
            //}
            //if (allSelected.Length < 1)
            //{
            //    var mouseOverViewData = NodeDrawerAtMouse;
            //    if (mouseOverViewData != null)
            //    {
            //        var mouseOverDataModel = NodeDrawerAtMouse.ViewModelObject;
            //        if (mouseOverDataModel != null)
            //        {
            //            yield return mouseOverDataModel;
            //        }
            //    }

            //}
        }
    }

    public Rect Rect { get; set; }

    public void CommandExecuted(IEditorCommand command)
    {
        DiagramViewModel.MarkDirty();
#if DEBUG
        Debug.Log(command.Title + " Executed");
#endif
        this.Refresh();
        Dirty = true;
    }

    public void CommandExecuting(IEditorCommand command)
    {
        DiagramViewModel.RecordUndo( command.Title);
    }
}
using Invert.Common;
using Invert.Common.UI;
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

public class ElementsDiagram : ICommandHandler
{
    public delegate void SelectionChangedEventArgs(IDiagramNode oldData, IDiagramNode newData);

    public delegate void ViewModelDataEventHandler(ElementData data);


    public event SelectionChangedEventArgs SelectionChanged;


    private IElementDesignerData _data;

    private List<INodeDrawer> _nodeDrawers = new List<INodeDrawer>();

    private INodeDrawer _selected;

    private ISelectable _selectedItem;

    private SerializedObject _serializedObject;
    private Event _currentEvent;
    private SerializedObject _o;

    public IEnumerable<IDiagramNode> AllSelected
    {
        get
        {
            return Data.GetDiagramItems().Where(p => p.IsSelected);
        }
    }

    public IDiagramNode CurrentMouseOverNode { get; set; }

    //public ISelectable CurrentMouseOverNodeItem
    //{
    //    get
    //    {
    //        var node = MouseOverViewData;
    //        if (node == null)
    //            return null;

    //        return node.Items.OfType<ISelectable>()
    //             .FirstOrDefault(p => p.Position.Scale(Scale).Contains(Event.current.mousePosition));

    //    }
    //}

    public IElementDesignerData Data
    {
        get { return _data; }
        set
        {
            _data = value;

            if (_data != null)
            {
                _data.Prepare();
                //_data.ReloadFilterStack();
            }
            Refresh(true);
        }
    }

    public Rect DiagramSize
    {
        get
        {
            Rect size = new Rect();
            foreach (var diagramItem in this.Data.GetDiagramItems())
            {
                var rect = diagramItem.Position.Scale(Scale);

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

    public List<INodeDrawer> NodeDrawers
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

    public INodeDrawer MouseOverViewData
    {
        get
        {
            return NodeDrawers.FirstOrDefault(p => p.Bounds.Scale(Scale).Contains(CurrentMousePosition));
            //return Data.DiagramItems.LastOrDefault(p => p.Position.Contains(CurrentMousePosition));
        }
    }

    protected IElementsDataRepository Repository { get; set; }

    public IDiagramNode SelectedData
    {
        get
        {
            if (Selected == null) return null;
            return Selected.ViewModel.GraphItemObject;
        }
        //set
        //{
        //    Selected = NodeDrawers.FirstOrDefault(p => p.ViewModel.GraphItemObject == value);
        //}
    }


    public INodeDrawer Selected
    {
        get { return SelectedDrawers.OfType<INodeDrawer>().FirstOrDefault();  }
    }

    public IEnumerable<IDrawer> SelectedDrawers
    {
        get
        {
            foreach (var nodeDrawer in NodeDrawers)
            {
                foreach (var child in nodeDrawer.Children)
                {
                    if (child.IsSelected) yield return child;
                }
                if (nodeDrawer.IsSelected) yield return nodeDrawer;
            }
        }
    }

    public IEnumerable<ItemViewModel> SelectedItems
    {
        get { return SelectedDrawers.Select(p => p.ViewModelObject).OfType<ItemViewModel>(); }
    }

    public ItemViewModel SelectedItem
    {
        get { return SelectedItems.First(); }
    }


    //public ISelectable SelectedItem
    //{
    //    get { return _selectedItem; }
    //    set
    //    {
    //        foreach (var item in NodeDrawers.SelectMany(p => p.Items))
    //            item.IsSelected = false;

    //        var old = _selectedItem;
    //        //Data.DiagramItems.ToList().ForEach(p => p.IsSelected = false);
    //        GUI.FocusControl("");

    //        _selectedItem = value;
    //        if (old != value)
    //        {

    //            OnSelectionChanged(SelectedData, SelectedData);
    //        }
    //        if (_selectedItem != null)
    //        {
    //            _selectedItem.IsSelected = true;
    //        }
    //    }
    //}

    public Vector2 SelectionOffset { get; set; }

    public Rect SelectionRect { get; set; }

    private SerializedObject SerializedObject
    {
        get
        {
            if (Data is UnityEngine.Object)
            {
                return _o ?? (_o = new SerializedObject(Data as UnityEngine.Object));
            }
            return null;
        }
        set { _o = value; }
    }

    public ElementsDiagram(string assetPath)
    {
        var fileExtension = Path.GetExtension(assetPath);
        if (string.IsNullOrEmpty(fileExtension)) fileExtension = ".asset";
        var repositories = uFrameEditor.Container.ResolveAll<IElementsDataRepository>();
        foreach (var elementsDataRepository in repositories)
        {
            var diagram = elementsDataRepository.LoadDiagram(assetPath);

            if (diagram == null) continue;

            Repository = elementsDataRepository;
            Data = diagram;

            break;
        }

        if (Repository == null || Data == null)
        {
            throw new Exception(
                string.Format(
                    "The asset with the extension {0} could not be loaded.  Do you have the plugin installed?",
                    fileExtension
                    ));
        }
        else
        {

            Data.Settings.CodePathStrategy =
             uFrameEditor.Container.Resolve<ICodePathStrategy>(Data.Settings.CodePathStrategyName ?? "Default");
            if (Data.Settings.CodePathStrategy == null)
            {
                Data.Settings.CodePathStrategy = uFrameEditor.Container.Resolve<ICodePathStrategy>("Default");
            }
            Data.Settings.CodePathStrategy.Data = Data;
            Data.Settings.CodePathStrategy.AssetPath =
                assetPath.Replace(string.Format("{0}{1}", Path.GetFileNameWithoutExtension(assetPath), fileExtension), "").Replace("/", Path.DirectorySeparatorChar.ToString());


        }
    }

    public INodeDrawer CreateDrawerFor(IDiagramNode node)
    {
        return uFrameEditor.CreateDrawer(node, this);
    }

    public void DeselectAll()
    {
        foreach (var diagramItem in AllSelected)
        {
            if (diagramItem.IsEditing)
            {
                diagramItem.EndEditing();
            }
            diagramItem.IsSelected = false;
        }
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
        get { return Data.Settings.SnapSize * Scale; }
    }

    public void Save()
    {
        Repository.SaveDiagram(Data);
    }

    public void Draw()
    {

        // Make sure they've upgraded to the latest json format
        if (UpgradeOldProject()) return;
        // Draw the title box
        GUI.Box(new Rect(0, 0f, Rect.width, 30f), Data.Name, ElementDesignerStyles.DiagramTitle);

        string focusItem = null;

        var links = Data.Links.ToArray();
        foreach (var link in links)
        {
            link.Draw(this);
        }

        // Draw all of our drawers
        foreach (var drawer in NodeDrawers.OrderBy(p => p.IsSelected).ToArray())
        {
            if (drawer.Dirty)
            {
                drawer.Refresh(Vector2.zero);
                drawer.Dirty = false;
            }
            //drawer.CalculateBounds();
            var shouldFocus = drawer.ShouldFocus;
            if (shouldFocus != null)
                focusItem = shouldFocus;

            drawer.Draw(Scale);
        }

        foreach (var link in links)
        {
            //link.Draw(this);
            link.DrawPoints(this);
        }

        if (focusItem != null)
        {
            EditorGUI.FocusTextInControl(focusItem);
        }

        if (IsMouseDown && SelectedData != null && SelectedItem == null && !CurrentEvent.control)
        {
            var newPosition = DragDelta;//CurrentMousePosition - SelectionOffset;
            var allSelected = AllSelected.ToArray();
            foreach (var diagramItem in allSelected)
            {
                diagramItem.Location += (newPosition * (1f / Scale));
                diagramItem.Location = new Vector2(Mathf.Round((diagramItem.Location.x) / SnapSize) * SnapSize, Mathf.Round(diagramItem.Location.y / SnapSize) * SnapSize);
            }

            foreach (var viewModelDrawer in NodeDrawers.Where(p => p.IsSelected))
            {
                viewModelDrawer.Refresh(Vector2.zero);
            }
            DidDrag = true;
            LastDragPosition = CurrentMousePosition;
        }
        else 
        {
            SelectionRect = IsMouseDown ? CreateSelectionRect(LastMouseDownPosition, CurrentMousePosition) : new Rect(0f,0f,0f,0f);
        }


        DrawSelectionRect(SelectionRect);

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
            foreach (var item in Data.GetDiagramItems())
            {
                item.IsSelected = selectionRect.Overlaps(item.Position.Scale(Scale));
            }
            ElementDesignerStyles.DrawExpandableBox(selectionRect, ElementDesignerStyles.BoxHighlighter4, string.Empty);
        }
    }

    private static void DrawHelp()
    {
        if (uFrameEditor.ShowHelp)
        {
            var rect = new Rect(Screen.width - 275f, 10f, 250f, uFrameEditor.KeyBindings.Length*20f);
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
        if (Data is JsonElementDesignerData)
        {
            var dd = Data as JsonElementDesignerData;
            if (dd.Errors)
            {
                GUI.Box(Rect, dd.Error.Message + Environment.NewLine + dd.Error.StackTrace);
            }
        }
    }

    private bool UpgradeOldProject()
    {
        if (Data is ElementDesignerData)
        {
            var rect = new Rect(50f, 50f, 200f, 75f);
            GUI.Box(rect, string.Empty);
            GUILayout.BeginArea(rect);
            GUILayout.Label("You need to upgrade to the new " + Environment.NewLine +
                            "file format for future compatability.");
            if (GUILayout.Button("Upgrade Now"))
            {
                this.ExecuteCommand(new ConvertToJSON());
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
            if (SelectedData != null && SelectedData.IsEditing)
            {
                SelectedData.EndEditing();
                //e.Use();
                this.Dirty = true;
            }
        }
        if (CurrentEvent.keyCode == KeyCode.F2)
        {
            if (SelectedData != null)
            {
                SelectedData.BeginEditing();
                e.Use();
            }
        }
    }

    public void Refresh(bool refreshDrawers = true)
    {
        if (refreshDrawers)
            NodeDrawers.Clear();

        foreach (var diagramItem in Data.GetDiagramItems())
        {
            diagramItem.Data = Data;
            diagramItem.Dirty = true;
            var drawer = CreateDrawerFor(diagramItem);
            if (drawer == null) continue;

            NodeDrawers.Add(drawer);

            if (refreshDrawers)
            {
                drawer.Refresh(Vector2.zero);
            }
        }
        Data.UpdateLinks();
        Dirty = true;
    }

    public void ShowAddNewContextMenu(bool addAtMousePosition = false)
    {
        var menu = uFrameEditor.CreateCommandUI<ContextMenuUI>(this, typeof(IDiagramContextCommand));
        menu.Go();
    }

    public void ShowContextMenu()
    {

        var menu = uFrameEditor.CreateCommandUI<ContextMenuUI>(this, typeof(IDiagramNodeCommand), Selected.CommandsType);
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

        if (SelectedData != null)
        {
            if (SelectedItem == null)
            {
                if (SelectedData is IDiagramFilter)
                {
                    if (SelectedData == Data.CurrentFilter)
                    {
                        Data.PopFilter(null);
                    }
                    else
                    {
                        Data.PushFilter(SelectedData as IDiagramFilter);
                    }

                    Refresh(true);
                    Refresh(true);
                }
                else
                {
                    Selected.DoubleClicked();
                }
            }

        }

    }

    private void OnMouseDown()
    {
        var selected = MouseOverViewData;
        if (selected != null)
        {
            if (Selected != null)
            {
                if (CurrentEvent.shift)
                {
                }
                else
                {
                    if (AllSelected.All(p => p != selected.ViewModel.GraphItemObject))
                    {
                        DeselectAll();
                    }
                }
            }
            if (Event.current.button != 2)
            {
                SelectionOffset = LastMouseDownPosition - new Vector2(selected.Bounds.x, selected.Bounds.y);
                LastDragPosition = LastMouseDownPosition;
            }

        }
        else
        {
            //if (AllSelected.All(p => p != SelectedData))
            //{
            foreach (var diagramItem in AllSelected)
            {
                if (diagramItem.IsEditing)
                {
                    diagramItem.EndEditing();

                }
                diagramItem.IsSelected = false;
            }
            //}
        }

        if (selected != null)
        selected.IsSelected = true;
        //Selected = selected;
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
            if (SelectedItem != null)
            {
                ShowItemContextMenu(SelectedItem);
            }
            else if (SelectedData != null)
            {
                ShowContextMenu();
            }
            else
            {
                ShowAddNewContextMenu(true);
            }
            IsMouseDown = false;
            return;
        }

        if (DidDrag)
        {
            Repository.MarkDirty(Data);
        }
        DidDrag = false;
    }

    public void ExecuteCommand(Action<ElementsDiagram> action)
    {
        this.ExecuteCommand(new SimpleEditorCommand<ElementsDiagram>(action));
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
            var allSelected = AllSelected.ToArray();

            foreach (var diagramItem in allSelected)
            {
                yield return diagramItem;
                if (diagramItem.Data != null)
                {
                    yield return diagramItem.Data;
                }
            }
            if (allSelected.Length < 1)
            {
                var mouseOverViewData = MouseOverViewData;
                if (mouseOverViewData != null)
                {
                    var mouseOverDataModel = MouseOverViewData.ViewModelObject;
                    if (mouseOverDataModel != null)
                    {
                        yield return mouseOverDataModel;
                    }
                }

            }
        }
    }

    public Rect Rect { get; set; }

    public void CommandExecuted(IEditorCommand command)
    {
        Repository.MarkDirty(Data);
#if DEBUG
        Debug.Log(command.Title + " Executed");
#endif
        this.Refresh();
        Dirty = true;
    }

    public void CommandExecuting(IEditorCommand command)
    {
        Repository.RecordUndo(Data, command.Title);
    }
}
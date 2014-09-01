using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class ElementsDesigner : EditorWindow, ICommandHandler
    {
        private static float HEIGHT = 768;

        private static List<Matrix4x4> stack = new List<Matrix4x4>();

        private static float WIDTH = 1024;

        public ModifierKeyState ModifierKeyStates
        {
            get { return _modifierKeyStates ?? (_modifierKeyStates = new ModifierKeyState()); }
            set { _modifierKeyStates = value; }
        }


        public ICommandUI Toolbar
        {
            get
            {
                if (_toolbar != null) return _toolbar;


                return _toolbar = uFrameEditor.CreateCommandUI<ToolbarUI>(this, typeof(IToolbarCommand));
            }
            set { _toolbar = value; }
        }

        [SerializeField]
        private ElementsDiagram _diagram;

        private Vector2 _scrollPosition;
        private ICommandUI _toolbar;
        private ModifierKeyState _modifierKeyStates;


        public static IElementDesignerData SelectedElementDiagram
        {
            get { return Selection.activeObject as IElementDesignerData; }
        }

        public ElementsDiagram Diagram
        {
            get { return _diagram; }
            set
            {
                _diagram = value;
                _toolbar = null;
            }
        }

        public float InspectorWidth
        {
            get
            {
                return 0;
                //if (Diagram == null || Diagram.Selected == null)
                //    return 0;
                //return 240;
            }
        }

        public bool IsMiddleMouseDown { get; set; }

        public string LastLoadedDiagram
        {
            get
            {
                return EditorPrefs.GetString("LastLoadedDiagram", null);
            }
            set
            {
                EditorPrefs.SetString("LastLoadedDiagram", value);
            }
        }

        public Vector2 PanStartPosition { get; set; }

        static public void BeginGUI()
        {
            stack.Add(GUI.matrix);
            Matrix4x4 m = new Matrix4x4();
            var w = (float)Screen.width;
            var h = (float)Screen.height;
            var aspect = w / h;
            var scale = 1f;
            var offset = Vector3.zero;
            if (aspect < (WIDTH / HEIGHT))
            { //screen is taller
                scale = (Screen.width / WIDTH);
                offset.y += (Screen.height - (HEIGHT * scale)) * 0.5f;
            }
            else
            { // screen is wider
                scale = (Screen.height / HEIGHT);
                offset.x += (Screen.width - (WIDTH * scale)) * 0.5f;
            }
            m.SetTRS(offset, Quaternion.identity, Vector3.one * scale);
            GUI.matrix *= m;
        }

        static public void EndGUI()
        {
            GUI.matrix = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
        }

        [MenuItem("Window/Element Designer", false, 1)]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = (ElementsDesigner)GetWindow(typeof(ElementsDesigner));
            window.title = "Elements";

            //window.DesignerViewModel = uFrameEditor.Application.Designer;
        
            //var repo = new ElementsDataRepository();
            //var diagram = new ElementsDiagram(repo);
            //diagram.Data.ViewModels.Add(repo.GetViewModel(typeof(FPSWeaponViewModel)));
            //diagram.Data.ViewModels.Add(repo.GetViewModel(typeof(FPSBulletViewModel)));

            // RemoveFromDiagram when switching to add all
            window.Show();
        }

        public void InfoBox(string message, MessageType type = MessageType.Info)
        {
            EditorGUI.HelpBox(new Rect(15, 30, 300, 30), message, type);
        }

        

        //public void OnGUI()
        //{
        //    foreach (var editorCommand in DesignerViewModel.ToolbarCommands.Where(p => p.Position == ToolbarPosition.Right).OrderBy(p => p.Order))
        //    {
        //        DoCommand(editorCommand);
        //    }
        //    GUILayout.FlexibleSpace();
        //    foreach (var editorCommand in DesignerViewModel.ToolbarCommands.Where(p=>p.Position == ToolbarPosition.Right).OrderBy(p => p.Order))
        //    {
        //        DoCommand(editorCommand);
        //    }

        //}
        public void DoCommand(IEditorCommand command)
        {
            var obj = ContextObjects.FirstOrDefault(p => command.For.IsAssignableFrom(p.GetType()));
            GUI.enabled = command.CanPerform(obj) == null;
            if (command is IDynamicOptionsCommand)
            {
                var cmd = command as IDynamicOptionsCommand;


                foreach (var ufContextMenuItem in cmd.GetOptions(obj))
                {
                    if (GUILayout.Button(new GUIContent(ufContextMenuItem.Name), EditorStyles.toolbarButton))
                    {
                        cmd.SelectedOption = ufContextMenuItem;
                        this.ExecuteCommand(command);
                    }
                }
            }
            else if (GUILayout.Button(new GUIContent(command.Title), EditorStyles.toolbarButton))
            {

                if (command is IParentCommand)
                {
                    var contextUI = uFrameEditor.CreateCommandUI<ContextMenuUI>(this, command.GetType());
                    contextUI.Flatten = true;
                    contextUI.Go();
                }
                else
                {
                    this.ExecuteCommand(command);
                }
            }
            GUI.enabled = true;
        }
        public void OnGUI()
        {
         
            if ((Diagram == null || Diagram.Data == null) && !string.IsNullOrEmpty(LastLoadedDiagram))
            {
                Diagram = null;
                if (!LoadDiagram(LastLoadedDiagram))
                {
                    LastLoadedDiagram = null;
                    return;
                }
                if (Diagram == null)
                {
                    LastLoadedDiagram = null;
                    return;
                }
            }
            
          
            var style = ElementDesignerStyles.Background;
            style.border = new RectOffset(
                Mathf.RoundToInt(41),
                Mathf.RoundToInt(41),
                Mathf.RoundToInt(32),
                Mathf.RoundToInt(32));
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            //DoToolbar(diagramRect);
            DoToolbar();
            GUILayout.EndHorizontal();
            if (Diagram == null) return;
            var diagramRect = new Rect(0f, (EditorStyles.toolbar.fixedHeight) - 1, Screen.width - 3, Screen.height - (EditorStyles.toolbar.fixedHeight * 2) - EditorStyles.toolbar.fixedHeight - 2);
            Diagram.Rect = diagramRect;
            GUI.Box(diagramRect, string.Empty, style);

            if (Diagram == null)
            {
            }
            else
            {
                if (Event.current.control && Event.current.type == EventType.mouseDown)
                {
                }
                _scrollPosition = GUI.BeginScrollView(diagramRect, _scrollPosition, Diagram.DiagramSize);

                HandlePanning(diagramRect);
                if (Diagram.Data != null)
                {
                    var softColor = Diagram.Data.Settings.GridLinesColor;
                    var hardColor = Diagram.Data.Settings.GridLinesColorSecondary;
                    //var softColor = new Color(1f, 1f, 0f, 0.1f);
                    //var hardColor = new Color(1f, 1f, 1f, 0.3f);
                    var x = 0f;
                    var every10 = 0;

                    while (x < diagramRect.width + _scrollPosition.x)
                    {
                        Handles.color = softColor;
                        if (every10 == 10)
                        {
                            Handles.color = hardColor; 
                            every10 = 0;
                        }
                        Handles.DrawLine(new Vector2(x, 0f), new Vector2(x, Screen.height + _scrollPosition.y));
                        x += Diagram.Data.Settings.SnapSize * ElementDesignerStyles.Scale;
                        every10++;
                    }
                    var y = 3f;
                    every10 = 0;
                    while (y < diagramRect.height + _scrollPosition.y)
                    {
                        Handles.color = softColor;
                        if (every10 == 10)
                        {
                            Handles.color = hardColor;
                            every10 = 0;
                        }
                        Handles.DrawLine(new Vector2(0, y), new Vector2(Screen.width + _scrollPosition.x, y));
                        y += Diagram.Data.Settings.SnapSize * ElementDesignerStyles.Scale;
                        every10++;
                    }
                }
                //BeginGUI();
                Diagram.Draw();

                //EndGUI();
                GUI.EndScrollView();
                GUILayout.Space(diagramRect.height);
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                //DoToolbar(diagramRect);
                Toolbar.GoBottom();
                GUILayout.EndHorizontal();
            }

            if (EditorApplication.isCompiling)
            {
                InfoBox("Compiling.. Please Wait!");
            }
            if (Diagram != null && Diagram.Data != null)
            {
                var refactors = Diagram.Data.RefactorCount;
                if (refactors > 0)
                {
                    InfoBox(string.Format("You have {0} refactors. Save before recompiling occurs.", refactors), MessageType.Warning);
                }
            }
            LastEvent = Event.current;
            if (LastEvent != null)
            {
                if (LastEvent.type == EventType.keyUp)
                {
                    if (LastEvent.keyCode == KeyCode.LeftShift || LastEvent.keyCode == KeyCode.RightShift) ModifierKeyStates.Shift = false;
                    if (LastEvent.keyCode == KeyCode.LeftControl || LastEvent.keyCode == KeyCode.RightControl) ModifierKeyStates.Ctrl = false;
                    if (LastEvent.keyCode == KeyCode.LeftAlt || LastEvent.keyCode == KeyCode.RightAlt) ModifierKeyStates.Alt = false;
                }
            }
           
            if (LastEvent != null)
            {
                if (LastEvent.type == EventType.keyDown)
                {
                    if (LastEvent.keyCode == KeyCode.LeftShift || LastEvent.keyCode == KeyCode.RightShift)
                        ModifierKeyStates.Shift = true;
                    if (LastEvent.keyCode == KeyCode.LeftControl || LastEvent.keyCode == KeyCode.RightControl)
                        ModifierKeyStates.Ctrl = true;
                    if (LastEvent.keyCode == KeyCode.LeftAlt || LastEvent.keyCode == KeyCode.RightAlt)
                        ModifierKeyStates.Alt = true;
                }
               // Debug.Log(string.Format("Shift: {0}, Alt: {1}, Ctrl: {2}",ModifierKeyStates.Shift,ModifierKeyStates.Alt,ModifierKeyStates.Ctrl));
            }
            var evt = Event.current;
            if (evt != null && evt.isKey && evt.type == EventType.KeyUp && Diagram != null)
            {
              
                if (Diagram.SelectedData == null || !Diagram.SelectedData.IsEditing)
                {
                    
                    Diagram.HandleKeyEvent(evt,ModifierKeyStates);
                    evt.Use();
                }
            }
            if (Event.current.type == EventType.ValidateCommand &&
          Event.current.commandName == "UndoRedoPerformed")
            {
               
            }

            if (Diagram != null && Diagram.Dirty || EditorApplication.isCompiling)
            {
                Repaint();
            }
         
        }
   
        public void OnLostFocus()
        {
            if (Diagram == null) return;
            Diagram.IsMouseDown = false;
            Diagram.DeselectAll();
        }

        //public virtual void OpenDiagramByAttribute(Type type)
        //{
        //    var attribute = type.GetCustomAttributes(typeof(DiagramInfoAttribute), true).FirstOrDefault() as DiagramInfoAttribute;
        //    if (attribute == null) return;
        //    LoadDiagramByName(attribute.DiagramName);
        //}
       
        public void Update()
        {
            //if (Diagram == null) return;
           
            //if (Diagram.IsMouseDown || Diagram.Dirty || EditorApplication.isCompiling)
            //{
              
            //    Diagram.Dirty = false;
            //}
            Repaint();
        }


        //public void HandleInput()
        //{
        //    var e = Event.current;

        //    if (e.type == EventType.MouseDown)
        //    {

        //        InputManager.MouseDown();

        //        if (e.clickCount > 1)
        //        {
        //            InputManager.MouseDoubleClick();
        //        }
        //        e.Use();
        //    }
        //    else if (e.type == EventType.MouseUp)
        //    {
        //        InputManager.MouseUp();
        //    }
        //    else
        //    {
        //        var mp = (_scrollPosition + e.mousePosition) * (1f / ElementsDiagram.Scale);

        //        InputManager.MousePositionDelta = mp - InputManager.MousePosition;
        //        InputManager.MousePosition = mp;
        //        InputManager.MouseMove();
        //    }
        //    LastEvent = e;
        //    if (LastEvent != null)
        //    {
        //        if (LastEvent.type == EventType.keyUp)
        //        {
        //            if (LastEvent.keyCode == KeyCode.LeftShift || LastEvent.keyCode == KeyCode.RightShift) ModifierKeyStates.Shift = false;
        //            if (LastEvent.keyCode == KeyCode.LeftControl || LastEvent.keyCode == KeyCode.RightControl) ModifierKeyStates.Ctrl = false;
        //            if (LastEvent.keyCode == KeyCode.LeftAlt || LastEvent.keyCode == KeyCode.RightAlt) ModifierKeyStates.Alt = false;
        //        }
        //    }

        //    if (LastEvent != null)
        //    {
        //        if (LastEvent.type == EventType.keyDown)
        //        {
        //            if (LastEvent.keyCode == KeyCode.LeftShift || LastEvent.keyCode == KeyCode.RightShift)
        //                ModifierKeyStates.Shift = true;
        //            if (LastEvent.keyCode == KeyCode.LeftControl || LastEvent.keyCode == KeyCode.RightControl)
        //                ModifierKeyStates.Ctrl = true;
        //            if (LastEvent.keyCode == KeyCode.LeftAlt || LastEvent.keyCode == KeyCode.RightAlt)
        //                ModifierKeyStates.Alt = true;
        //        }
        //        // Debug.Log(string.Format("Shift: {0}, Alt: {1}, Ctrl: {2}",ModifierKeyStates.Shift,ModifierKeyStates.Alt,ModifierKeyStates.Ctrl));
        //    }
        //    var evt = Event.current;
        //    if (evt != null && evt.isKey && evt.type == EventType.KeyUp && Diagram != null)
        //    {
        //        InputManager.KeyPressed(evt.keyCode);


        //    }
        //}

        //public InputManager InputManager
        //{
        //    get
        //    {
        //        return Diagram.InputManager;
        //    }
        //}

        public Vector2 LastMousePosition { get; set; }
        private void DoToolbar()
        {
            try
            {
                if (
                    GUILayout.Button(
                        new GUIContent(Diagram == null || Diagram.Data == null ? "--Select Diagram--" : Diagram.Data.Name),
                        EditorStyles.toolbarPopup))
                {
                    SelectDiagram();
                }
            }
            catch (Exception ex)
            {
                Diagram = null;
            
                Repaint();
                return;

            }
            Toolbar.Go();
        }

        private void HandlePanning(Rect diagramRect)
        {
            if (Event.current.button == 2 && Event.current.type == EventType.MouseDown)
            {
                IsMiddleMouseDown = true;
                PanStartPosition = Event.current.mousePosition;
            }
            if (Event.current.button == 2 && Event.current.rawType == EventType.MouseUp && IsMiddleMouseDown)
            {
                IsMiddleMouseDown = false;
            }
            if (IsMiddleMouseDown)
            {
                var delta = PanStartPosition - Event.current.mousePosition;
                _scrollPosition += delta;
                if (_scrollPosition.x < 0)
                    _scrollPosition.x = 0;
                if (_scrollPosition.y < 0)
                    _scrollPosition.y = 0;
                if (_scrollPosition.x > diagramRect.width - diagramRect.x)
                {
                    _scrollPosition.x = diagramRect.width - diagramRect.x;
                }
                if (_scrollPosition.y > diagramRect.height - diagramRect.y)
                {
                    _scrollPosition.y = diagramRect.height - diagramRect.y;
                }
                Repaint();
            }

        }

        public bool LoadDiagram(string path)
        {
            try
            {
                //Undo.undoRedoPerformed = UndoRedoPerformed;
                Undo.undoRedoPerformed += UndoRedoPerformed;
                //Diagram = uFrameEditor.Container.Resolve<ElementsDiagram>();
                Diagram = new ElementsDiagram(path);
                //Diagram.SelectionChanged += DiagramOnSelectionChanged;
                LastLoadedDiagram = path;
                Diagram.Dirty = true;
                Diagram.Data.ApplyFilter();
                Diagram.Refresh(true);
                this.Repaint();
            }
            catch (Exception ex)
            {
#if DEBUG
                UnityEngine.Debug.LogException(ex);
#endif
                Debug.Log("Either a plugin isn't installed or the file could no longer be found.");
                LastLoadedDiagram = null;
                return false;
            }
            return true;
            // var newScrollPosition = new Vector2(Diagram.DiagramSize.width, Diagram.DiagramSize.height).normalized / 2;
            //_scrollPosition = new Vector2(250,250);
        }

        private void UndoRedoPerformed()
        {
            Diagram = null;
            Repaint();
        }

        public void LoadDiagramByName(string diagramName)
        {
            var repos = uFrameEditor.Container.ResolveAll<IElementsDataRepository>();
            var diagrams = repos.SelectMany(p => p.GetProjectDiagrams()).ToDictionary(p => p.Key, p => p.Value);
            if (diagrams.ContainsKey(diagramName))
                LoadDiagram(diagrams[diagramName]);

            //var diagram = UFrameAssetManager.Diagrams.FirstOrDefault(p => p.Name == diagramName);
            //if (diagram == null) return;
            //LoadDiagram(diagram);
        }

        private void SelectDiagram()
        {
            var repositories = uFrameEditor.Container.ResolveAll<IElementsDataRepository>().ToArray();
            var diagramNames = repositories.SelectMany(p => p.GetProjectDiagrams().Keys).ToArray();
            var diagramPaths = repositories.SelectMany(p => p.GetProjectDiagrams().Values).ToArray();


            var menu = new GenericMenu();
            for (int index = 0; index < diagramNames.Length; index++)
            {
                var diagramName = diagramNames[index];
                var diagram = diagramPaths[index];

                menu.AddItem(new GUIContent(diagramName), Diagram != null && diagram == LastLoadedDiagram, () =>
                {
                    LastLoadedDiagram = diagramName;
                    LoadDiagram(diagram);

                });
            }
            menu.ShowAsContext();
        }

        public IEnumerable<object> ContextObjects
        {
            get
            {
                yield return this;
                if (Diagram != null)
                    yield return Diagram;
                if (Diagram != null && Diagram.Data != null)
                    yield return Diagram.Data;
            }
        }

        public Event LastEvent { get; set; }

        public void CommandExecuted(IEditorCommand command)
        {
            if (Diagram != null)
            {
                Diagram.Refresh();
            }
            Repaint();
        }

        public void CommandExecuting(IEditorCommand command)
        {

        }
    }
}

public class ModifierKeyState
{
    public bool Shift { get; set; }
    public bool Alt { get; set; }
    public bool Ctrl { get; set; }
}
public class InputManager
{
    //private Stack<IInputHandler> _inputHandlerStack = new Stack<IInputHandler>();

    private INodeDrawer[] _selected;

    //public IInputHandler CurrentHandler
    //{
    //    get
    //    {
    //        return InputHandlerStack.Peek();
    //    }
    //}

    public ElementsDiagram Diagram { get; set; }

    public INodeDrawer DrawerAtMouse { get; set; }

    public bool IsMouseDown { get; set; }

    public bool IsMultiSelecting
    {
        get;
        set;
    }

    public Vector2 LastMouseDownPosition { get; set; }

    public ModifierKeyState ModifierKeyState { get; set; }

    public Vector2 MousePosition { get; set; }

    public Vector2 MousePositionDelta { get; set; }

    public ICommandHandler CommandHandler { get; set; }

    public INodeDrawer[] Selected
    {
        get { return _selected; }
        set
        {
            if (_selected != null)
                foreach (var item in value)
                    item.OnDeselecting(this);

            if (value != null)
                foreach (var item in value)
                    item.OnSelecting(this);

            var old = _selected;
            _selected = value;

            if (old != null)
                foreach (var item in old)
                    item.OnDeselected(this);

            if (value != null)
                foreach (var item in value)
                {
                    item.IsSelected = true;
                    item.OnSelected(this);
                }
        }
    }
    public Rect SelectionRect { get; set; }
    public bool CancelBubble { get; set; }

    //public IEnumerable<ListItemControl> SelectedListItems
    //{
    //    get { return Selected.OfType<ListItemControl>(); }
    //}

    //public IEnumerable<IDiagramNodeItem> SelectedItemDatas
    //{
    //    get { return SelectedListItems.Select(p => p.NodeItemData); }
    //}

    //public IDiagramNodeItem SelectedItemData
    //{
    //    get { return SelectedItemDatas.FirstOrDefault(); }
    //}


    //protected Stack<IInputHandler> InputHandlerStack
    //{
    //    get { return _inputHandlerStack; }
    //    set { _inputHandlerStack = value; }
    //}

    public virtual void BubbleMouseEvent(Action<INodeDrawer> eventAction)
    {
        
        //CancelBubble = false;
        //var controls = GetControlsAtMouse(Diagram.Children);
        //foreach (var control in controls)
        //{
        //    if (CancelBubble) break;
        //    eventAction(control);
        //}

    }

    public virtual void KeyPressed(KeyCode keyCode)
    {
        var bindings = uFrameEditor.KeyBindings;
        var keyStates = ModifierKeyState;
        foreach (var keyBinding in bindings)
        {
            if (keyBinding.Key != keyCode)
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
                var acceptableArguments = CommandHandler.ContextObjects.Where(p => command.For.IsAssignableFrom(p.GetType())).ToArray();
                foreach (var argument in acceptableArguments)
                {
                    if (command.CanPerform(argument) == null)
                    {
#if DEBUG
                        UnityEngine.Debug.Log("Key Command Executed: " + command.GetType().Name);
#endif
                        CommandHandler.ExecuteCommand(command);
                        return;
                    }
                }

            }
        }
    }
    public virtual void MouseDoubleClick()
    {

        var control = GetControlAtMouse() as INodeDrawer;

        if (control != null && control.ViewModelObject.DataObject is IDiagramFilter)
        {
            var selectedData = control.ViewModelObject.DataObject;

            if (selectedData == Diagram.Data.CurrentFilter)
            {
                Diagram.Data.PopFilter(null);
                Diagram.Refresh();
            }
            else
            {
                Diagram.Data.PushFilter(selectedData as IDiagramFilter);
                Diagram.Refresh();
            }
        }
    }

    public INodeDrawer GetControlAtMouse()
    {
        return Diagram.NodeDrawers.FirstOrDefault(p => p.Bounds.Contains(MousePosition));
        //return GetControlsAtMouse(Diagram.NodeDrawers).FirstOrDefault();
    }

    //public IEnumerable<INodeDrawer> GetControlsAtMouse(IEnumerable<INodeDrawer> controls)
    //{
    //    foreach (var control in controls)
    //    {
    //        if (control.Bounds.Contains(MousePosition))
    //        {
    //            return control;
    //            var container = control as IUFContainerControl;
    //            // If its a container control then search its children
    //            if (container != null)
    //            {
    //                var result = GetControlsAtMouse(container.Children);
    //                foreach (var item in result)
    //                    yield return item;


    //            }
    //            yield return control;
    //        }
    //    }
    //}

    public virtual void MouseDown()
    {
        IsMouseDown = true;

        //ConnectableAtMouse = Diagram.Connectables.FirstOrDefault(p => p.Position().Contains(MousePosition));

        // Make sure we update the drawer that is at the mouse position
        DrawerAtMouse = GetControlAtMouse();

        var selected = DrawerAtMouse as INodeDrawer;

        // If we've clicked on something and there isn't a current selection
        if (selected != null && Selected.Length <= 1)
        {
            Selected = new[] { selected };
        }
        else if (DrawerAtMouse == null) // If we hit a empty area
        {
            Selected = new INodeDrawer[] { };
            IsMultiSelecting = true;
        }
        if (DrawerAtMouse != null)
        {

            //BubbleMouseEvent((c) => c.OnMouseDown(this));
        }
        LastMouseDownPosition = MousePosition;
    }

    public virtual void MouseMove()
    {
        var oldDrawerAtMouse = DrawerAtMouse;
        DrawerAtMouse = GetControlAtMouse();

        // Handle Mouse Enter & Mouse Exit
        if (oldDrawerAtMouse != DrawerAtMouse)
        {
            if (oldDrawerAtMouse != null)
                oldDrawerAtMouse.OnMouseExit(this);

            //BubbleMouseEvent(p=>p.OnMouseExit(this));
        }
        if (DrawerAtMouse != oldDrawerAtMouse)
        {
            if (DrawerAtMouse != null)
                DrawerAtMouse.OnMouseEnter(this);
            //BubbleMouseEvent(p => p.OnMouseEnter(this));
        }
        if (DrawerAtMouse != null)
            DrawerAtMouse.OnMouseMove(this);
        //BubbleMouseEvent(p => p.OnMouseMove(this));

        if (IsMultiSelecting)
        {
            HandleSelectionRect();
        }
        else if (IsMouseDown && Selected != null && Selected.Length > 0)
        {
            //Debug.Log("Moving");
            foreach (var item in Selected)
            {
                //(delta * (1f / ElementsDiagram.Scale));
                item.OnDrag(this);
            }
        }
    }

    public virtual void MouseUp()
    {
        DrawerAtMouse = GetControlAtMouse();

        if (IsMultiSelecting)
        {
            Selected =
                Diagram.NodeDrawers.Where(p => SelectionRect.Overlaps(p.Bounds)).ToArray();
        }
        else
        {
            if (DrawerAtMouse != null && IsMouseDown)
            {
                DrawerAtMouse.OnMouseUp();
            }
            //BubbleMouseEvent(_ => _.OnMouseUp(this));
        }

        IsMouseDown = false;
        IsMultiSelecting = false;
        SelectionRect = new Rect(0f, 0f, 0f, 0f);
    }

    private void HandleSelectionRect()
    {
        var cur = MousePosition;
        if (cur.x > LastMouseDownPosition.x)
        {
            if (cur.y > LastMouseDownPosition.y)
            {
                SelectionRect = new Rect(LastMouseDownPosition.x, LastMouseDownPosition.y,
                    cur.x - LastMouseDownPosition.x, cur.y - LastMouseDownPosition.y);
            }
            else
            {
                SelectionRect = new Rect(
                    LastMouseDownPosition.x, cur.y, cur.x - LastMouseDownPosition.x, LastMouseDownPosition.y - cur.y);
                //SelectionRect = new Rect(LastMouseDownPosition.x, LastMouseDownPosition.y,
                //    cur.x - LastMouseDownPosition.x, cur.y - LastMouseDownPosition.y);
            }
        }
        else
        {
            if (cur.y > LastMouseDownPosition.y)
            {
                // x is less and y is greater
                SelectionRect = new Rect(
                    cur.x, LastMouseDownPosition.y, LastMouseDownPosition.x - cur.x, cur.y - LastMouseDownPosition.y);
            }
            else
            {
                // both are less
                SelectionRect = new Rect(
                    cur.x, cur.y, LastMouseDownPosition.x - cur.x, LastMouseDownPosition.y - cur.y);
            }
            //SelectionRect = new Rect(LastMouseDownPosition.x, LastMouseDownPosition.y,
            //   LastMouseDownPosition.x - cur.x, LastMouseDownPosition.y - cur.y);
        }
    }

    public void DeselectAll()
    {
        Selected = new INodeDrawer[] { };
    }
}
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
        private ElementsDiagram _diagramDrawer;

        private Vector2 _scrollPosition;
        private ICommandUI _toolbar;
        private ModifierKeyState _modifierKeyStates;
        private MouseEvent _mouseEvent;


        public static INodeRepository SelectedElementDiagram
        {
            get { return Selection.activeObject as IElementDesignerData; }
        }


        public ElementsDiagram DiagramDrawer
        {
            get { return _diagramDrawer; }
            set
            {
                _diagramDrawer = value;
                _toolbar = null;
            }
        }

        public DiagramViewModel DiagramViewModel 
        {
            get
            {
                if (DiagramDrawer == null) return null;
                return DiagramDrawer.DiagramViewModel;
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

            if ((DiagramViewModel == null) && !string.IsNullOrEmpty(LastLoadedDiagram))
            {
                DiagramDrawer = null;
                if (!LoadDiagram(LastLoadedDiagram))
                {
                    LastLoadedDiagram = null;
                    return;
                }
                if (DiagramDrawer == null)
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
            if (DiagramDrawer == null) return;
            var diagramRect = new Rect(0f, (EditorStyles.toolbar.fixedHeight) - 1, Screen.width - 3, Screen.height - (EditorStyles.toolbar.fixedHeight * 2) - EditorStyles.toolbar.fixedHeight - 2);
            DiagramDrawer.Rect = diagramRect;
            EditorGUI.DrawRect(diagramRect, uFrameEditor.Settings.BackgroundColor);
            GUI.Box(diagramRect, string.Empty, style);

            if (DiagramDrawer == null)
            {
            }
            else
            {
                if (Event.current.control && Event.current.type == EventType.mouseDown)
                {
                }
                _scrollPosition = GUI.BeginScrollView(diagramRect, _scrollPosition, DiagramDrawer.DiagramSize);

                HandlePanning(diagramRect);
                if (DiagramViewModel != null)
                {
                    var softColor = uFrameEditor.Settings.GridLinesColor;
                    var hardColor = uFrameEditor.Settings.GridLinesColorSecondary;
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
                        x += DiagramViewModel.Settings.SnapSize * ElementDesignerStyles.Scale;
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
                        y += DiagramViewModel.Settings.SnapSize * ElementDesignerStyles.Scale;
                        every10++;
                    }
                }
                //BeginGUI();
                DiagramDrawer.Draw(ElementDesignerStyles.Scale);
                HandleInput();

//#if DEBUG
//                GUILayout.BeginArea(new Rect(10f, 70f, 500f, 500f));
//                GUILayout.Label(string.Format("Mouse Position: x = {0}, y = {1}", MouseEvent.MousePosition.x, MouseEvent.MousePosition.y));
//                GUILayout.Label(string.Format("Mouse Position Delta: x = {0}, y = {1}", MouseEvent.MousePositionDelta.x, MouseEvent.MousePositionDelta.y));
//                GUILayout.Label(string.Format("Mouse Down: {0}", MouseEvent.IsMouseDown));
//                GUILayout.Label(string.Format("Last Mouse Down Position: {0}", MouseEvent.LastMousePosition));
//                if (DiagramDrawer != null)
//                {

//                    GUILayout.Label(string.Format("Drawer Count: {0}", DiagramDrawer.DiagramViewModel.GraphItems.Count));
//                    if (DiagramDrawer.DrawersAtMouse != null)
//                        foreach (var drawer in DiagramDrawer.DrawersAtMouse)
//                        {
//                            GUILayout.Label(drawer.ToString());
//                        }
//                    if (DiagramDrawer.DiagramViewModel != null)
//                        foreach (var drawer in DiagramDrawer.DiagramViewModel.SelectedGraphItems)
//                        {
//                            GUILayout.Label(drawer.ToString());
//                        }
//                }

//                GUILayout.EndArea();
//#endif
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
            if (DiagramViewModel != null)
            {
                var refactors = DiagramViewModel.RefactorCount;
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
            if (evt != null && evt.isKey && evt.type == EventType.KeyUp && DiagramDrawer != null)
            {
              
                if (DiagramViewModel != null && (DiagramViewModel.SelectedNode == null || !DiagramViewModel.SelectedNode.IsEditing))
                {
                    if (DiagramDrawer.HandleKeyEvent(evt, ModifierKeyStates))
                    {
                        evt.Use();
                    }
                    
                }
            }



            if (Event.current.type == EventType.ValidateCommand &&
          Event.current.commandName == "UndoRedoPerformed")
            {
               
            }

            if (DiagramDrawer != null && DiagramDrawer.Dirty || EditorApplication.isCompiling)
            {
                
            }
         
        }
   
        public void OnLostFocus()
        {
            if (DiagramViewModel != null)
            DiagramViewModel.DeselectAll();

            MouseEvent.IsMouseDown = false;
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

        public MouseEvent MouseEvent
        {
            get { return _mouseEvent ?? (_mouseEvent = new MouseEvent(ModifierKeyStates,DiagramDrawer)); }
            set { _mouseEvent = value; }
        }

        private Vector2 _mousePosition;

        
        public void HandleInput()
        {
            if (DiagramDrawer == null) return;

            var e = Event.current;
            if (e == null)
            {
                return;
            }
            var handler = MouseEvent.CurrentHandler;

         
            if (e.type == EventType.MouseDown)
            {
                MouseEvent.MouseDownPosition = MouseEvent.MousePosition;
                MouseEvent.IsMouseDown = true;
                MouseEvent.MouseButton = e.button;
                handler.OnMouseDown(MouseEvent);
                if (e.button == 1)
                {
                    handler.OnRightClick(MouseEvent);
                }
 
                if (e.clickCount > 1)
                {
                    handler.OnMouseDoubleClick(MouseEvent);
               }
                LastMouseDownEvent = e;
            }
            if (e.rawType == EventType.MouseUp)
            {
                MouseEvent.MouseUpPosition = MouseEvent.MousePosition;
                MouseEvent.IsMouseDown = false;
                handler.OnMouseUp(MouseEvent);
            }
            else if (e.rawType == EventType.KeyDown)
            {
                
            }
            else
            {
                var mp = ( e.mousePosition) * (1f / ElementsDiagram.Scale);
            
                MouseEvent.MousePosition = mp;
                MouseEvent.MousePositionDelta = MouseEvent.MousePosition - MouseEvent.LastMousePosition;

                handler.OnMouseMove(MouseEvent);
                MouseEvent.LastMousePosition = mp;
            }
          
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

        //public 
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
                      new GUIContent(uFrameEditor.CurrentProject == null ? "--Select Project--" : uFrameEditor.CurrentProject.Name),
                      EditorStyles.toolbarPopup))
                {
                    SelectProject();
                }
                if (uFrameEditor.CurrentProject != null)
                {
                    if (
                    GUILayout.Button(
                        new GUIContent(DiagramViewModel == null ? "--Select Diagram--" : DiagramViewModel.Title),
                        EditorStyles.toolbarPopup))
                    {
                        SelectDiagram();
                    }
                }
                
            }
            catch (Exception ex)
            {
                DiagramDrawer = null;
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
                
            }

        }

        public bool LoadDiagram(string path)
        {
            try
            {
                //Undo.undoRedoPerformed = UndoRedoPerformed;
                Undo.undoRedoPerformed += UndoRedoPerformed;
                //Diagram = uFrameEditor.Container.Resolve<ElementsDiagram>();
                DiagramDrawer = new ElementsDiagram(new DiagramViewModel(path, uFrameEditor.CurrentProject));
                MouseEvent = new MouseEvent(ModifierKeyStates,DiagramDrawer);
                //Diagram.SelectionChanged += DiagramOnSelectionChanged;
                LastLoadedDiagram = path;
                DiagramDrawer.Dirty = true;
                //DiagramDrawer.Data.ApplyFilter();
                DiagramDrawer.Refresh();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                Debug.Log("Either a plugin isn't installed or the file could no longer be found. See Exception error");
                LastLoadedDiagram = null;
                return false;
            }
            return true;
            // var newScrollPosition = new Vector2(Diagram.DiagramSize.width, Diagram.DiagramSize.height).normalized / 2;
            //_scrollPosition = new Vector2(250,250);
        }

        private void UndoRedoPerformed()
        {
            DiagramDrawer = null;
       
        }

        public void LoadDiagramByName(string diagramName)
        {
            var repos = uFrameEditor.Container.ResolveAll<IProjectRepository>();
            var diagrams = repos.SelectMany(p => p.GetProjectDiagrams()).ToDictionary(p => p.Key, p => p.Value);
            if (diagrams.ContainsKey(diagramName))
                LoadDiagram(diagrams[diagramName]);

            //var diagram = UFrameAssetManager.Diagrams.FirstOrDefault(p => p.Name == diagramName);
            //if (diagram == null) return;
            //LoadDiagram(diagram);
        }
        private void SelectProject()
        {
            var projects = uFrameEditor.Projects;


            var menu = new GenericMenu();
            foreach (var project in projects)
            {
                ProjectRepository project1 = project;
                menu.AddItem(new GUIContent(project.name), project == uFrameEditor.CurrentProject, () =>
                {
                    uFrameEditor.CurrentProject = project1;
                    uFrameEditor.CurrentProject.CurrentDiagram = uFrameEditor.CurrentProject.Diagrams.FirstOrDefault();
                });
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Force Refresh"), false, () => { uFrameEditor.Projects = null; });
            menu.ShowAsContext();
        }
        private void SelectDiagram()
        {
            var projectDiagrams = uFrameEditor.CurrentProject.GetProjectDiagrams();
            var diagramNames = projectDiagrams.Keys.ToArray();
            var diagramPaths = projectDiagrams.Values.ToArray();


            var menu = new GenericMenu();
            for (int index = 0; index < diagramNames.Length; index++)
            {
                var diagramName = diagramNames[index];
                var diagram = diagramPaths[index];

                menu.AddItem(new GUIContent(diagramName), DiagramDrawer != null && diagram == LastLoadedDiagram, () =>
                {
                    LastLoadedDiagram = diagramName;
                    LoadDiagram(diagram);

                });
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Force Refresh"),false,()=> { uFrameEditor.CurrentProject.Refresh(); } );
            menu.ShowAsContext();
        }

        public IEnumerable<object> ContextObjects
        {
            get
            {
                yield return this;
                if (DiagramDrawer != null)
                {
                    foreach (var co in DiagramDrawer.ContextObjects)
                    {
                        yield return co;
                    }
                }
            }
        }

        public Event LastEvent { get; set; }
        public Event LastMouseDownEvent { get; set; }
        public void CommandExecuted(IEditorCommand command)
        {
            if (DiagramDrawer != null)
            {
                DiagramDrawer.Refresh();
            }
            
        }

        public void CommandExecuting(IEditorCommand command)
        {

        }
    }
}

public class MouseEvent
{
    private Stack<IInputHandler> _inputHandlers;
    public ModifierKeyState ModifierKeyStates { get; set; }

    public MouseEvent(ModifierKeyState modifierKeyStates,IInputHandler defaultHandler)
    {
        ModifierKeyStates = modifierKeyStates;
        DefaultHandler = defaultHandler;
    }

    public void Begin(IInputHandler handler)
    {
        if (handler != null)
        {
            InputHandlers.Push(handler);
        }
    }
    public void Cancel()
    {
        if (this.InputHandlers.Count > 0)
            this.InputHandlers.Pop();
    }
    public IInputHandler CurrentHandler
    {
        get
        {
            if (InputHandlers.Count < 1)
                return DefaultHandler;
            return InputHandlers.Peek();
        }
    }
    public Stack<IInputHandler> InputHandlers
    {
        get { return _inputHandlers ?? (_inputHandlers = new Stack<IInputHandler>()); }
        set { _inputHandlers = value; }
    }

    public Vector2 MousePosition { get; set; }
    public bool IsMouseDown { get; set; }
    public Vector2 LastMousePosition { get; set; }
    public Vector2 MouseDownPosition { get; set; }
    public Vector2 MouseUpPosition { get; set; }
    public Vector2 MousePositionDelta { get; set; }

    public IInputHandler DefaultHandler { get; set; }
    public int MouseButton { get; set; }
    public bool NoBubble { get; set; }
}

public class ModifierKeyState
{
    public bool Shift { get; set; }
    public bool Alt { get; set; }
    public bool Ctrl { get; set; }
}

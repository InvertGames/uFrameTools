using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using UnityEngine;

namespace Invert.Core.GraphDesigner
{

    public class DiagramDrawer : Drawer, IInputHandler
    {
        public delegate void SelectionChangedEventArgs(IDiagramNode oldData, IDiagramNode newData);
        public event SelectionChangedEventArgs SelectionChanged;


        private IDrawer _nodeDrawerAtMouse;
        private SelectionRectHandler _selectionRectHandler;
        private IDrawer[] _cachedChildren = new IDrawer[] { };
        private Dictionary<IDiagramFilter, Vector2> _cachedPaths;


        public static float Scale
        {
            get { return InvertGraphEditor.CurrentDiagramViewModel.Scale; }
        }



        public IDiagramNode CurrentMouseOverNode { get; set; }

        public DiagramViewModel DiagramViewModel
        {
            get { return this.DataContext as DiagramViewModel; }
            set { this.DataContext = value; }
        }


        public Rect Rect { get; set; }



        public DiagramDrawer(DiagramViewModel viewModel)
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

        public void DrawBreadcrumbs(IPlatformDrawer platform,  float y)
        {
            var rect = new Rect(0, y, Bounds.width, 40f);
            var color = new Color(InvertGraphEditor.Settings.BackgroundColor.r * 0.8f, InvertGraphEditor.Settings.BackgroundColor.g * 0.8f, InvertGraphEditor.Settings.BackgroundColor.b * 0.8f, 1f);

            platform.DrawRect(rect, color);
            var lineRect = new Rect(rect);
            lineRect.height = 2;
            lineRect.y = y + 38f;
            platform.DrawRect(lineRect, new Color(InvertGraphEditor.Settings.BackgroundColor.r * 0.6f, InvertGraphEditor.Settings.BackgroundColor.g * 0.6f, InvertGraphEditor.Settings.BackgroundColor.b * 0.6f, 1f));
            var x = 15f;
            var first = true;
            if (_cachedPaths != null)
            foreach (var item in _cachedPaths)
            {
                var item1 = item;
                platform.DoButton(new Rect(x, rect.y + 20 - (item.Value.y / 2), item.Value.x, item.Value.y), first ? item.Key.Name : "< " + item.Key.Name, first ? CachedStyles.GraphTitleLabel : CachedStyles.ItemTextEditingStyle,
                    () =>
                    {
                        InvertApplication.Execute(new LambdaCommand(() =>
                        {
                            DiagramViewModel.GraphData.PopToFilter(item1.Key);
                        }));
                    });
                x += item.Value.x + 15;
                first = false;
            }
        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {

            


        
            //var x = rect.x + 10;

            //foreach (var item in DiagramDrawer.DiagramViewModel.GraphData.GetFilterPath())
            //{
            //    var item1 = item;
            //    var size = drawer.CalculateSize(item.Name, CachedStyles.GraphTitleLabel);
            //    x += size.x + 10;
            //}
            // Make sure they've upgraded to the latest json format
#if UNITY_DLL
            if (UpgradeOldProject()) return;
#endif
            //// Draw the title box
            //GUI.Box(new Rect(0, 0f, DiagramSize.width, 30f), DiagramViewModel.Title , ElementDesignerStyles.DiagramTitle);

            if (DiagramViewModel.LastMouseEvent != null)
            {
                var handler = DiagramViewModel.LastMouseEvent.CurrentHandler;
                if (!(handler is DiagramDrawer))
                    handler.Draw(platform, scale);
            }
            // Draw all of our drawers
            foreach (var drawer in _cachedChildren)
            {
                if (drawer.Dirty)
                {
                    drawer.Refresh((IPlatformDrawer)platform,drawer.Bounds.position,false);
                    drawer.Dirty = false;
                }
                drawer.Draw(platform, Scale);

            }
          //  platform.DrawLabel(new Rect(5f, 5f, 200f, 100f), DiagramViewModel.Title, CachedStyles.GraphTitleLabel);


            DrawErrors();
            DrawHelp();
        }
//        //TODO move this to platform specific operation
//#if UNITY_DLL
//        public bool HandleKeyEvent(Event evt, ModifierKeyState keyStates)
//        {
//            var bindings = InvertGraphEditor.KeyBindings;
//            foreach (var keyBinding in bindings)
//            {
//                if (keyBinding.Key != evt.keyCode)
//                {
//                    continue;
//                }
//                if (keyBinding.RequireAlt && !keyStates.Alt)
//                {
//                    continue;
//                }
//                if (keyBinding.RequireShift && !keyStates.Shift)
//                {
//                    continue;
//                }
//                if (keyBinding.RequireControl && !keyStates.Ctrl)
//                {
//                    continue;
//                }

//                var command = keyBinding.Command;
//                if (command != null)
//                {
//                    if (command.CanExecute(InvertGraphEditor.DesignerWindow) == null)
//                    {
//                        InvertGraphEditor.ExecuteCommand(command);
//                    }
//                    else
//                    {
//                        return false;
//                    }

//                    return true;
//                }
//                return false;
//            }
//            return false;
//        }
//#endif
        public override void OnMouseDoubleClick(MouseEvent mouseEvent)
        {
           
            DiagramViewModel.LastMouseEvent = mouseEvent;
            if (DrawersAtMouse == null)
            {

                DrawersAtMouse = GetDrawersAtPosition(this, mouseEvent.MousePosition).ToArray();
            }
            base.OnMouseDoubleClick(mouseEvent);
            if (DrawersAtMouse.Length < 1)
            {
                if (mouseEvent.ModifierKeyStates.Alt)
                {
                    DiagramViewModel.ShowContainerDebug();
                }
                else
                {
                    DiagramViewModel.ShowQuickAdd();
                }

                return;
            }
            if (!BubbleEvent(d => d.OnMouseDoubleClick(mouseEvent), mouseEvent))
            {

                return;
            }
            else
            {
               
            }
            DiagramViewModel.Navigate();
            Refresh((IPlatformDrawer)InvertGraphEditor.PlatformDrawer);
            Refresh((IPlatformDrawer)InvertGraphEditor.PlatformDrawer);
     

        }

        public override void OnMouseEnter(MouseEvent e)
        {
            base.OnMouseEnter(e);
            BubbleEvent(d => d.OnMouseEnter(e), e);
        }
        public override void OnMouseExit(MouseEvent e)
        {
            base.OnMouseExit(e);
            DiagramViewModel.LastMouseEvent = e;
            BubbleEvent(d => d.OnMouseExit(e), e);
        }

        public override void OnMouseDown(MouseEvent mouseEvent)
        {
            base.OnMouseDown(mouseEvent);
            DiagramViewModel.LastMouseEvent = mouseEvent;
            if (DrawersAtMouse == null) return;
            if (!DrawersAtMouse.Any())
            {
                DiagramViewModel.NothingSelected();
                if (mouseEvent.ModifierKeyStates.Ctrl)
                {
                    DiagramViewModel.ShowQuickAdd();
                }
                mouseEvent.Begin(SelectionRectHandler);
            }
            else
            {
                BubbleEvent(d => d.OnMouseDown(mouseEvent), mouseEvent);
            }
        }

        public bool BubbleEvent(Action<IDrawer> action, MouseEvent e)
        {
            if (DrawersAtMouse == null) return true;

            foreach (var item in DrawersAtMouse.OrderByDescending(p => p.ZOrder))
            {
               // if (!item.Enabled) continue;
                action(item);
                if (e.NoBubble)
                {
                    e.NoBubble = false;
                    return false;

                    break;
                }
            }
            return true;
        }

        public override void OnMouseMove(MouseEvent e)
        {
            base.OnMouseMove(e);
            DiagramViewModel.LastMouseEvent = e;
            if (e.IsMouseDown && e.MouseButton == 0 && !e.ModifierKeyStates.Any)
            {
                foreach (var item in Children.OfType<DiagramNodeDrawer>())
                {
                    if (item.ViewModelObject.IsSelected)
                    {
#if UNITY_DLL
                        if (DiagramViewModel.Settings.Snap)
                        {
                            item.ViewModel.Position += e.MousePositionDeltaSnapped;
                            item.ViewModel.Position = item.ViewModel.Position.Snap(DiagramViewModel.Settings.SnapSize);
                        }
                        else
                        {
#endif
                            item.ViewModel.Position += e.MousePositionDelta;
#if UNITY_DLL
                        }
#endif
                        if (item.ViewModel.Position.x < 0)
                        {
                            item.ViewModel.Position = new Vector2(0f, item.ViewModel.Position.y);
                        }
                        if (item.ViewModel.Position.y < 0)
                        {
                            item.ViewModel.Position = new Vector2(item.ViewModel.Position.x, 0f);
                        }
                        item.Dirty = true;
                        //item.Refresh((IPlatformDrawer)InvertGraphEditor.PlatformDrawer,item.Bounds.position,false);
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

        //public static MouseEvent LastMouseEvent { get; set; }

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
            DiagramViewModel.LastMouseEvent = mouseEvent;
            BubbleEvent(d => d.OnMouseUp(mouseEvent), mouseEvent);

        }

        public override void OnRightClick(MouseEvent mouseEvent)
        {
            DiagramViewModel.LastMouseEvent = mouseEvent;
            BubbleEvent(d => d.OnRightClick(mouseEvent), mouseEvent);
            if (DrawersAtMouse == null)
            {
                ShowAddNewContextMenu(mouseEvent);
                return;

            }
            //var item = DrawersAtMouse.OrderByDescending(p=>p.ZOrder).FirstOrDefault();
            IDrawer item = DrawersAtMouse.OfType<ConnectorDrawer>().FirstOrDefault();
            if (item != null)
            {
                InvertApplication.SignalEvent<IShowContextMenu>(_=>_.Show(mouseEvent,item.ViewModelObject));
                return;
            }
            item = DrawersAtMouse.OfType<ItemDrawer>().FirstOrDefault();
            if (item != null)
            {
                if (item.Enabled)
                ShowItemContextMenu(mouseEvent);
                return;
            }
            item = DrawersAtMouse.OfType<DiagramNodeDrawer>().FirstOrDefault();
            if (item == null)
                item = DrawersAtMouse.OfType<HeaderDrawer>().FirstOrDefault();
            if (item != null)
            {
                if (!item.ViewModelObject.IsSelected)
                item.ViewModelObject.Select();
                ShowContextMenu(mouseEvent);
                return;
            }
            ShowAddNewContextMenu(mouseEvent);
        }

        public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        {
            base.Refresh(platform, position, hardRefresh);
            // Eventually it will all be viewmodels
            if (DiagramViewModel == null) return;
            Dictionary<IDiagramFilter, Vector2> dictionary = new Dictionary<IDiagramFilter, Vector2>();
            var first = true;
            foreach (var filter in new [] {DiagramViewModel.GraphData.RootFilter}.Concat(this.DiagramViewModel.GraphData.GetFilterPath()).Reverse())
            {
                var name = first ? filter.Name : "< " + filter.Name;
                dictionary.Add(filter, platform.CalculateSize(name, first ? CachedStyles.GraphTitleLabel : CachedStyles.ItemTextEditingStyle));
                first = false;
            }
                
            _cachedPaths = dictionary;

            Children.Clear();
            DiagramViewModel.Load();
            Children.Add(SelectionRectHandler);
            Dirty = true;
            _cachedChildren = Children.OrderBy(p => p.ZOrder).ToArray();
        }

        public void Save()
        {
            DiagramViewModel.Save();
        }

        public void ShowAddNewContextMenu(MouseEvent mouseEvent)
        {
            InvertApplication.SignalEvent<IShowContextMenu>(_ => _.Show(mouseEvent, DiagramViewModel));
        }

        public void ShowContextMenu(MouseEvent mouseEvent)
        {
            InvertApplication.SignalEvent<IShowContextMenu>(_ => _.Show(mouseEvent, DiagramViewModel.SelectedNode));
        }

        public void ShowItemContextMenu(MouseEvent mouseEvent)
        {
            InvertApplication.SignalEvent<IShowContextMenu>(_ => _.Show(mouseEvent, DiagramViewModel.SelectedNodeItem));
        }

        protected override void DataContextChanged()
        {
            base.DataContextChanged();
            DiagramViewModel.GraphItems.CollectionChanged += GraphItemsOnCollectionChangedWith;
        }



        protected virtual void OnSelectionChanged(IDiagramNode olddata, IDiagramNode newdata)
        {
            SelectionChangedEventArgs handler = SelectionChanged;
            if (handler != null) handler(olddata, newdata);
        }

        private static void DrawHelp()
        {
            // TODO implement platform stuff
#if UNITY_DLL
            //if (InvertGraphEditor.Settings.ShowHelp)
            //{
            //    var rect = new Rect(Screen.width - 275f, 10f, 250f, InvertGraphEditor.KeyBindings.Length * 20f);
            //    GUI.Box(rect, string.Empty);

            //    GUILayout.BeginArea(rect);
            //    foreach (var keyBinding in InvertGraphEditor.KeyBindings.Select(p => p.Name + ": " + p.ToString()).Distinct())
            //    {
            //        EditorGUILayout.LabelField(keyBinding);
            //    }
            //    EditorGUILayout.LabelField("Open Code: Ctrl+Click");
            //    GUILayout.EndArea();

            //}
#endif
        }

        private void DrawErrors()
        {
#if UNITY_DLL
            if (DiagramViewModel.HasErrors)
            {
                GUI.Box(Rect, DiagramViewModel.Errors.Message + Environment.NewLine + DiagramViewModel.Errors.StackTrace);
            }
#endif
        }

        //private void DrawSelectionRect(Rect selectionRect)
        //{
        //    if (selectionRect.width > 20 && selectionRect.height > 20)
        //    {
        //        foreach (var item in Children)
        //        {
        //            item.IsSelected = selectionRect.Overlaps(item.Bounds.Scale(Scale));
        //        }
        //        InvertGraphEditor.PlatformDrawer.DrawStretchBox(selectionRect,InvertStyles.BoxHighlighter4,12);
        //        //ElementDesignerStyles.DrawExpandableBox(selectionRect, ElementDesignerStyles.BoxHighlighter4, string.Empty);
        //    }
        //}

        public SelectionRectHandler SelectionRectHandler
        {
            get { return _selectionRectHandler ?? (_selectionRectHandler = new SelectionRectHandler(DiagramViewModel)); }
            set { _selectionRectHandler = value; }
        }

        public static bool IsEditingField { get; set; }

        //    if (CurrentEvent.keyCode == KeyCode.Return)
        //    {
        //        if (DiagramViewModel.SelectedNode != null && DiagramViewModel.SelectedNode.IsEditing)
        //        {
        //            DiagramViewModel.SelectedNode.EndEditing();
        private void GraphItemsOnCollectionChangedWith(object sender, NotifyCollectionChangedEventArgs e)
        {
            GraphItemsOnCollectionChangedWith(e);
        }
        private void GraphItemsOnCollectionChangedWith(NotifyCollectionChangedEventArgs changeArgs)
        {
            if (changeArgs.NewItems != null)
                foreach (var item in changeArgs.NewItems.OfType<ViewModel>())
                {
            
                    if (item == null) InvertApplication.Log("Graph Item is null");
                 
                    var drawer = InvertGraphEditor.Container.CreateDrawer<IDrawer>(item);
                    
                    if (drawer == null) InvertApplication.Log("Drawer is null");
       
                    Children.Add(drawer);
                  
        
                    drawer.Refresh((IPlatformDrawer)InvertGraphEditor.PlatformDrawer);
                
                }
        }
#if UNITY_DLL
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
#endif
    }

    public class InspectorDrawer : Drawer<InspectorViewModel>
    {
        private float _inspectorWidth = 200f;

        public InspectorDrawer(InspectorViewModel viewModel) : base(viewModel)
        {
            
        }

        public override void Refresh(IPlatformDrawer platform)
        {
            base.Refresh(platform);
        }

        public float InspectorWidth
        {
            get { return _inspectorWidth; }
            set { _inspectorWidth = value; }
        }

        public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        {
            base.Refresh(platform, position, hardRefresh);
            var x = position.x;
            var starty = position.y;
            this.Children.Clear();
            Children.AddRange(CreateDrawers());

            var y = position.y;
            var height = 0f;
            var maxWidth = 0f;
            foreach (var child in Children)
            {
                child.Refresh(platform, new Vector2(x + 10, y), hardRefresh);
                var rect = new Rect(child.Bounds);
                rect.y = y;
                child.Bounds = rect;
                y += child.Bounds.height;
                height += child.Bounds.height;
                if (child.Bounds.width > maxWidth)
                {
                    maxWidth = child.Bounds.width;
                }
            }

            this.Bounds = new Rect(x, starty, maxWidth + 24, height);
            foreach (var child in Children)
            {
                var newRect = new Rect(child.Bounds) { width = maxWidth };
                child.Bounds = newRect;
                child.OnLayout();
            }

       
            //Debug.Log("Bounds at " + position);
        }

        public override void OnLayout()
        {
            base.OnLayout();
            if (ViewModel.TargetViewModel != null)
            {
                var targetBounds = ViewModel.TargetViewModel.Bounds;
                this.Bounds = new Rect(targetBounds.x + targetBounds.width, targetBounds.y, Bounds.width, Bounds.height);
            }
            

        }

        private IEnumerable<IDrawer> CreateDrawers()
        {
            InvertApplication.Log("Creating drawers");
            foreach (var item in ViewModel.ContentItems)
            {
                var drawer = InvertGraphEditor.Container.CreateDrawer(item);
                if (drawer == null)
                {
                    InvertApplication.Log(string.Format("Couldn't create drawer for {0} make sure it is registered.",
                        item.GetType().Name));
                    continue;
                }

                yield return drawer;
            }
        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            base.Draw(platform, scale);

            //if (ViewModel.IsDirty)
            //{
                Refresh(platform);
              //  ViewModel.IsDirty = false;
            //}
            if (ViewModel.Visible)
            {
                var targetBounds = ViewModel.TargetViewModel.Bounds;
                this.Bounds = new Rect(targetBounds.x + targetBounds.width, targetBounds.y, Bounds.width, Bounds.height);
                var adjustedBounds = this.Bounds;
                adjustedBounds.width += 10f;
                adjustedBounds.x -= 5f;
                adjustedBounds.y -= 10f;
                adjustedBounds.height += 20f;
                platform.DrawStretchBox(adjustedBounds, CachedStyles.NodeBackground, 12f);
                foreach (var child in Children)
                {
                    child.Draw(platform, scale);
                }
            }
            
        }
    }
}
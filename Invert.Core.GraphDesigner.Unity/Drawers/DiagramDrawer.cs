using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Invert.Common;
using Invert.Core.GraphDesigner.Unity;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class DiagramDrawer : Drawer, ICommandHandler, IInputHandler
    {
        public delegate void SelectionChangedEventArgs(IDiagramNode oldData, IDiagramNode newData);

   

        public event SelectionChangedEventArgs SelectionChanged;

        
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
                //if (CurrentMouseOverNode != null && !CurrentMouseOverNode.IsSelected)
                //{
                //    yield return CurrentMouseOverNode;
                
                //    CurrentMouseOverNode.IsSelected = true;
                //}
                if (DiagramViewModel != null)
                {
                    yield return DiagramViewModel;
                    yield return DiagramViewModel.DataObject;
                    foreach (var item in DiagramViewModel.GraphItems)
                    {
                        if (!item.IsMouseOver && !item.IsSelected) continue;
                        yield return item;
                        yield return item.DataObject;
                    }
                }

            }
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

        public void CommandExecuted(IEditorCommand command)
        {
            this.Refresh();
        }

        public void CommandExecuting(IEditorCommand command)
        {
        }

        public override void Draw(float scale)
        {
            bool shouldEditField = IsEditingField;
            IsEditingField = false;
            // Make sure they've upgraded to the latest json format
            if (UpgradeOldProject()) return;
            //// Draw the title box
            //GUI.Box(new Rect(0, 0f, DiagramSize.width, 30f), DiagramViewModel.Title , ElementDesignerStyles.DiagramTitle);

       
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
            if (IsEditingField)
            {
                //GUI.FocusControl("EditingField");
                EditorGUI.FocusTextInControl("EditingField");
                EditorGUI.FocusTextInControl("EditingField");
            }
            else
            {
            
            }
        
            DrawErrors();
            DrawHelp();
        }

        public bool HandleKeyEvent(Event evt, ModifierKeyState keyStates)
        {
            var bindings = InvertGraphEditor.KeyBindings;
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
                            InvertApplication.Log("Key Command Executed: " + command.GetType().Name);
                            InvertGraphEditor.ExecuteCommand(command);
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

            if (e.IsMouseDown && e.MouseButton == 0 && !e.ModifierKeyStates.Any)
            {
                foreach (var item in Children.OfType<DiagramNodeDrawer>())
                {
                    if (item.ViewModelObject.IsSelected)
                    {
                        if (DiagramViewModel.Settings.Snap)
                        {
                            item.ViewModel.Position += e.MousePositionDeltaSnapped;
                            item.ViewModel.Position = item.ViewModel.Position.Snap(DiagramViewModel.Settings.SnapSize);
                        }
                        else
                        {
                            item.ViewModel.Position += e.MousePositionDelta;
                        }
                    
                    

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
            var item = DrawersAtMouse.Reverse().FirstOrDefault();
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
                            InvertGraphEditor.ExecuteCommand((v) => { connection1.Remove(connection1); });
                        
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
            var menu = InvertGraphEditor.CreateCommandUI<ContextMenuUI>(this, typeof(IDiagramContextCommand));
            menu.Go();
        }

        public void ShowContextMenu()
        {
            var menu = InvertGraphEditor.CreateCommandUI<ContextMenuUI>(this, typeof(IDiagramNodeCommand), DiagramViewModel.SelectedNode.CommandsType);
            menu.Go();
        }

        public void ShowItemContextMenu(object item)
        {
            var menu = InvertGraphEditor.CreateCommandUI<ContextMenuUI>(this, typeof(IDiagramNodeItemCommand));
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
            if (InvertGraphEditor.Settings.ShowHelp)
            {
                var rect = new Rect(Screen.width - 275f, 10f, 250f, InvertGraphEditor.KeyBindings.Length * 20f);
                GUI.Box(rect, string.Empty);

                GUILayout.BeginArea(rect);
                foreach (var keyBinding in InvertGraphEditor.KeyBindings.Select(p => p.Name + ": " + p.ToString()).Distinct())
                {
                    EditorGUILayout.LabelField(keyBinding);
                }
                EditorGUILayout.LabelField("Open Code: Ctrl+Click");
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

        public static bool IsEditingField { get; set; }

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
                    var drawer = InvertGraphEditor.Container.CreateDrawer<IDrawer>(item);
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
}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public abstract class DiagramNodeDrawer<TViewModel> : DiagramNodeDrawer where TViewModel : DiagramNodeViewModel
    {
        protected DiagramNodeDrawer()
        {
        }

        protected DiagramNodeDrawer(TViewModel viewModel)
        {
            this.ViewModelObject = viewModel;
        }

        public override Rect Bounds
        {
            get { return ViewModelObject.Bounds; }
            set { ViewModelObject.Bounds = value; }
        }

        public TViewModel NodeViewModel
        {
            get { return ViewModel as TViewModel; }
        }
    }

    public abstract class GenericNodeDrawer<TData, TViewModel> : DiagramNodeDrawer<TViewModel>
        where TViewModel : DiagramNodeViewModel where TData : GenericNode
    {


        protected GenericNodeDrawer(TViewModel viewModel) : base(viewModel)
        {
        }

        protected GenericNodeDrawer()
        {
            
        }

        public override void Refresh(IPlatformDrawer platform)
        {
            base.Refresh(platform);
        }

        public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        {
            base.Refresh(platform, position);


        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            base.Draw(platform, scale);
            bool hasErrors = false;

            if (ViewModel.ShowHelp)
            {
                for (int index = 1; index <= NodeViewModel.ContentItems.Count; index++)
                {
                    var item = NodeViewModel.ContentItems[index - 1];
                    if (!string.IsNullOrEmpty(item.Comments))
                    {
                        var bounds = new Rect(item.Bounds);
                        bounds.width = item.Bounds.height;
                        bounds.height = item.Bounds.height;
                        bounds.y += (item.Bounds.height/2) - 12;
                        bounds.x = this.Bounds.x - (bounds.width + 4);
                        platform.DrawLabel(bounds,index.ToString(),CachedStyles.NodeHeader13,DrawingAlignment.MiddleCenter);
                    }
                }
            }
        }

    }

    public abstract class DiagramNodeDrawer : Drawer, INodeDrawer, IDisposable
    {

        private string _cachedLabel;
        private string[] _cachedTags;
        private string _cachedTag;
        private ErrorInfo[] _cachedIssues;
        private object _headerStyle;


        [Inject]
        public IUFrameContainer Container { get; set; }

        public DiagramNodeViewModel ViewModel
        {
            get { return DataContext as DiagramNodeViewModel; }
            set { DataContext = value; }
        }

        protected DiagramNodeDrawer()
        {

        }

        protected override void DataContextChanged()
        {
            base.DataContextChanged();

           // ViewModel.ContentItems.CollectionChanged += ContentItemsOnCollectionChangedWith;
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "IsDirty")
            {
                if (ViewModel.IsDirty)
                {
                    this.RefreshContent();
                
                    ViewModel.IsDirty = false;
                }
           
            }
        }

        private void ContentItemsOnCollectionChangedWith(object sender, NotifyCollectionChangedEventArgs changeargs)
        {
            //this.RefreshContent();
        }
        private void ContentItemsOnCollectionChangedWith(NotifyCollectionChangedEventArgs changeargs)
        {
            //this.RefreshContent();
        }

        void IDrawer.OnMouseDown(MouseEvent mouseEvent)
        {
            OnMouseDown(mouseEvent);
        }

        public float Scale
        {
            get { return InvertGraphEditor.DesignerWindow.Scale; }
        }

        public virtual object ItemStyle
        {
            get { return CachedStyles.Item4; }
        }

        public object SelectedItemStyle
        {
            get { return CachedStyles.SelectedItemStyle; }
        }

        public virtual float Width
        {
            get
            {
                var maxLengthItem = InvertGraphEditor.PlatformDrawer.CalculateSize(ViewModel.FullLabel, CachedStyles.DefaultLabelLarge);
                if (ViewModel.IsCollapsed)
                {
                    foreach (var item in ViewModel.Items)
                    {
                        var newSize = InvertGraphEditor.PlatformDrawer.CalculateSize(item.FullLabel, CachedStyles.DefaultLabelLarge);

                        if (maxLengthItem.x < newSize.x)
                        {
                            maxLengthItem = newSize;
                        }
                    }
                }
                if (ViewModel.ShowSubtitle)
                {
                    var subTitle = InvertGraphEditor.PlatformDrawer.CalculateSize(ViewModel.SubTitle, CachedStyles.DefaultLabelLarge);
                    if (subTitle.x > maxLengthItem.x)
                    {
                        maxLengthItem = subTitle;
                    }
                }


                return Math.Max(150f, maxLengthItem.x + 40);
            }
        }

        public float ItemHeight
        {
            get { return 20; }
        }

        public virtual float Padding
        {
            get { return 8; }
        }

        public virtual object BackgroundStyle
        {
            get { return CachedStyles.NodeHeader1; }
        }

        public float ItemExpandedHeight
        {
            get { return 0; }
        }

        string IDrawer.ShouldFocus { get; set; }



        public DiagramDrawer Diagram { get; set; }


        protected virtual void GetContentDrawers(List<IDrawer> drawers)
        {

            foreach (var item in ViewModel.ContentItems)
            {
                var drawer = InvertGraphEditor.Container.CreateDrawer(item);
                if (drawer == null)
                    InvertApplication.Log(string.Format("Couldn't create drawer for {0} make sure it is registered.",
                        item.GetType().Name));
                drawers.Add(drawer);
            }
        }



        public override void Draw(IPlatformDrawer platform, float scale)
        {



            var width = platform.CalculateSize(_cachedTag, CachedStyles.Tag1).x;
            var labelRect =
                new Rect((Bounds.x + (Bounds.width/2)) - (width/2), Bounds.y - (16f), width, 15f).Scale(Scale);

            platform.DrawLabel(labelRect, _cachedTag, CachedStyles.Tag1, DrawingAlignment.MiddleCenter);
          
#if UNITY_DLL
            var adjustedBounds = new Rect(Bounds.x - 9, Bounds.y + 1, Bounds.width + 19, Bounds.height + 9);
#else
                 var adjustedBounds = Bounds; //new Rect(Bounds.x - 9, Bounds.y + 1, Bounds.width + 19, Bounds.height + 9);
#endif
            var boxRect = adjustedBounds.Scale(Scale);
            platform.DrawStretchBox(boxRect, CachedStyles.NodeBackground, 18);
            
            //if (ViewModel.IsSelected || ViewModel.IsMouseOver)
            //{
            //    platform.DrawStretchBox(boxRect, CachedStyles.NodeBackground, 20);
      
            //}
            if (ViewModel.AllowCollapsing)
            {

                var rect = new Rect((Bounds.x + (Bounds.width/2f)) - 21f,
                    Bounds.y + Bounds.height, 42f, 18f);
                var style = ViewModel.IsCollapsed
                    ? CachedStyles.NodeExpand
                    : CachedStyles.NodeCollapse;

                platform.DoButton(rect.Scale(scale), string.Empty, style, () =>
                {
                    InvertGraphEditor.ExecuteCommand((item) =>
                    {
                        ViewModel.IsCollapsed = !ViewModel.IsCollapsed;
                        Dirty = true;
                    },false);
                });
               

            }


            foreach (var item in Children)
            {
                if (item.Dirty)
                {
                    Refresh((IPlatformDrawer) platform);
                    item.Dirty = false;
                }
                item.Draw(platform, scale);
            }
            bool hasErrors = _cachedIssues.Length > 0;



            if (!ViewModel.IsLocal)
            {
                platform.DrawStretchBox(boxRect, CachedStyles.BoxHighlighter5, 20);
            }
            if (ViewModel.IsMouseOver)
            {
                platform.DrawStretchBox(boxRect, CachedStyles.BoxHighlighter3, 20);
            }
            if (ViewModel.IsSelected)
            {
                platform.DrawStretchBox(boxRect, CachedStyles.BoxHighlighter2, 20);
            }
            if (hasErrors)
            {
                platform.DrawStretchBox(boxRect, CachedStyles.BoxHighlighter6, 20);
            }

            if (ViewModel.IsMouseOver || ViewModel.IsSelected) 
            {
                for (int index = 0; index < _cachedIssues.Length; index++)
                {
                    var keyValuePair = _cachedIssues[index];
                    var w = platform.CalculateSize(keyValuePair.Message, CachedStyles.DefaultLabel).x;//EditorStyles.label.CalcSize(new GUIContent(keyValuePair.Key)).x);
                    var x = (Bounds.x + (Bounds.width/2f)) - (w/2f);
                    var rect = new Rect(x, (Bounds.y + Bounds.height + 18) + (40f*(index)), w + 20f, 40);
                    platform.DrawWarning(rect, keyValuePair.Message);
                    if (keyValuePair.AutoFix != null)
                    {
                        platform.DoButton(new Rect(rect.x + rect.width + 5,rect.y,75,25).Scale(Scale),"Auto Fix",null,keyValuePair.AutoFix);
                    }
                    hasErrors = true;
                }
            }
          
        }

        public object GetNodeColorStyle()
        {

            switch (ViewModel.Color)
            {
                case NodeColor.DarkGray:
                    return CachedStyles.NodeHeader1;
                case NodeColor.Blue:
                    return CachedStyles.NodeHeader2;
                case NodeColor.Gray:
                    return CachedStyles.NodeHeader3;
                case NodeColor.LightGray:
                    return CachedStyles.NodeHeader4;
                case NodeColor.Black:
                    return CachedStyles.NodeHeader5;
                case NodeColor.DarkDarkGray:
                    return CachedStyles.NodeHeader6;
                case NodeColor.Orange:
                    return CachedStyles.NodeHeader7;
                case NodeColor.Red:
                    return CachedStyles.NodeHeader8;
                case NodeColor.YellowGreen:
                    return CachedStyles.NodeHeader9;
                case NodeColor.Green:
                    return CachedStyles.NodeHeader10;
                case NodeColor.Purple:
                    return CachedStyles.NodeHeader11;
                case NodeColor.Pink:
                    return CachedStyles.NodeHeader12;
                case NodeColor.Yellow:
                    return CachedStyles.NodeHeader13;

            }
            return CachedStyles.NodeHeader1;

        }

        protected virtual object HeaderStyle
        {
            get { return _headerStyle ?? (_headerStyle = GetNodeColorStyle()); }
        }

        protected virtual object GetHighlighter()
        {
            return CachedStyles.BoxHighlighter4;
        }

        //protected virtual void DrawItem(IDiagramNodeItem item, ElementsDiagram diagram, bool importOnly)
        //{
        //    if (item.IsSelected && item.IsSelectable && !importOnly)
        //    {
        //        var rect = new Rect(item.Position).Scale(Scale);
        //        //rect.y += ItemHeight;
        //        //rect.height -= ItemHeight;
        //        //rect.height += ItemExpandedHeight;
        //        GUI.Box(rect, string.Empty, SelectedItemStyle);
        //        GUILayout.BeginArea(rect);
        //        EditorGUI.BeginChangeCheck();
        //        EditorGUILayout.BeginHorizontal();

        //        DrawSelectedItem(item, diagram);
        //        EditorGUILayout.EndHorizontal();
        //        GUILayout.EndArea();
        //    }
        //    else
        //    {

        //        GUI.Box(item.Position.Scale(Scale), string.Empty, item.IsSelected ? SelectedItemStyle : ItemStyle);

        //        DrawItemLabel(item);

        //    }
        //    if (!string.IsNullOrEmpty(item.Highlighter))
        //    {
        //        var highlighterPosition = new Rect(item.Position);
        //        highlighterPosition.width = 4;
        //        highlighterPosition.y += 2;
        //        highlighterPosition.x += 2;
        //        highlighterPosition.height = ItemHeight - 6;
        //        GUI.Box(highlighterPosition.Scale(Scale), string.Empty, ElementDesignerStyles.GetHighlighter(item.Highlighter));
        //    }
        //}

        //protected virtual void DrawItemLabel(IDiagramNodeItem item)
        //{
        //    var style = new GUIStyle(ItemStyle);
        //    style.normal.textColor = BackgroundStyle.normal.textColor;
        //    GUI.Label(item.Position.Scale(Scale), item.Label, style);


        //}

        public override void OnMouseDown(MouseEvent mouseEvent)
        {
            ViewModelObject.Select();
            if (mouseEvent.ModifierKeyStates.Ctrl)
            {
                if (mouseEvent.ModifierKeyStates.Alt)
                {
                    this.ViewModel.CtrlShiftClicked();
                }
                else
                {
                    this.ViewModel.CtrlClicked();

                }

            }
        }

        public override void OnMouseDoubleClick(MouseEvent mouseEvent)
        {
            base.OnMouseDoubleClick(mouseEvent);
        }

        public override void OnMouseMove(MouseEvent e)
        {
            base.OnMouseMove(e);
            ViewModel.IsMouseOver = true;
        }

        public virtual IEditorCommand RemoveItemCommand
        {
            get { return InvertGraphEditor.Container.Resolve<IEditorCommand>("RemoveNodeItem"); }
        }

        public virtual void RefreshContent()
        {
            var drawers = new List<IDrawer>();
            drawers.Add(new HeaderDrawer()
            {
                BackgroundStyle = HeaderStyle,
                TextStyle = CachedStyles.ViewModelHeaderStyle,
                ViewModelObject = ViewModelObject,
                Padding = HeaderPadding,

            });
            if (!ViewModel.IsCollapsed)
            {
                GetContentDrawers(drawers);
            }
            Children = drawers.ToList();
        }

        public virtual float HeaderPadding
        {
            get { return 5; }
        }

        public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        {
            _headerStyle = null;
            if (hardRefresh)
            {
                _cachedIssues = ViewModel.Issues.ToArray();
                _cachedTag = string.Join(" | ", ViewModel.Tags.ToArray());
            }
            
            if (Children == null || Children.Count < 1)
            {
                RefreshContent();
            }

            var startY = ViewModel.Position.y;
            // Now lets stretch all the content drawers to the maximum width
            var minWidth = Math.Max(120f, platform.CalculateSize(_cachedTag, CachedStyles.Tag1).x);
            var height = LayoutChildren(platform, startY, ref minWidth);

            _cachedLabel = ViewModel.Label;

            if (!ViewModel.IsCollapsed)
            {
                Bounds = new Rect(ViewModel.Position.x, ViewModel.Position.y, minWidth, height + Padding);
            }
            else
            {
                Bounds = new Rect(ViewModel.Position.x, ViewModel.Position.y, minWidth, height);
            }
            //if (ViewModel.IsCollapsed)
            //{
            var cb = new Rect(Children[0].Bounds);
            cb.x += 15;
            cb.y += 1;
            cb.width -=13;
           // ViewModelObject.ConnectorBounds = cb;
           // //}
           // //else
           // //{
                ViewModel.ConnectorBounds = cb;
           //// }
            
           

            //_cachedTags = ViewModel.Tags.Reverse().ToArray();
            //ViewModel.HeaderPosition = new Rect(ViewModel.Position.x, ViewModel.Position.y, maxWidth, ViewModel.HeaderSize);
        }

        protected virtual float LayoutChildren(IPlatformDrawer platform, float startY, ref float minWidth)
        {
            var height = 0f;


            // Get our content drawers
            foreach (var child in Children)
            {
                child.Refresh(platform, new Vector2(ViewModel.Position.x, startY));
                startY += child.Bounds.height;
            }

            foreach (var item in Children)
            {
                if (item.Bounds.width > minWidth) minWidth = item.Bounds.width;
                height += item.Bounds.height;
            }

            foreach (var cachedDrawer in Children)
            {
                cachedDrawer.Bounds = new Rect(cachedDrawer.Bounds) {width = minWidth};
                cachedDrawer.Dirty = false;
                cachedDrawer.OnLayout();
            }
            return height;
        }

        public bool IsExternal { get; set; }

        public IEnumerable<IDrawer> SelectedChildren
        {
            get { return Children.Where(p => p.IsSelected); }
        }


        public void Dispose()
        {
            ViewModel.ContentItems.CollectionChanged -= ContentItemsOnCollectionChangedWith;
        }
    }

    //public class BulletPointDrawer : Drawer<BulletPointViewModel>
    //{
        
    //}

    
}
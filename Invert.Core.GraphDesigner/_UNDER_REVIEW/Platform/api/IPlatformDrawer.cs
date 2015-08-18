using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public enum DrawingAlignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public interface IPlatformDrawer
    {
        void BeginRender(object sender, MouseEvent mouseEvent);

        //void DrawConnector(float scale, ConnectorViewModel viewModel);
        Vector2 CalculateSize(string text, object tag1);
       
        Vector2 CalculateImageSize(string imageName);

        void DoButton(Rect scale, string label, object style, Action action, Action rightClick = null);
        void DoButton(Rect scale, string label, object style, Action<Vector2> action, Action<Vector2> rightClick = null);

        void DrawBezier(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, Color color, float width);

        void DrawColumns(Rect rect, float[] columnWidths, params Action<Rect>[] columns);

        void DrawColumns(Rect rect, params Action<Rect>[] columns);

        void DrawImage(Rect bounds, string texture, bool b);
        void DrawImage(Rect bounds, object texture, bool b);

        void DrawLabel(Rect rect, string label, object style, DrawingAlignment alignment = DrawingAlignment.MiddleLeft);
        
        void DrawPolyLine(Vector2[] lines, Color color);

        void DrawPropertyField(PropertyFieldViewModel propertyFieldDrawer, float scale);

        void DrawStretchBox(Rect scale, object nodeBackground, float offset);

        void DrawStretchBox(Rect scale, object nodeBackground, Rect offset);

        void DrawTextbox(string id, Rect bounds, string value, object itemTextEditingStyle, Action<string, bool> valueChangedAction);

        void DrawWarning(Rect rect, string key);

        void DrawNodeHeader(Rect boxRect, object backgroundStyle, bool isCollapsed, float scale, object image);

        void DoToolbar(Rect toolbarTopRect, DesignerWindow designerWindow, ToolbarPosition position);

        void DoTabs(Rect tabsRect, DesignerWindow designerWindow);

        void EndRender();
        //Rect GetRect(object style);
        void DrawRect(Rect boundsRect, Color color);
        
    }

    public interface IStyleProvider
    {
        object GetImage(string name);
        object GetStyle(InvertStyles name);
        object GetFont(string fontName);

        INodeStyleSchema GetNodeStyleSchema(NodeStyle name);
        IConnectorStyleSchema GetConnectorStyleSchema(ConnectorStyle name);
    }

    public enum ConnectorStyle
    {
        Triangle,
        Circle
    }

    public interface IImmediateControlsDrawer<TControl> : IPlatformDrawer
    {
        List<string> ControlsLeftOver { get; set; }
        Dictionary<string, TControl> Controls { get; set; }
        void AddControlToCanvas(TControl control);
        void RemoveControlFromCanvas(TControl control);
        void SetControlPosition(TControl control, float x, float y, float width, float height);
    }

    public static class PlatformDrawerExtensions
    {
        //public static void DoVertical(float startX, float startY, float width, float itemHeight, params Action<Rect>[] rows)
        //{

        //}
        //public static bool DoToolbar(this IPlatformDrawer drawer, Rect rect, string label, bool open, Action add = null, Action leftButton = null, Action paste = null, GUIStyle addButtonStyle = null, GUIStyle pasteButtonStyle = null, bool fullWidth = true)
        //{
        //    var style =  open ? CachedStyles.Toolbar : CachedStyles.ToolbarButton;
        //    drawer.DrawStretchBox(rect, style ,0f);

        //    var labelRect = new Rect(rect.x + 2, rect.y + (rect.height / 2) - 8, rect.width - (add != null ? 50 : 0), 16);
        //    var result = open;
        //    if (leftButton == null)
        //    {
        //        drawer.DoButton(rect, label, style, () =>
        //        {

        //        });
        //    }
        //    else
        //    {
        //        if (GUI.Button(labelRect, new GUIContent(label, ElementDesignerStyles.ArrowLeftTexture), labelStyle))
        //        {
        //            leftButton();
        //        }
        //    }

        //    if (paste != null)
        //    {
        //        var addButtonRect = new Rect(rect.x + rect.width - 42, rect.y + (rect.height / 2) - 8, 16, 16);
        //        if (GUI.Button(addButtonRect, "", pasteButtonStyle ?? ElementDesignerStyles.PasteButtonStyle))
        //        {
        //            paste();
        //        }
        //    }

        //    if (add != null)
        //    {
        //        var addButtonRect = new Rect(rect.x + rect.width - 21, rect.y + (rect.height / 2) - 8, 16, 16);
        //        if (GUI.Button(addButtonRect, "", addButtonStyle ?? ElementDesignerStyles.AddButtonStyleUnscaled))
        //        {
        //            add();
        //        }
        //    }
        //    return result;
        //}

        //public static bool DoSectionBar(this IPlatformDrawer drawer, Rect rect, string title)
        //{
        //    var tBar = DoToolbar(label, EditorPrefs.GetBool(label, true), add, leftButton, paste);
        //    if (tBar)
        //    {
        //        EditorPrefs.SetBool(label, !EditorPrefs.GetBool(label));
        //    }
        //    return EditorPrefs.GetBool(label);
        //}
        //public static void DoTriggerButton()
        //{

        //}

        public static void BeginImmediate<TControl>(this IImmediateControlsDrawer<TControl> drawer)
        {
            if (drawer.Controls == null)
            {
                drawer.Controls = new Dictionary<string, TControl>();
            }
            if (drawer.ControlsLeftOver == null)
            {
                drawer.ControlsLeftOver = new List<string>();
            }
            drawer.ControlsLeftOver.AddRange(drawer.Controls.Select(p => p.Key));
        }
        public static void EndImmediate<TControl>(this IImmediateControlsDrawer<TControl> drawer)
        {
            for (int index = 0; index < drawer.ControlsLeftOver.Count; index++)
            {
                var item = drawer.ControlsLeftOver[index];
                drawer.RemoveControlFromCanvas(drawer.Controls[item]);
                drawer.Controls.Remove(item);
            }
            drawer.ControlsLeftOver.Clear();

        }
        public static TControlType EnsureControl<TControl, TControlType>(this IImmediateControlsDrawer<TControl> drawer, string id, Rect rect, Func<TControlType> init = null) where TControlType : TControl
        {

            TControl control;
            if (!drawer.Controls.TryGetValue(id, out control))
            {
                if (init != null)
                {
                    control = init();
                    drawer.AddControlToCanvas(control);
                    drawer.Controls.Add(id, control);
                }

            }
            drawer.ControlsLeftOver.Remove(id);
            drawer.SetControlPosition(control, rect.x, rect.y, rect.width, rect.height);
            return (TControlType)control;
        }
    }

    public static class CachedStyles
    {
        private static object _boxHighlighter5;
        private static object _boxHighlighter1;
        private static object _nodeExpand;
        private static object _item1;
        private static object _defaultLabel;
        private static object _nodeBackground;
        private static object _nodeCollapse;
        private static object _boxHighlighter2;
        private static object _boxHighlighter3;
        private static object _boxHighlighter4;
        private static object _boxHighlighter6;
        private static object _nodeHeader1;
        private static object _nodeHeader2;
        private static object _nodeHeader3;
        private static object _nodeHeader4;
        private static object _nodeHeader5;
        private static object _nodeHeader6;
        private static object _nodeHeader7;
        private static object _nodeHeader8;
        private static object _nodeHeader9;
        private static object _nodeHeader10;
        private static object _nodeHeader11;
        private static object _nodeHeader12;
        private static object _nodeHeader13;
        private static object _itemTextEditingStyle;
        private static object _addButtonStyle;
        private static object _headerStyle;
        private static object _defaultLabelLarge;
        private static object _clearItemStyle;
        private static object _viewModelHeaderStyle;
        private static object _itemStyle;
        private static object _selectedItemStyle;
        private static object _item2;
        private static object _item3;
        private static object _item4;
        private static object _item5;
        private static object _item6;
        private static object _tag1;
        private static object _toolbar;
        private static object _toolbarButton;
        private static object _toolbarButtonDrop;
        private static object _graphTitleLabel;
        private static IConnectorStyleSchema _connectorStyleSchemaTriangle;
        private static IConnectorStyleSchema _connectorStyleSchemaCircle;
        private static INodeStyleSchema _nodeStyleSchemaNormal;
        private static INodeStyleSchema _nodeStyleSchemaMinimalistic;
        private static INodeStyleSchema _nodeStyleSchemaBold;
        private static object _nodeBackgroundBorderless;

        public static object Item1
        {
            get { return _item1 ?? (_item1 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.Item1)); }
        }
        public static object Item2
        {
            get { return _item2 ?? (_item2 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.Item2)); }
        }
        public static object Item3
        {
            get { return _item3 ?? (_item3 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.Item3)); }
        }
        public static object Item4
        {
            get { return _item4 ?? (_item4 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.Item4)); }
        }
        public static object Item5
        {
            get { return _item5 ?? (_item5 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.Item5)); }
        }
        public static object Item6
        {
            get { return _item6 ?? (_item6 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.Item6)); }
        }
        public static object DefaultLabel
        {
            get { return _defaultLabel ?? (_defaultLabel = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.DefaultLabel)); }
        }
        public static object NodeBackground
        {
            get { return _nodeBackground ?? (_nodeBackground = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeBackground)); }
        }       
        public static object NodeBackgroundBorderless
        {
            get { return _nodeBackgroundBorderless ?? (_nodeBackgroundBorderless = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeBackgroundBorderless)); }
        }
        public static object NodeExpand
        {
            get { return _nodeExpand ?? (_nodeExpand = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeExpand)); }
        }
        public static object NodeCollapse
        {
            get { return _nodeCollapse ?? (_nodeCollapse = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeCollapse)); }
        }
        public static object BoxHighlighter1
        {
            get { return _boxHighlighter1 ?? (_boxHighlighter1 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.BoxHighlighter1)); }
        }
        public static object BoxHighlighter2
        {
            get { return _boxHighlighter2 ?? (_boxHighlighter2 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.BoxHighlighter2)); }
        }
        public static object BoxHighlighter3
        {
            get { return _boxHighlighter3 ?? (_boxHighlighter3 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.BoxHighlighter3)); }
        }
        public static object BoxHighlighter4
        {
            get { return _boxHighlighter4 ?? (_boxHighlighter4 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.BoxHighlighter4)); }
        }
        public static object BoxHighlighter5
        {
            get { return _boxHighlighter5 ?? (_boxHighlighter5 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.BoxHighlighter5)); }
        }
        public static object BoxHighlighter6
        {
            get { return _boxHighlighter6 ?? (_boxHighlighter6 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.BoxHighlighter6)); }
        }
        public static object NodeHeader1
        {
            get { return _nodeHeader1 ?? (_nodeHeader1 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader1)); }
        }
        public static object NodeHeader2
        {
            get { return _nodeHeader2 ?? (_nodeHeader2 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader2)); }
        }
        public static object NodeHeader3
        {
            get { return _nodeHeader3 ?? (_nodeHeader3 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader3)); }
        }
        public static object NodeHeader4
        {
            get { return _nodeHeader4 ?? (_nodeHeader4 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader4)); }
        }
        public static object NodeHeader5
        {
            get { return _nodeHeader5 ?? (_nodeHeader5 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader5)); }
        }
        public static object NodeHeader6
        {
            get { return _nodeHeader6 ?? (_nodeHeader6 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader6)); }
        }
        public static object NodeHeader7
        {
            get { return _nodeHeader7 ?? (_nodeHeader7 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader7)); }
        }
        public static object NodeHeader8
        {
            get { return _nodeHeader8 ?? (_nodeHeader8 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader8)); }
        }
        public static object NodeHeader9
        {
            get { return _nodeHeader9 ?? (_nodeHeader9 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader9)); }
        }
        public static object NodeHeader10
        {
            get { return _nodeHeader10 ?? (_nodeHeader10 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader10)); }
        }
        public static object NodeHeader11
        {
            get { return _nodeHeader11 ?? (_nodeHeader11 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader11)); }
        }
        public static object NodeHeader12
        {
            get { return _nodeHeader12 ?? (_nodeHeader12 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader12)); }
        }
        public static object NodeHeader13
        {
            get { return _nodeHeader13 ?? (_nodeHeader13 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.NodeHeader13)); }
        }


        public static object ItemTextEditingStyle
        {
            get { return _itemTextEditingStyle ?? (_itemTextEditingStyle = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.ItemTextEditingStyle)); }
        }
        public static object AddButtonStyle
        {
            get { return _addButtonStyle ?? (_addButtonStyle = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.AddButtonStyle)); }
        }
        public static object HeaderStyle
        {
            get { return _headerStyle ?? (_headerStyle = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.HeaderStyle)); }
        }
        public static object DefaultLabelLarge
        {
            get { return _defaultLabelLarge ?? (_defaultLabelLarge = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.DefaultLabelLarge)); }
        }
        public static object GraphTitleLabel
        {
            get { return _graphTitleLabel ?? (_graphTitleLabel = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.GraphTitleLabel)); }
        }
        public static object ClearItemStyle
        {
            get { return _clearItemStyle ?? (_clearItemStyle = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.ClearItemStyle)); }
        }
        public static object ViewModelHeaderStyle
        {
            get { return _viewModelHeaderStyle ?? (_viewModelHeaderStyle = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.ViewModelHeaderStyle)); }
        }
        public static object ItemStyle
        {
            get { return _itemStyle ?? (_itemStyle = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.ItemStyle)); }
        }
        public static object SelectedItemStyle
        {
            get { return _selectedItemStyle ?? (_selectedItemStyle = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.SelectedItemStyle)); }
        }
        public static object Tag1
        {
            get { return _tag1 ?? (_tag1 = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.Tag1)); }
        }
        public static object Toolbar
        {
            get { return _toolbar ?? (_toolbar = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.Toolbar)); }
        }
        public static object ToolbarButton
        {
            get { return _toolbarButton ?? (_toolbarButton = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.ToolbarButton)); }
        }
        public static object ToolbarButtonDrop
        {
            get { return _toolbarButtonDrop ?? (_toolbarButtonDrop = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.ToolbarButtonDown)); }
        }        
        public static object HeaderTitleStyle
        {
            get { return _toolbarButtonDrop ?? (_toolbarButtonDrop = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.HeaderTitleStyle)); }
        }
        public static object HeaderSubTitleStyle
        {
            get { return _toolbarButtonDrop ?? (_toolbarButtonDrop = InvertGraphEditor.StyleProvider.GetStyle(InvertStyles.HeaderSubTitleStyle)); }
        }

        


        public static IConnectorStyleSchema ConnectorStyleSchemaTriangle
        {
            get { return _connectorStyleSchemaTriangle ?? (_connectorStyleSchemaTriangle = InvertGraphEditor.StyleProvider.GetConnectorStyleSchema(ConnectorStyle.Triangle)); }
        }

        public static IConnectorStyleSchema ConnectorStyleSchemaCircle
        {
            get { return _connectorStyleSchemaCircle ?? (_connectorStyleSchemaCircle = InvertGraphEditor.StyleProvider.GetConnectorStyleSchema(ConnectorStyle.Circle)); }
        }

        public static INodeStyleSchema NodeStyleSchemaNormal
        {
            get { return _nodeStyleSchemaNormal ?? (_nodeStyleSchemaNormal = InvertGraphEditor.StyleProvider.GetNodeStyleSchema(NodeStyle.Normal)); }
            set { _nodeStyleSchemaNormal = value; }
        }

        public static INodeStyleSchema NodeStyleSchemaMinimalistic
        {
            get { return _nodeStyleSchemaMinimalistic ?? (_nodeStyleSchemaMinimalistic = InvertGraphEditor.StyleProvider.GetNodeStyleSchema(NodeStyle.Minimalistic)); }
            set { _nodeStyleSchemaMinimalistic = value; }
        }

        public static INodeStyleSchema NodeStyleSchemaBold
        {
            get { return _nodeStyleSchemaBold ?? (_nodeStyleSchemaBold = InvertGraphEditor.StyleProvider.GetNodeStyleSchema(NodeStyle.Bold)); }
            set { _nodeStyleSchemaBold = value; }
        }
    }

    public interface IDebugWindowEvents
    {
        void RegisterInspectedItem(object item, string name, bool includeReflectiveInspector = false);
        void QuickInspect(object data, string name, Vector2 mousePosition);
        void Watch(object data, string name, Vector2 mousePosition);
    }

    public enum NodeStyle
    {
        Normal,
        Minimalistic,
        Bold
    }

    public enum InvertStyles
    {
        DefaultLabel,
        Tag1,
        NodeBackground,
        NodeExpand,
        NodeCollapse,
        BoxHighlighter5,
        BoxHighlighter3,
        BoxHighlighter1,
        BoxHighlighter2,
        BoxHighlighter6,
        BoxHighlighter4,
        NodeHeader1,
        NodeHeader2,
        NodeHeader3,
        NodeHeader4,
        NodeHeader5,
        NodeHeader6,
        NodeHeader7,
        NodeHeader8,
        NodeHeader9,
        NodeHeader10,
        NodeHeader11,
        NodeHeader12,
        NodeHeader13,
        Item1,
        Item2,
        Item3,
        Item4,
        Item5,
        Item6,
        SelectedItemStyle,
        ItemStyle,
        ViewModelHeaderStyle,
        ClearItemStyle,
        DefaultLabelLarge,
        HeaderStyle,
        HeaderTitleStyle,
        HeaderSubTitleStyle,
        AddButtonStyle,
        ItemTextEditingStyle,
        Toolbar,
        ToolbarButton,
        ToolbarButtonDown,
        GraphTitleLabel,
        NodeBackgroundBorderless
    }

 
}
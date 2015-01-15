using System;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public enum DrawingAlignment
    {
        MiddleLeft,
        MiddleRight,
        MiddleCenter,
        TopRight,
        TopCenter,
        TopLeft,
        BottomCenter,
        BottomLeft,
        BottomRight
    }

  

    public interface IPlatformDrawer
    {
        void BeginRender(object sender, MouseEvent mouseEvent);

        //void DrawConnector(float scale, ConnectorViewModel viewModel);
        Vector2 CalculateSize(string text, object tag1);

        void DoButton(Rect scale, string label, object style, Action action);

        void DrawBezier(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent,
            Color color, float width);

        void DrawColumns(Rect rect,int[] columnWidths, params Action<Rect>[] columns);
        void DrawColumns(Rect rect, params Action<Rect>[] columns);

        void DrawImage(Rect bounds, string texture, bool b);

        void DrawLabel(Rect rect, string label, object style, DrawingAlignment alignment = DrawingAlignment.MiddleLeft);

        void DrawPolyLine(Vector2[] lines, Color color);

        void DrawPropertyField(PropertyFieldDrawer propertyFieldDrawer, float scale);

        void DrawStretchBox(Rect scale, object nodeBackground, float offset);

        void DrawStretchBox(Rect scale, object nodeBackground, Rect offset);

        void DrawTextbox(string id, Rect bounds, string value, object itemTextEditingStyle, Action<string, bool> valueChangedAction);

        void DrawWarning(Rect rect, string key);

        void EndRender();
    }

    public interface IStyleProvider
    {
        object GetImage(string name);

        object GetStyle(InvertStyles name);
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
        AddButtonStyle,
        ItemTextEditingStyle
    }
}
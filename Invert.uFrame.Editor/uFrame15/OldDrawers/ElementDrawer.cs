using System;
using System.Collections.Generic;
using Invert.Common;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class ElementDrawer : DiagramNodeDrawer<ElementNodeViewModel>
{
    public ElementDrawer()
    {
    }


    public override object ItemStyle
    {
        get { return CachedStyles.Item4; }
    }


    public ElementDrawer(ElementNodeViewModel viewModel)
    {
        ViewModel = viewModel;
    }



    //public SectionHeaderDrawer PropertiesHeader
    //{
    //    get
    //    {
    //        if (_propertiesHeader == null)
    //        {
    //            _propertiesHeader = Container.Resolve<SectionHeaderDrawer>(null, false, ElementViewModel);

    //            _propertiesHeader.Label = "Properties";
    //            _propertiesHeader.HeaderType = typeof(ViewModelPropertyData);
    //            if (NodeViewModel.IsLocal)
    //                _propertiesHeader.AddCommand = Container.Resolve<AddElementPropertyCommand>();
    //        }
    //        return _propertiesHeader;
    //    }
    //    set { _propertiesHeader = value; }
    //}
    //public SectionHeaderDrawer ComputedHeader
    //{
    //    get
    //    {
    //        if (_computedHeader == null)
    //        {
    //            _computedHeader = Container.Resolve<SectionHeaderDrawer>(null, false, ElementViewModel);
    //            _computedHeader.Label = "Computed";
    //            _computedHeader.HeaderType = typeof(ViewModelPropertyData);
    //        }
    //        return _computedHeader;
    //    }
    //    set { _computedHeader = value; }
    //}
    //public SectionHeaderDrawer CollectionsHeader
    //{
    //    get
    //    {

    //        if (_collectionsHeader == null)
    //        {
    //            _collectionsHeader = Container.Resolve<SectionHeaderDrawer>(null, false, ElementViewModel);
    //            _collectionsHeader.Label = "Collections";
    //            _collectionsHeader.HeaderType = typeof(ViewModelCollectionData);
    //            if (NodeViewModel.IsLocal)
    //                _collectionsHeader.AddCommand = Container.Resolve<AddElementCollectionCommand>();
    //        }
    //        return _collectionsHeader;
    //    }
    //    set { _collectionsHeader = value; }
    //}

    //public SectionHeaderDrawer CommandsHeader
    //{
    //    get
    //    {
    //        if (_commandsHeader == null)
    //        {
    //            _commandsHeader = Container.Resolve<SectionHeaderDrawer>(null, false, ElementViewModel);
    //            _commandsHeader.Label = "Commands";
    //            _commandsHeader.HeaderType = typeof(ViewModelCommandData);
    //            if (NodeViewModel.IsLocal)
    //                _commandsHeader.AddCommand = Container.Resolve<AddElementCommandCommand>();
    //        }
    //        return _commandsHeader;
    //    }
    //    set { _commandsHeader = value; }
    //}


    //private SectionHeaderDrawer _propertiesHeader;
    //private SectionHeaderDrawer _collectionsHeader;
    //private SectionHeaderDrawer _commandsHeader;

    private float _width;

    //public IConnectionPointDrawer InheritanceConnector
    //{
    //    get { return _inheritanceConnector ?? (_inheritanceConnector = new DefaultConnectionPointDrawer()); }
    //    set { _inheritanceConnector = value; }
    //}
    public override void Draw(IPlatformDrawer platform, float scale)
    {
        base.Draw(platform, scale);
        if (NodeViewModel.IsTemplate)
            GUI.Label(new Rect(Bounds.x + 10, Bounds.y - 18f, 120f, 15f ).Scale(Scale), "Abstract",
                 ElementDesignerStyles.Tag2);
    }

    protected override object GetHighlighter()
    {

        return base.GetHighlighter();
    }

    public override float Width
    {
        get
        {

            return Math.Max(110 * Scale, _width);
        }
    }

    public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
    {
        base.Refresh(platform, position);

        //_isMultiInstance = NodeViewModel.IsMultiInstance;
        _maxNameWidth = MaxNameWidth(EditorStyles.label);
        _maxTypeWidth = MaxTypeWidth(EditorStyles.label);

        _width = Math.Max(EditorStyles.largeLabel.CalcSize(new GUIContent(ViewModel.FullLabel)).x + 50, _maxNameWidth + _maxTypeWidth);
    }

    private float _maxTypeWidth;
    private float _maxNameWidth;
    private SectionHeaderDrawer _computedHeader;
    private bool _isRegistered;


    public virtual float MaxTypeWidth(GUIStyle style)
    {
        var maxLengthItem = Vector2.zero;

        if (ViewModel.AllowCollapsing && !ViewModel.IsCollapsed)
        {
            foreach (var item in NodeViewModel.ViewModelItems)
            {
                var rtn = item.RelatedTypeName ?? "[None]";

                if (ElementDataBase.TypeNameAliases.ContainsKey(rtn))
                {
                    rtn = ElementDataBase.TypeNameAliases[rtn];
                }
                var newSize = style.CalcSize(new GUIContent(ElementDataBase.TypeAlias(rtn)));

                if (maxLengthItem.x < newSize.x)
                {
                    maxLengthItem = newSize;
                }
            }
        }
        return maxLengthItem.x + 5;

    }
    public float MaxNameWidth(GUIStyle style)
    {
        //style.fontStyle= FontStyle.Bold;
        var maxLengthItem = Vector2.zero;
        if (ViewModel.AllowCollapsing && !ViewModel.IsCollapsed)
        {
            foreach (var item in NodeViewModel.ViewModelItems)
            {
                var newSize = style.CalcSize(new GUIContent(item.Name));

                if (maxLengthItem.x < newSize.x)
                {
                    maxLengthItem = newSize;
                }
            }
        }


        return maxLengthItem.x + 8;

    }
    protected override object HeaderStyle
    {
        get { return CachedStyles.NodeHeader3; }
    }

    //protected override void DrawSelectedItem(IDiagramNodeItem nodeItem, ElementsDiagram diagram)
    //{
    //    var item = nodeItem as IViewModelItem;
    //    if (item == null)
    //    {
    //        base.DrawSelectedItem(nodeItem, diagram);
    //        return;
    //    }
    //    GUILayout.Space(7);
    //    var rtn = item.RelatedTypeName ?? "[None]";

    //    if (ElementDataBase.TypeNameAliases.ContainsKey(rtn))
    //    {
    //        rtn = ElementDataBase.TypeNameAliases[rtn];
    //    }
    //    if (GUILayout.Button(rtn, ElementDesignerStyles.ClearItemStyle))
    //    {
    //        var commandName = item.GetType().Name.Replace("Data", "") + "TypeSelection";
    //        var command = Container.Resolve<IEditorCommand>(commandName);
    //        if (command == null)
    //        {
    //            Debug.Log("Type selection command not found for " + commandName);
    //        }
    //        else
    //        {
    //            uFrameEditor.ExecuteCommand(command);
    //        }



    //    }
    //    base.DrawSelectedItem(nodeItem, diagram);
    //}


    //protected override void DrawItemLabel(IDiagramNodeItem item)
    //{
    //    var vmItem = item as IViewModelItem;
    //    if (vmItem == null)
    //    {
    //        base.DrawItemLabel(item);
    //    }
    //    else
    //    {
    //        GUILayout.BeginArea(item.Position.Scale(Scale));
    //        GUILayout.BeginHorizontal();
    //        GUILayout.Space(7);

    //        var style = new GUIStyle(ElementDesignerStyles.ClearItemStyle)
    //        {
    //            fontStyle = FontStyle.Normal,
    //            alignment = TextAnchor.MiddleLeft,
    //            normal = { textColor = ElementDesignerStyles.NodeBackground.normal.textColor }
    //        };
    //        // style.fontSize = Mathf.RoundToInt(style.fontSize * Scale);
    //        var rtn = vmItem.RelatedTypeName ?? string.Empty;
    //        if (ElementDataBase.TypeNameAliases.ContainsKey(rtn))
    //        {
    //            rtn = ElementDataBase.TypeNameAliases[rtn];
    //        }
    //        //GUILayout.Label((_maxTypeWidth * Scale).ToString(), style, GUILayout.Width(_maxTypeWidth * Scale));
    //        GUI.Label(new Rect(5,0f,_maxTypeWidth * Scale,ItemHeight), rtn, style);
    //        style.fontStyle = FontStyle.Bold;
    //        style.alignment = TextAnchor.MiddleLeft;
    //        GUI.Label(new Rect(_maxTypeWidth * Scale, 0f, (_maxNameWidth + 10)* Scale, ItemHeight), item.Name, style);
    //        //GUILayout.Label((_maxNameWidth * Scale).ToString(), style, GUILayout.Width(_maxNameWidth * Scale));
    //       // GUI.Label(vmItem.Name, style, GUILayout.Width(_maxNameWidth * Scale));
    //        GUILayout.EndHorizontal();
    //        GUILayout.EndArea();
    //    }
    //    //base.DrawItemLabel(item);

    //}

    public ElementNodeViewModel ElementViewModel
    {
        get
        {
            return ViewModel as ElementNodeViewModel;
        }
    }

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        base.GetContentDrawers(drawers);
        //var inheritedProperties =
        //      ElementViewModel.ContentItems.OfType<ElementPropertyItemViewModel>()
        //          .Where(p => p.NodeItem.Node != NodeViewModel.GraphItem).ToArray();
        //if (inheritedProperties.Length > 0)
        //{
        //    drawers.Add(new SectionHeaderDrawer(new SectionHeaderViewModel()
        //    {
        //        Name = "Inherited Properties"
        //    }));
        //    foreach (var item in inheritedProperties)
        //        drawers.Add(InvertGraphEditor.CreateDrawer(item));
        //}


        //drawers.Add(PropertiesHeader);
        //foreach (
        //    var item in
        //        ElementViewModel.ContentItems.OfType<ElementPropertyItemViewModel>()
        //            .Where(p => p.NodeItem.Node == NodeViewModel.GraphItem))
        //    drawers.Add(InvertGraphEditor.CreateDrawer(item));



        //// Collections Here
        //var inheritedCollections =
        //      ElementViewModel.ContentItems.OfType<ElementCollectionItemViewModel>()
        //          .Where(p => p.NodeItem.Node != NodeViewModel.GraphItem).ToArray();
        //if (inheritedCollections.Length > 0)
        //{
        //    drawers.Add(new SectionHeaderDrawer(new SectionHeaderViewModel()
        //    {
        //        Name = "Inherited Collections"
        //    }));
        //    foreach (var item in inheritedCollections)
        //        drawers.Add(InvertGraphEditor.CreateDrawer(item));
        //}


        //drawers.Add(CollectionsHeader);
        //foreach (
        //    var item in
        //        ElementViewModel.ContentItems.OfType<ElementCollectionItemViewModel>()
        //            .Where(p => p.NodeItem.Node == NodeViewModel.GraphItem))
        //    drawers.Add(InvertGraphEditor.CreateDrawer(item));


        
        //// COMMANDS HERE
        //var inheritedCommands =
        //ElementViewModel.ContentItems.OfType<ElementCommandItemViewModel>()
        //    .Where(p => p.NodeItem.Node != NodeViewModel.GraphItem).ToArray();
        //if (inheritedCommands.Length > 0)
        //{
        //    drawers.Add(new SectionHeaderDrawer(new SectionHeaderViewModel()
        //    {
        //          Name = "Inherited Commands"
        //    }));

        //    foreach (var item in inheritedCommands)
        //        drawers.Add(InvertGraphEditor.CreateDrawer(item));
        //}

        //drawers.Add(CommandsHeader);
        //foreach (
        //    var item in
        //        ElementViewModel.ContentItems.OfType<ElementCommandItemViewModel>()
        //            .Where(p => p.NodeItem.Node == NodeViewModel.GraphItem))
        //    drawers.Add(InvertGraphEditor.CreateDrawer(item));

        //drawers.Add(CommandsHeader);
        //foreach (var item in ElementViewModel.ContentItems.OfType<ElementCommandItemViewModel>())
        //{
        //    drawers.Add(uFrameEditor.CreateDrawer(item));
        //}


        //var properties = NodeViewModel.Properties.Where(p=>!p.IsComputed).Cast<IDiagramNodeItem>().ToArray();
        //yield return new DiagramSubItemGroup()
        //{
        //    Header = PropertiesHeader,
        //    Items = properties
        //};
        //var computed = NodeViewModel.Properties.Where(p => p.IsComputed).Cast<IDiagramNodeItem>().ToArray();
        //if (computed.Length > 0)
        //{
        //    yield return new DiagramSubItemGroup()
        //    {
        //        Header = ComputedHeader,
        //        Items = computed
        //    };
        //}
        //yield return new DiagramSubItemGroup()
        //{
        //    Header = CollectionsHeader,
        //    Items = NodeViewModel.Collections.Cast<IDiagramNodeItem>().ToArray()
        //};
        //yield return new DiagramSubItemGroup()
        //{
        //    Header = CommandsHeader,
        //    Items = NodeViewModel.Commands.Cast<IDiagramNodeItem>().ToArray()
        //};
    }
}
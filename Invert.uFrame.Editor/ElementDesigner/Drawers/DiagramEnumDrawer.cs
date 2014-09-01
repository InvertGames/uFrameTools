using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class DiagramEnumDrawer : DiagramNodeDrawer<EnumNodeViewModel>
{
    private NodeItemHeader _itemsHeader;

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader8; }
    }

    public DiagramEnumDrawer()
    {
    }

    public DiagramEnumDrawer(EnumData data, ElementsDiagram diagram)
        : base(diagram)
    {
        ViewModel = new EnumNodeViewModel(data);
    }

    public NodeItemHeader ItemsHeader
    {
        get
        {

            if (_itemsHeader == null)
            {
                _itemsHeader = Container.Resolve<NodeItemHeader>();
                _itemsHeader.Label = "Items";
                _itemsHeader.HeaderType = typeof(EnumData);
                _itemsHeader.AddCommand = Container.Resolve<AddEnumItemCommand>();
            }

            return _itemsHeader;
        }
        set { _itemsHeader = value; }
    }

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        base.GetContentDrawers(drawers);
        foreach (var item in NodeViewModel.EnumItems)
        {
            drawers.Add(new EnumItemDrawer(item));
        }
        //yield return new DiagramSubItemGroup()
        //{
        //    Header = ItemsHeader,
        //    Items = NodeViewModel.EnumItems.Cast<IDiagramNodeItem>().ToArray()
        //};
    }

    protected override void DrawSelectedItem(IDiagramNodeItem nodeItem, ElementsDiagram diagram)
    {
        base.DrawSelectedItem(nodeItem, diagram);

    }
}
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

    public DiagramEnumDrawer(EnumNodeViewModel viewModel)

    {
        ViewModel = viewModel;
    }

    public NodeItemHeader ItemsHeader
    {
        get
        {

            if (_itemsHeader == null)
            {
                _itemsHeader = Container.Resolve<NodeItemHeader>(null,false,ViewModel);
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
        drawers.Insert(1,ItemsHeader);
        //yield return new DiagramSubItemGroup()
        //{
        //    Header = ItemsHeader,
        //    Items = NodeViewModel.EnumItems.Cast<IDiagramNodeItem>().ToArray()
        //};
    }


}
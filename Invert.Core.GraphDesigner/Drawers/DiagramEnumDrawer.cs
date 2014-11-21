using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class DiagramEnumDrawer : DiagramNodeDrawer<EnumNodeViewModel>
{
    private SectionHeaderDrawer _itemsHeader;

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

}
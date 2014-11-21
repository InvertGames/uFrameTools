using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class SubSystemDrawer : DiagramNodeDrawer<SubSystemViewModel>
{
    private SectionHeaderDrawer _instancesHeader;


    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader1; }
    }
  

    public SubSystemDrawer()
    {
    }

    public SubSystemDrawer(SubSystemViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public override GUIStyle ItemStyle
    {
        get { return ElementDesignerStyles.Item4; }
    }

}
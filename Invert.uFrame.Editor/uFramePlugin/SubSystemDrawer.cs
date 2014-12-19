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


    protected override object HeaderStyle
    {
        get { return CachedStyles.NodeHeader1; }
    }
  

    public SubSystemDrawer()
    {
    }

    public SubSystemDrawer(SubSystemViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public override object ItemStyle
    {
        get { return CachedStyles.Item4; }
    }

}
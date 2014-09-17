using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class SubSystemDrawer : DiagramNodeDrawer<SubSystemViewModel>
{
    private NodeItemHeader _instancesHeader;

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        //base.GetContentDrawers(drawers);
        drawers.Add(InstancesHeader);
        foreach (var item in ViewModel.ContentItems.OfType<RegisterInstanceItemViewModel>())
        {
            var drawer = uFrameEditor.CreateDrawer(item);
            if (drawer == null) Debug.Log(string.Format("Couldn't create drawer for {0} make sure it is registered.", item.GetType().Name));
            drawers.Add(drawer);
        }
    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader1; }
    }
    public NodeItemHeader InstancesHeader
    {
        get
        {
            if (_instancesHeader != null) return _instancesHeader;

            _instancesHeader = new NodeItemHeader(ViewModel)
            {
                Label = "Instances",
                HeaderType = typeof(RegisterInstanceItemViewModel),

            };

            if (NodeViewModel.IsLocal)
                _instancesHeader.AddCommand = uFrameEditor.Container.Resolve<AddInstanceCommand>();

            return _instancesHeader;
        }
        set { _instancesHeader = value; }
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
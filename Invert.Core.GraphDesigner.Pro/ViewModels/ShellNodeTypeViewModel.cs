using System.Linq;
using Invert.Core.GraphDesigner;

public class ShellNodeTypeViewModel : GenericNodeViewModel<ShellNodeTypeNode>
{
    public ShellNodeTypeViewModel(ShellNodeTypeNode graphItemObject, DiagramViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
    {
    }

    protected override void CreateContent()
    {
        base.CreateContent();

        if (GraphItem != InvertGraphEditor.CurrentProject.CurrentFilter)
        {
            //ContentItems.Add(new SectionHeaderViewModel()
            //{
            //    Name = "Preview"
            //});

            //foreach (var item in GraphItem.InputSlots)
            //{
            //    var input = new InputOutputViewModel()
            //    {
            //        Name = item.Name + " Type",
            //        IsInput = true,
            //        InputConnectorType = typeof(void),
            //        DataObject =item.GetConnectionReference<ReferenceItemType>(),
            //    };
            //    ContentItems.Add(input);
             

            //}
            //foreach (var section in GraphItem.Sections)
            //{
            //    var sectionVM = new SectionHeaderViewModel()
            //    {
            //        Name = section.Name,
            //        AddCommand = new SimpleEditorCommand<DiagramNodeViewModel>(_ => { },section.Name)
            //    };
            //    ContentItems.Add(sectionVM);
            //}
            //foreach (var item in GraphItem.OutputSlots)
            //{
                
            //    var input = new InputOutputViewModel()
            //    {
            //        Name = item.Name + " Type",
            //        IsOutput = true,
            //        InputConnectorType = typeof(void),
            //        DataObject = item.GetConnectionReference<ReferenceItemType>(),
            //    };
            //    ContentItems.Add(input);
            //    //input.InputConnectorType = typeof(IShellReferenceType);
            //    //input.InputConnector.AllowMultiple = false;
            //    //input.DataObject = item.GetConnectionReference<ReferenceItemType>();
            //    //input.InputConnector.DataObject = item;
            //    //input.InputConnector.DataObject = item.GetConnectionReference<ReferenceItemType>();
            //}
        }
    }

    public void AddReferenceSection()
    {
        
    }

    public void AddSection()
    {
        
    }

    public void AddInput()
    {
        
    }

    public void AddOutput()
    {
        
    }
}
using System.CodeDom;
using Invert.uFrame.Editor;

public class ViewComponentGenerator : ViewClassGenerator
{

    public ViewComponentData ViewComponentData
    {
        get; set;
    }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        AddViewComponent(ViewComponentData);
    }

    public void AddViewComponent(ViewComponentData componentData)
    {
        var element = componentData.View;
        if (element == null) return;

        var baseComponent = componentData.Base;

        var ctr = baseComponent == null
            ? new CodeTypeReference(uFrameEditor.UFrameTypes.ViewComponent)
            : new CodeTypeReference(baseComponent.Name);


        var decl = new CodeTypeDeclaration()
        {
            Name = componentData.Name,
            Attributes = MemberAttributes.Public,
            IsPartial = true
        };
        if (IsDesignerFile)
        {
            decl.BaseTypes.Add(ctr);

            if (baseComponent == null)
            {
                decl.CreateViewModelProperty(ViewComponentData.View.ViewForElement);
                AddExecuteMethods(ViewComponentData.View.ViewForElement, decl, true);
            }
        }
        Namespace.Types.Add(decl);
    }
}
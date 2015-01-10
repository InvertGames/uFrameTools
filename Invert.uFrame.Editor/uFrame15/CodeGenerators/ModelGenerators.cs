using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner;
using Invert.uFrame.CodeGen.ClassNodeGenerators;
using Invert.uFrame.Editor;
using UnityEditor;


public class ModelClassNodeCodeFactory : ClassNodeCodeFactory<ModelClassGenerator, ModelClassNodeData>
{
    public override string EditableFileDirectory
    {
        get { return Path.Combine("Data","Classes"); }
    }

    public override IEnumerable<OutputGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
        ModelClassNodeData item)
    {
        //var list =  base.CreateGenerators(settings, pathStrategy, diagramData, item).ToArray();
        //foreach (var i in list) yield return i;
        yield return new ModelClassInterfaceGenerator()
        {
            ObjectData = item,
            IsDesignerFile = true,
            Filename = pathStrategy.GetDesignerFilePath(DesignerFilePostFix)
        };
    }
}

public class ModelClassInterfaceGenerator : ClassNodeGenerator<ModelClassNodeData>
{
    public override bool ShouldImplementINotifyPropertyChanged
    {
        get { return false; }
    }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        Decleration.IsPartial = false;
        Decleration.Name = "I" + Data.Name;
        Decleration.IsInterface = true;
    }


    protected override CodeMemberField CreateCollectionPropertyField(ClassCollectionData p)
    {
        return null;
    }

    protected override void SetBaseTypes(ClassNodeData baseType)
    {
        //base.SetBaseTypes(baseType);
        Decleration.BaseTypes.Add("I" + baseType.Name);
    }
}
public class ModelClassGenerator : ClassNodeGenerator<ModelClassNodeData>
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        Namespace.Imports.Add(new CodeNamespaceImport("UnityEngine"));
    }

    protected override void SetDefaultBaseTypes()
    {
        Decleration.BaseTypes.Add("UnityEngine.ScriptableObject");
        base.SetDefaultBaseTypes();

    }

    protected override CodeMemberField CreatePropertyField(ClassPropertyData p)
    {
        var field = base.CreatePropertyField(p);
        field.CustomAttributes.Add(new CodeAttributeDeclaration("SerializeField"));
        return field;
    }
}
public class RepositoryEditorFactory : DesignerGeneratorFactory<IGraphData>
{
    public override IEnumerable<OutputGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData, IGraphData item)
    {
        yield return new ModelAssetEditorGenerator()
        {
            IsDesignerFile = true,
            DesignerData = diagramData,
            Filename = Path.Combine("Data", Path.Combine("Editor", diagramData.Name + "AssetsEditor.designer.cs"))
        };
    }
}

public class ModelAssetEditorGenerator : CodeGenerator
{
    public INodeRepository DesignerData { get; set; }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        Decleration = new CodeTypeDeclaration(DesignerData.Name + "AssetsEditor");
        Decleration.Attributes = MemberAttributes.Static;
        var priority = 1;

        foreach (var elementData in DesignerData.NodeItems.OfType<ModelClassNodeData>().OrderBy(p => p.Name))
        {
            var method = new CodeMemberMethod()
            {
                Name = "Create" + elementData.Name,
                Attributes = MemberAttributes.Static
            };
            method.CustomAttributes.Add(
                new CodeAttributeDeclaration(new CodeTypeReference(typeof(MenuItem)), new CodeAttributeArgument(new CodePrimitiveExpression(string.Format("Assets/{0}/New {1}", DesignerData.Name, elementData.Name)))
                    , new CodeAttributeArgument(new CodePrimitiveExpression(false))
                    , new CodeAttributeArgument(new CodePrimitiveExpression(priority))));

            priority++;

            method.Statements.Add(
                new CodeSnippetExpression(string.Format("UFrameAssetManager.CreateAsset<{0}>();",
                    elementData.Name)));
            Decleration.Members.Add(method);
        }
        Namespace.Types.Add(Decleration);
    }

    public CodeTypeDeclaration Decleration { get; set; }
}


using System.CodeDom;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Invert.uFrame.Editor;
using UnityEngine;

public class ViewGenerator : ViewClassGenerator
{
    public ViewData View
    {
        get;
        set;
    }

    public void CreateViewViewBase()
    {

    }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);

        //AddViewBase();

        Decleration =  new CodeTypeDeclaration(View.NameAsView) { IsPartial = true };
        if (View.ViewForElement != null)
        {
            if (View.ViewForElement.IsTemplate)
            {
                Decleration.TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public;
            }
        }
        else
        {
            Decleration.TypeAttributes = TypeAttributes.Public;
        }
        if (IsDesignerFile)
        {
            Decleration.BaseTypes.Add(View.NameAsViewViewBase);
        }
        else
        {
            var bindingGenerators = uFrameEditor.GetBindingGeneratorsForView(View).ToArray();

            foreach (var bindingGenerator in bindingGenerators)
            {
                bindingGenerator.IsOverride = !IsDesignerFile;
                bindingGenerator.CreateMembers(Decleration.Members);
            }
        }
        Namespace.Types.Add(Decleration);

        //if (View.BaseNode == null)
        //{
        //    return;
        //}
        //if (IsDesignerFile)
        //{
        //    AddViewBase(View.ViewForElement as ElementData, View.NameAsViewViewBase, View.BaseViewName);
        //}
        //else
        //{
        //    AddView(View);
        //}
        //var baseView = View.BaseView;
        //if (baseView != null && IsDesignerFile)
        //{
        //    AddView(View);
        //    AddViewBase(View.ViewForElement as ElementData, View.NameAsViewViewBase, baseView.NameAsView);
        //}
        //else
        //{
        //    AddView(View);
        //    if (IsDesignerFile)
        //    {
        //        AddViewBase(View.ViewForElement as ElementData, View.NameAsViewViewBase, View.BaseViewName);    
        //    }

        //}
    }

    public void AddView(ViewData view)
    {
        var decl = new CodeTypeDeclaration(view.NameAsView) { IsPartial = true };
        if (view.ViewForElement != null)
        {
            if (view.ViewForElement.IsTemplate)
            {
                decl.TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public;
            }
        }
        else
        {
            decl.TypeAttributes = TypeAttributes.Public;
        }
        if (IsDesignerFile)
        {
            var viewViewBase = View.BaseView != null;
            decl.BaseTypes.Add(new CodeTypeReference(viewViewBase ? view.NameAsViewViewBase : view.BaseViewName));
            //decl.Members.Add(CreateUpdateMethod(view, decl));
            GenerateBindMethod(decl, view);
        }
        else
        {
            foreach (var bindingGenerator in view.NewBindings.Select(p=>p.Generator))
            {
                bindingGenerator.CreateMembers(decl.Members);
            }
            //GenerateBindingMembers(decl,view.ViewForElement,true);
            var bindMethod = new CodeMemberMethod()
            {
                Name = "Bind",
                Attributes = MemberAttributes.Override | MemberAttributes.Public
            };
            decl.Members.Add(bindMethod);
            bindMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Bind"));

        }
        Namespace.Types.Add(decl);
    }
}

public class ViewViewBaseGenerator : ViewClassGenerator
{
    public ViewData View { get; set; }

    public void CreateBindMethod()
    {
        var bindMethod = new CodeMemberMethod()
        {
            Name = "Bind",
            Attributes = MemberAttributes.Public | MemberAttributes.Override

        };

        bindMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Bind"));


    }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        var forElement = View.ViewForElement;

        Decleration = new CodeTypeDeclaration()
        {
            Name = View.NameAsViewViewBase,
            Attributes = MemberAttributes.Public | MemberAttributes.Abstract,
        };

        Decleration.BaseTypes.Add(View.BaseViewName);

        AddCreateModelMethod(forElement);
        //CreateBindMethod();
        // implement abstract method bind defined in the viewbase class
        //var bindMethod = new CodeMemberMethod()
        //{
        //    Name = "Bind",
        //    Attributes = MemberAttributes.Override | MemberAttributes.Public
        //};
        //Decleration.Members.Add(bindMethod);
        AddComponentReferences(Decleration);
        AddBindingMembers();
        
        //foreach (var viewBindingExtender in BindingExtenders)
        //{
        //    viewBindingExtender.ExtendViewBase(Decleration, View);
        //}

        var bindMethod = GenerateBindMethod(Decleration, View);
        //foreach (var item in View.Properties)
        //{
        //    bindMethod.Statements.Add(new CodeAssignStatement(new CodeSnippetExpression(string.Format("{0}.{1}", View.ViewForElement.Name, item.NameAsProperty)),
        //                new CodeSnippetExpression(item.Expression)));
        //}
        // Make sure we only generate a view model property for whats needed
        if (View.BaseView != null)
        {
            AddViewModelProperty(forElement);
          
            AddInitializeViewModelMethod(forElement);
            AddExecuteMethods(forElement, Decleration);
            AddViewModelTypeProperty(forElement);
            if (forElement.IsRegistered)
            {
                AddDefaultIdentifierProperty(forElement);
            }
        }
        //CreateUpdateMethod(View, Decleration);
        Namespace.Types.Add(Decleration);
    }
    protected void AddComponentReferences(CodeTypeDeclaration decl)
    {
        
        
        foreach (var viewComponentData in this.View.ViewComponents)
        {
            decl.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof (RequireComponent)),
                new CodeAttributeArgument(new CodeSnippetExpression(
                    string.Format("typeof({0})",viewComponentData.Name)))));

            var backingField = new CodeMemberField(viewComponentData.Name, "_" + viewComponentData.Name);
            backingField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializeField))));
            backingField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(HideInInspector))));
            var property = new CodeMemberProperty()
            {
                Type = new CodeTypeReference(viewComponentData.Name),
                Name = viewComponentData.Name,
                Attributes = MemberAttributes.Public,
                HasGet = true,
                HasSet = true
            };

            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression(
                string.Format("{0} ?? ({0} = this.gameObject.EnsureComponent<{1}>())", backingField.Name, viewComponentData.Name))));
            property.SetStatements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name),
                    new CodePropertySetValueReferenceExpression()));
            decl.Members.Add(backingField);
            decl.Members.Add(property);
        }
    }

    private void AddBindingMembers()
    {
        var bindingGenerators =
            uFrameEditor.GetBindingGeneratorsForView(View);

        foreach (var bindingGenerator in bindingGenerators)
        {
            bindingGenerator.IsOverride = !IsDesignerFile;
            bindingGenerator.CreateMembers(Decleration.Members);
        }
    }
}
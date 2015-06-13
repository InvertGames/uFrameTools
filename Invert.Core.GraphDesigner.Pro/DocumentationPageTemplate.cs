﻿using System;
using System.Reflection;
using Invert.Core.GraphDesigner;

[TemplateClass(TemplateLocation.Both,"{0}Page")]
public class DocumentationPageTemplate : DocumentationPage, IClassTemplate<IDocumentable>
{
    public string OutputPath
    {
        get { return Path2.Combine("Documentation","Editor", "Pages"); }
    }

    public bool CanGenerate
    {
        get { return true; }
    }
    
    public void TemplateSetup()
    {

        var className = Ctx.Data.Node.Name + "Page";
        if (Ctx.Data.Node != Ctx.Data)
        {
            className = Ctx.Data.Node.Name.Clean() + Ctx.Data.Name.Clean() + "Page";
        }
        if (Ctx.IsDesignerFile)
        {
            className += "Base";
        }
        Ctx.CurrentDecleration.Name = className;


        if (Ctx.IsDesignerFile)
        {
            if (this.Ctx.Data.Node.Graph.RootFilter != Ctx.Data)
            {
                Ctx.SetBaseType(this.Ctx.Data.Node.Graph.RootFilter.Name.Clean() + "Page");
            }
         
        }
        else
        {
            Ctx.SetBaseType(className + "Base");
        }

        if (Ctx.IsDesignerFile || this.Ctx.Data.Node.Graph.RootFilter == Ctx.Data)
        {
            Ctx.CurrentDecleration.TypeAttributes |= TypeAttributes.Abstract;
            //Ctx.CurrentDecleration.Attributes = MemberAttributes.Abstract | MemberAttributes.Public;
        }
        
    }

    public TemplateContext<IDocumentable> Ctx { get; set; }

    [GenerateProperty(TemplateLocation.DesignerFile)]
    public override Type ParentPage
    {
        get
        {
            if (Ctx.Data != Ctx.Data.Node.Graph.RootFilter)
            {
                if (Ctx.Data.Node != Ctx.Data)
                {
                    Ctx._("return typeof({0}PageBase)", Ctx.Data.Node.Name);
                }
                else
                {
                    Ctx._("return null");
                }
                    
            }
            else
            {
                Ctx._("return null");
            }
            return null;
        }
    }

    [GenerateProperty(TemplateLocation.DesignerFile)]
    public override Type RelatedNodeType
    {
        get
        {
            var classType = Ctx.Data as IClassTypeNode;
            if (classType != null)
            {
                Ctx._("return typeof({0})", classType.ClassName);
            }
            else
            {
                Ctx._("return typeof({0})", Ctx.Data.Name);
            }
            
            return null;
        }
        set { base.RelatedNodeType = value; }
    }

    [GenerateProperty(TemplateLocation.DesignerFile)]
    public override string Name
    {
        get
        {
            if (Ctx.CurrentDecleration.TypeAttributes == TypeAttributes.Abstract)
            {
                Ctx._("return base.Name");
            }
            else
            {
                Ctx._("return \"{0}\"", Ctx.Data.Name);
            }
           
            return null;
        }
    }
    [GenerateMethod(TemplateLocation.Both)]
    public override void GetContent(IDocumentationBuilder _)
    {
        
    }
}
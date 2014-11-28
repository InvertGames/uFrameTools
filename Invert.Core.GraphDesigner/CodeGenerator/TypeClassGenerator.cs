using System;
using System.CodeDom;
using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public class TypeClassGenerator<TData,TTemplateType> : CodeGenerator where TData : DiagramNode
    {
        private CodeTypeDeclaration _decleration;
        public Type TemplateType
        {
            get { return typeof (TTemplateType); }
        }

        public virtual bool IsDesignerFileOnly
        {
            get { return false; }
        }
        public TData Data
        {
            get { return ObjectData as TData; }
            set { ObjectData = value; }
        }
        public CodeTypeDeclaration Decleration
        {
            get
            {
                return _decleration;
            }
            set { _decleration = value; }
        }

        public virtual string ClassNameFormat
        {
            get { return "{0}"; }
        }

        public virtual string ClassName(DiagramNode node)
        {
            var typeNode = node as IClassTypeNode;
            if (typeNode != null)
            {
                return typeNode.ClassName;
            }
            return string.Format(ClassNameFormat, node.Name);
        }

        public virtual string ClassNameBase(DiagramNode node)
        {
            if (IsDesignerFileOnly)
                return ClassName(node);

            return ClassName(node) + "Base";
        }
        public override void Initialize(CodeFileGenerator fileGenerator)
        {
            base.Initialize(fileGenerator);
            if (!string.IsNullOrEmpty(TemplateType.Namespace))
                TryAddNamespace(TemplateType.Namespace);
            Decleration = TemplateType.ToClassDecleration();

            var inheritable = Data as GenericInheritableNode;

            if (IsDesignerFile)
            {
                Decleration.Name = ClassNameBase(Data);
                if (inheritable != null && inheritable.BaseNode != null)
                {
                 
                    Decleration.BaseTypes.Clear();
                    Decleration.BaseTypes.Add(ClassName(inheritable.BaseNode));
                }
         
            }
            else
            {
                Decleration.Name = ClassName(Data);
                Decleration.BaseTypes.Clear();
                Decleration.BaseTypes.Add(ClassNameBase(Data));
            }
            
            Namespace.Types.Add(Decleration);
            if (IsDesignerFile)
            {
                base.Initialize(fileGenerator);
                
                if (IsDesignerFile)
                {
                    InitializeDesignerFile();
                }
                else
                {
                    InitializeEditableFile();
                }
            }
        }

        protected virtual void InitializeEditableFile()
        {
                
        }

        protected virtual void InitializeDesignerFile()
        {
            
        }
        protected void OverrideMethod<TItem>(string name, IEnumerable<TItem> selector, Func<CodeMemberMethod, TItem, CodeMemberMethod> postProcess)
        {
            foreach (var item in selector)
            {
                TItem item1 = item;
                var i = item1 as IDiagramNodeItem;

                OverrideMethod(name, (m) =>
                {
                    if (i != null)
                    {
                        m.Name = i.Name;
                    }
                    return postProcess(m, item1);
                });
                
            }
        }
        protected void OverrideMethod(string name, Func<CodeMemberMethod,CodeMemberMethod> postProcess)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (postProcess == null) throw new ArgumentNullException("postProcess");

            var method = TemplateType.MethodFromTypeMethod(name, false);
            var result = postProcess(method);
            if (result != null)
            {
                Decleration.Members.Add(result);
            }
            
        }

        protected void OverrideProperty(string name, Func<CodeMemberProperty, CodeMemberProperty> postProcess)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (postProcess == null) throw new ArgumentNullException("postProcess");

            var oroperty = TemplateType.PropertyFromTypeProperty(name, true);
            var result = postProcess(oroperty);

            if (result != null)
            {
        
                if (oroperty.HasGet && oroperty.GetStatements.Count < 1)
                {
                    var field = Decleration._private_(oroperty.Type.ToString(), "_" + oroperty.Name);
                    oroperty._get("return {0}",field.Name);

                    if (oroperty.HasSet && oroperty.SetStatements.Count < 1)
                    {
                        oroperty._set("{0} = value", field.Name);
                    }
                }
                Decleration.Members.Add(result);
            }

        }
        protected void OverrideProperty<TItem>(string name, IEnumerable<TItem> selector, Func<CodeMemberProperty, TItem, CodeMemberProperty> postProcess)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (postProcess == null) throw new ArgumentNullException("postProcess");
            foreach (var item in selector)
            {
                var property = TemplateType.PropertyFromTypeProperty(name, true);
                var typed = item as GenericTypedChildItem;
                if (!IsDesignerFile && property.Attributes != MemberAttributes.Final)
                {
                    property.Attributes |= MemberAttributes.Override;
                }
                if (typed != null)
                {
                 
                    if (property.Type.TypeArguments.Count > 0)
                        property.Type.TypeArguments[0] = new CodeTypeReference(typed.RelatedTypeName);
                    else
                    {
                        property.Type = new CodeTypeReference(typed.RelatedTypeName);
                    }
                }
                var graphItem = item as IDiagramNodeItem;
                if (graphItem != null)
                {
                    property.Name = graphItem.Name;
                }
                var result = postProcess(property,item);
                if (result != null)
                {
                    if (property.HasGet && property.GetStatements.Count < 1)
                    {
                        var field = Decleration._private_(property.Type, "_" + property.Name);
                        property._get("return {0}", field.Name);

                        if (property.HasSet && property.SetStatements.Count < 1)
                        {
                            property._set("{0} = value", field.Name);
                        }
                    }
                    Decleration.Members.Add(result);
                }
            } 
            
        }
    }

    public interface IClassTypeNode
    {
        string ClassName { get; }
    }
}
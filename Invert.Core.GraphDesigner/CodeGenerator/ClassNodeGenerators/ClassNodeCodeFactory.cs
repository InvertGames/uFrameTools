using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;

namespace Invert.uFrame.CodeGen.ClassNodeGenerators
{
    public class ClassNodeCodeFactory<TClassGenerator, TClassData> : DesignerGeneratorFactory<TClassData> 
        where TClassData : ClassNodeData where TClassGenerator : ClassNodeGenerator<TClassData>, new()
    {

        public virtual string DesignerFilePostFix { get { return "Classes"; } }
        public virtual string EditableFileDirectory { get { return "Types"; } }


        public override IEnumerable<CodeGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
            TClassData item)
        {
            yield return new TClassGenerator()
            {
                Data = item,
                IsDesignerFile = false,
                Filename = Path.Combine(EditableFileDirectory, item.Name + ".cs")
            };
            yield return new TClassGenerator()
            {
                Data = item,
                IsDesignerFile = true,
                Filename = pathStrategy.GetDesignerFilePath(DesignerFilePostFix)
            };
        }

      
    }

    public class ClassNodeGenerator<TData> : CodeGenerator where TData : ClassNodeData
    {
        public TData Data { get; set; }

        public virtual bool ShouldImplementINotifyPropertyChanged
        {
            get
            {
                return true;
            }
        }
        public override void Initialize(CodeFileGenerator fileGenerator)
        {
            base.Initialize(fileGenerator);
            Namespace.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
            Namespace.Imports.Add(new CodeNamespaceImport("System.Collections.ObjectModel"));
            Decleration = new CodeTypeDeclaration(Data.Name) {Attributes = MemberAttributes.Public, IsPartial = true};

            if (IsDesignerFile)
            {
                var baseType = Data.BaseClass;
                if (baseType != null)
                {
                    SetBaseTypes(baseType);
                }
                else
                {
                    SetDefaultBaseTypes();
                    if (ShouldImplementINotifyPropertyChanged)
                    ImplementINotifyPropertyChanged();
                }


                foreach (var p in Data.Properties)
                {
                    var field = CreatePropertyField(p);
                    var property = CreateProperty(field, p);
                    if (field != null)
                    Decleration.Members.Add(field);
                    if (property != null)
                    Decleration.Members.Add(property);
                }
                foreach (var p in Data.Collections)
                {
                    var field = CreateCollectionPropertyField(p);
                    var property = CreateCollectionProperty(field, p);
                    if (field != null)
                    Decleration.Members.Add(field);
                    if (property != null)
                    Decleration.Members.Add(property);
                }
            }

            Namespace.Types.Add(Decleration);
        }

        private void ImplementINotifyPropertyChanged()
        {
            Decleration.Members.Add(
                new CodeSnippetTypeMember("public event PropertyChangedEventHandler PropertyChanged;"));

            Decleration.Members.Add(
                new CodeSnippetTypeMember(
                    "protected virtual void OnPropertyChanged(string propertyName){ PropertyChangedEventHandler handler = PropertyChanged; if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName)); }"));
        }

        protected virtual void SetDefaultBaseTypes()
        {
            Decleration.BaseTypes.Add(new CodeTypeReference(typeof (INotifyPropertyChanged)));
        }

        protected virtual void SetBaseTypes(ClassNodeData baseType)
        {
            Decleration.BaseTypes.Add(new CodeTypeReference(baseType.Name));
        }

        protected virtual  CodeMemberField CreateCollectionPropertyField(ClassCollectionData p)
        {
            var field = new CodeMemberField(string.Format("List<{0}>", p.RelatedTypeName), "_" + p.Name);
            return field;
        }

        protected virtual CodeMemberProperty CreateCollectionProperty(CodeMemberField field, ClassCollectionData p)
        {
            if (field == null) return null;
            var property = field.EncapsulateField(p.Name);
            return property;
        }

        protected virtual CodeMemberField CreatePropertyField(ClassPropertyData p)
        {
            var field = new CodeMemberField(p.RelatedTypeName, "_" + p.Name);
            return field;
        }

        protected virtual CodeMemberProperty CreateProperty(CodeMemberField field, ClassPropertyData p)
        {
            if (field == null) return null;
            var property = field.EncapsulateField(p.Name);
            property.SetStatements.Add(
                new CodeSnippetExpression(string.Format("OnPropertyChanged(\"{0}\")", property.Name)));
            return property;
        }

        public CodeTypeDeclaration Decleration { get; set; }
    }

    public class SimpleClassNodeCodeFactory : ClassNodeCodeFactory<SimpleClassNodeGenerator, ClassNodeData>
    {
        
    }

    public class SimpleClassNodeGenerator : ClassNodeGenerator<ClassNodeData>
    {
        public override void Initialize(CodeFileGenerator fileGenerator)
        {
            base.Initialize(fileGenerator);
            TryAddNamespace("UnityEngine");
        }
    }
}

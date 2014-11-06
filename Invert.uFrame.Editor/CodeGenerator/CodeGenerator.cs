using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using Invert.uFrame.Editor;

namespace Invert.uFrame.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ShowInSettings : Attribute
    {
        public ShowInSettings(string @group)
        {
            Group = @group;
        }

        public string Group { get; set; }
    }
    public abstract class CodeGenerator
    {
        private CodeNamespace _ns;
        private CodeCompileUnit _unit;

        public string FullPathName
        {
            get { return Path.Combine(AssetPath, Filename).Replace("\\", "/"); }
        }
        public string RelativeFullPathName
        {
            get { return Path.Combine(AssetPath, Filename).Replace("\\", "/").Substring(7); }
        }
        public void TryAddNamespace(string ns)
        {
            foreach (CodeNamespaceImport n in _ns.Imports)
            {
                if (n.Namespace == ns)
                    return;
            }
            _ns.Imports.Add(new CodeNamespaceImport(ns));
        }
        public virtual Type RelatedType
        {
            get;
            set;
        }

        public virtual string Filename
        {
            get;
            set;
        }

        public virtual void Initialize(CodeFileGenerator fileGenerator)
        {
            _unit = fileGenerator.Unit;
            _ns = fileGenerator.Namespace;
        }

        public CodeNamespace Namespace
        {
            get { return _ns; }
        }

        public CodeCompileUnit Unit
        {
            get { return _unit; }
        }

        public bool IsDesignerFile
        {
            get;
            set;
        }

        public Type GeneratorFor { get; set; }
        public object ObjectData { get; set; }
        public GeneratorSettings Settings { get; set; }
        public string AssetPath { get; set; }

        public bool IsEnabled(IProjectRepository project)
        {

            var customAttribute = this.GetType().GetCustomAttributes(typeof(ShowInSettings), true).OfType<ShowInSettings>().FirstOrDefault();
            if (customAttribute == null) return true;

            return project.GetSetting(customAttribute.Group, true);

        }
        public void ProcessModifiers(CodeTypeDeclaration declaration)
        {

            var typeDeclerationModifiers = uFrameEditor.Container.ResolveAll<ITypeGeneratorPostProcessor>().Where(p => p.For.IsAssignableFrom(this.GetType()));
            foreach (var typeDeclerationModifier in typeDeclerationModifiers)
            {
                //typeDeclerationModifier.FileGenerator = codeFileGenerator;
                typeDeclerationModifier.Declaration = declaration;

                typeDeclerationModifier.Generator = this;
                //uFrameEditor.Log("Processed: " + typeDeclerationModifier.GetType().Name);
                typeDeclerationModifier.Apply();
            }

        }
    }

    public abstract class NodeCodeGenerator<TData> : CodeGenerator where TData : DiagramNode
    {
        private CodeTypeDeclaration _decleration;
        public TData Data { get; set; }

        public virtual string NameAsClass
        {
            get { return Data.Name; }
        }

        public virtual string NameAsDesignerClass
        {
            get { return Data.Name + "Base"; }
        }

        public override void Initialize(CodeFileGenerator fileGenerator)
        {
            base.Initialize(fileGenerator);
            if (IsDesignerFile)
            {
                InitializeDesignerFile();
                Namespace.Types.Add(Decleration);
            }
            else
            {
                Decleration.BaseTypes.Add(NameAsDesignerClass);
                InitializeEditableFile();
                Namespace.Types.Add(Decleration);
            }
        }

        protected abstract void InitializeEditableFile();

        protected abstract void InitializeDesignerFile();

        public CodeTypeDeclaration Decleration
        {
            get
            {
                if (_decleration == null)
                {
                    _decleration = new CodeTypeDeclaration(IsDesignerFile ? NameAsClass : NameAsDesignerClass);
                    _decleration.IsPartial = true;
                }
                return _decleration;
            }
            set { _decleration = value; }
        }
    }

}

namespace Invert.uFrame.Code.Bindings
{
    public interface IBindingGenerator
    {
        string Title { get; }
        string Description { get; }
        string MethodName { get; }
        ITypeDiagramItem Item { get; set; }
        bool IsApplicable { get; }
        bool IsOverride { get; set; }
        bool CallBase { get; set; }
        string BindingConditionFieldName { get; }
        bool GenerateDefaultImplementation { get; set; }
        ElementData Element { get; set; }
        bool IsBase { get; set; }
        void CreateMembers(CodeTypeMemberCollection collection);
        void CreateBindingStatement(CodeTypeMemberCollection collection, CodeConditionStatement bindingCondition);
    }

}
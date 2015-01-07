using System;
using System.CodeDom;
using System.IO;
using System.Linq;

namespace Invert.Core.GraphDesigner
{
    public abstract class OutputGenerator
    {
        public virtual string Filename
        {
            get;
            set;
        }

        public virtual Type GeneratorFor { get; set; }

        public object ObjectData { get; set; }
        public GeneratorSettings Settings { get; set; }
        public string AssetPath { get; set; }

        public string FullPathName
        {
            get { return Path.Combine(AssetPath, Filename).Replace("\\", "/"); }
        }

        public string RelativeFullPathName
        {
            get { return Path.Combine(AssetPath, Filename).Replace("\\", "/").Substring(7); }
        }

        public virtual void Initialize(CodeFileGenerator codeFileGenerator)
        {
            
        }

    
        public bool IsEnabled(IProjectRepository project)
        {

            var customAttribute = this.GetType().GetCustomAttributes(typeof(ShowInSettings), true).OfType<ShowInSettings>().FirstOrDefault();
            if (customAttribute == null) return true;

            return project.GetSetting(customAttribute.Group, true);

        }

        public virtual bool IsValid(FileInfo fileInfo)
        {
           
            return true;
        }
        public bool AlwaysRegenerate { get; set; }
    }

    public abstract class CodeGenerator : OutputGenerator
    {
        public override bool IsValid(FileInfo fileInfo)
        {
            if (IsDesignerFile) return true;
            return RelatedType == null;
        }

        private CodeNamespace _ns;
        private CodeCompileUnit _unit;
        public override void Initialize(CodeFileGenerator codeFileGenerator)
        {
            base.Initialize(codeFileGenerator);
            _unit = codeFileGenerator.Unit;
            _ns = codeFileGenerator.Namespace;
            
            TryAddNamespace("System");
            TryAddNamespace("System.Collections");
            TryAddNamespace("System.Collections.Generic");
            TryAddNamespace("System.Linq");
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
            get { return AlwaysRegenerate;}
            set { AlwaysRegenerate = value; }
        }

        public void ProcessModifiers(CodeTypeDeclaration declaration)
        {

            var typeDeclerationModifiers = InvertApplication.Container.ResolveAll<ITypeGeneratorPostProcessor>().Where(p => p.For.IsAssignableFrom(this.GetType()));
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
}
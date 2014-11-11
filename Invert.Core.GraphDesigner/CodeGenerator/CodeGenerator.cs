using System;
using System.CodeDom;
using System.Collections;
using System.IO;
using System.Linq;
using Invert.Core;
using Invert.uFrame.Editor;

namespace Invert.uFrame.Editor
{
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
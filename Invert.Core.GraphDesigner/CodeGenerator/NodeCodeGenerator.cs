using System.CodeDom;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor
{
    public abstract class NodeCodeGenerator<TData> : CodeGenerator where TData : GenericNode
    {
        private CodeTypeDeclaration _decleration;
        private TData _data;

        public TData Data
        {
            get { return ObjectData as TData; }
            set { ObjectData = value; }
        }

        public virtual string NameAsClass
        {
            get { return Data.Name; }
        }

        public virtual string NameAsDesignerClass
        {
            get { return Data.Name + "Base"; }
        }

        public sealed override void Initialize(CodeFileGenerator fileGenerator)
        {
            base.Initialize(fileGenerator);
            var nodeConfig = InvertGraphEditor.Container.GetNodeConfig<TData>();
            if (!nodeConfig.TypeGeneratorConfigs.ContainsKey(this.GetType())) return;

            GeneratorConfig = nodeConfig.TypeGeneratorConfigs[this.GetType()] as NodeGeneratorConfig<TData>;

            Decleration = new CodeTypeDeclaration(IsDesignerFile ? NameAsDesignerClass : NameAsClass)
            {
                IsPartial = true
            };
            Compose();
        }

        public NodeGeneratorConfig<TData> GeneratorConfig { get; set; }

        protected virtual void Compose()
        {
            if (IsDesignerFile)
            {

                var designerMemberGenerators = GeneratorConfig.GetChildGenerators(Decleration, Data, MemberGeneratorLocation.DesignerFile);
                foreach (var generator in designerMemberGenerators)
                {
                    Decleration.Members.Add(generator.Create(true));
                }
                var memberGenerators = GeneratorConfig.GetMemberGenerators(Decleration, Data, MemberGeneratorLocation.DesignerFile);
                foreach (var generator in memberGenerators)
                {
                    Decleration.Members.Add(generator.Create(true));
                }
                InitializeDesignerFile();
               
            }
            else
            {
                Decleration.BaseTypes.Add(NameAsDesignerClass);
                var designerMemberGenerators = GeneratorConfig.GetChildGenerators(Decleration, Data, MemberGeneratorLocation.EditableFile);
                foreach (var generator in designerMemberGenerators)
                {
                    Decleration.Members.Add(generator.Create(false));
                }
                var memberGenerators = GeneratorConfig.GetMemberGenerators(Decleration, Data, MemberGeneratorLocation.EditableFile);
                foreach (var generator in memberGenerators)
                {
                    Decleration.Members.Add(generator.Create(false));
                }
                InitializeEditableFile();
            }
            Namespace.Types.Add(Decleration);
        }

        protected abstract void InitializeEditableFile();

        protected abstract void InitializeDesignerFile();

        public CodeTypeDeclaration Decleration
        {
            get
            {
               
                return _decleration;
            }
            set { _decleration = value; }
        }

    }
}
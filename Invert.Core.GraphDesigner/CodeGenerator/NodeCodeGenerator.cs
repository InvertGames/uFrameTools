using System.CodeDom;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor
{
    public class NodeCodeGenerator<TData> : CodeGenerator where TData : GenericNode
    {
        private CodeTypeDeclaration _decleration;
        private TData _data;

        public TData Data
        {
            get { return ObjectData as TData; }
            set { ObjectData = value; }
        }

        //public override string Filename
        //{
        //    get
        //    {

        //        if (IsDesignerFile)
        //        {
        //            if (GeneratorConfig.DesignerFilename == null)
        //            {
        //                return base.Filename;
        //            }
        //            return GeneratorConfig.DesignerFilename.GetValue(Data);
        //        }
        //        else
        //        {
        //            if (GeneratorConfig.Filename == null)
        //            {
        //                return base.Filename;
        //            }
        //            return GeneratorConfig.Filename.GetValue(Data);
        //        }
                
        //    }
        //    set { base.Filename = value; }
            
        //}

        public virtual string NameAsClass
        {
            get { return GeneratorConfig.ClassName.GetValue(Data); }
        }

        public virtual string NameAsDesignerClass
        {
            get { return NameAsClass + "Base"; }
        }

        public sealed override void Initialize(CodeFileGenerator fileGenerator)
        {
            var nodeConfig = InvertGraphEditor.Container.GetNodeConfig<TData>();
            if (!nodeConfig.TypeGeneratorConfigs.ContainsKey(this.GetType())) return;

            GeneratorConfig = nodeConfig.TypeGeneratorConfigs[this.GetType()] as NodeGeneratorConfig<TData>;
            if (GeneratorConfig == null) return;
            if (GeneratorConfig.Condition != null && !GeneratorConfig.Condition(Data)) return;
            base.Initialize(fileGenerator);
         

            Decleration = new CodeTypeDeclaration(IsDesignerFile ? NameAsDesignerClass : NameAsClass)
            {
                IsPartial = true
            };
            Compose();
        }

        public NodeGeneratorConfig<TData> GeneratorConfig { get; set; }

        protected virtual void Compose()
        {
            if (GeneratorConfig.Namespaces != null)
            {
                foreach (var item in GeneratorConfig.Namespaces.GetValue(Data))
                {
                    TryAddNamespace(item);
                }
            }
            if (IsDesignerFile)
            {
                if (GeneratorConfig.DesignerDeclaration != null)
                {
                    Decleration = GeneratorConfig.DesignerDeclaration.GetValue(Data);
                }
                if (GeneratorConfig.BaseType != null)
                {
                    Decleration.BaseTypes.Add(GeneratorConfig.BaseType.GetValue(Data));
                }

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
                if (GeneratorConfig.Declaration != null)
                {
                    Decleration = GeneratorConfig.Declaration.GetValue(Data);
                }
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

        protected virtual void InitializeEditableFile()
        {
            
        }

        protected virtual void InitializeDesignerFile()
        {
            
        }

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
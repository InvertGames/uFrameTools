using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;

namespace Invert.Core.GraphDesigner
{
    public class GenericNodeGenerator<TData> : CodeGenerator where TData : GenericNode
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

        public override void Initialize(CodeFileGenerator fileGenerator)
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
                
                Decleration.Members.AddRange( GeneratorConfig.GetChildMembers(Decleration, Data, MemberGeneratorLocation.DesignerFile).ToArray());

                var memberGenerators = GeneratorConfig.GetMembers(Decleration, Data, MemberGeneratorLocation.DesignerFile);
                foreach (var generator in memberGenerators)
                {
                    Decleration.Members.Add(generator);
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
                var designerMemberGenerators = GeneratorConfig.GetChildMembers(Decleration, Data, MemberGeneratorLocation.EditableFile);
                foreach (var generator in designerMemberGenerators)
                {
                    Decleration.Members.Add(generator);
                }
                var memberGenerators = GeneratorConfig.GetMembers(Decleration, Data, MemberGeneratorLocation.EditableFile);
                foreach (var generator in memberGenerators)
                {
                    Decleration.Members.Add(generator);
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

        public void AddMembers<TFor>(Func<TData, IEnumerable<TFor>> selector, IMemberGenerator generator)
        {
            foreach (var item in selector(Data))
            {
                Decleration.Members.Add(generator.Create(Decleration, item, IsDesignerFile));
            }
        }

        public void AddMember<TFor>(Func<TData, TFor> selector, IMemberGenerator generator)
        {
            Decleration.Members.Add(generator.Create(Decleration, selector(Data), IsDesignerFile));
        }
    }
}
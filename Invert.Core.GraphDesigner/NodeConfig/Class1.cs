using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class NodeConfig<TType> : NodeConfig where TType : GenericNode, IConnectable
    {
        private List<Func<ConfigCodeGeneratorSettings, CodeGenerator>> _codeGenerators;
        private Dictionary<Type, NodeGeneratorConfig> _typeGeneratorConfigs;
        private List<string> _tags;

        public NodeConfig(IUFrameContainer container)
            : base(container)
        {
            container.Register<DesignerGeneratorFactory, CodeGeneratorFactory>("Config_" + typeof(TType).Name + "_Generator");
        }

        public class CodeGeneratorFactory : DesignerGeneratorFactory<TType>
        {
            public override IEnumerable<CodeGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
                TType item)
            {
                var config = Container.GetNodeConfig<TType>();
                if (config.CodeGenerators == null) yield break;

                var generatorArgs = new ConfigCodeGeneratorSettings()
                {
                    Settings = settings,
                    PathStrategy = pathStrategy,
                    Data = item,
                    Repository = diagramData
                };
                foreach (var generatorMethod in config.CodeGenerators)
                {
                    var result = generatorMethod(generatorArgs);
                    if (result != null)
                    {
                        yield return result;
                    }
                }

            }
        }
        public class ConfigCodeGeneratorSettings
        {
            public GeneratorSettings Settings { get; set; }
            public ICodePathStrategy PathStrategy { get; set; }
            public INodeRepository Repository { get; set; }
            public TType Data { get; set; }
        }

        public List<Func<ConfigCodeGeneratorSettings, CodeGenerator>> CodeGenerators
        {
            get
            {
                return _codeGenerators ?? (_codeGenerators = new List<Func<ConfigCodeGeneratorSettings, CodeGenerator>>());
            }
            set { _codeGenerators = value; }
        }


        public NodeConfig<TType> AddTag(string name)
        {
            Tags.Add(name);
            return this;
        }
        public NodeConfig<TType> ConnectsTo<TTarget>(bool oneToMany) where TTarget : class, IConnectable
        {
            return ConnectsTo<TTarget>(oneToMany, UnityEngine.Color.white);
        }

        public NodeConfig<TType> ConnectsTo<TTarget>(bool oneToMany, Color color) where TTarget : class, IConnectable
        {
            if (oneToMany)
                Container.RegisterInstance<IConnectionStrategy>(new OneToManyConnectionStrategy<TType, TTarget>(color), typeof(TType).Name + "_" + typeof(TTarget).Name + "Connection");
            else
            {
                Container.RegisterInstance<IConnectionStrategy>(new OneToOneConnectionStrategy<TType, TTarget>(color), typeof(TType).Name + "_" + typeof(TTarget).Name + "Connection");
            }
            return this;
        }
        public NodeConfig<TType> ConnectsFrom<TSource>(bool oneToMany, Color color) where TSource : class, IConnectable
        {
            if (oneToMany)
                Container.RegisterInstance<IConnectionStrategy>(new OneToManyConnectionStrategy<TSource, TType>(color), typeof(TSource).Name + "_" + typeof(TType).Name + "Connection");
            else
            {
                Container.RegisterInstance<IConnectionStrategy>(new OneToOneConnectionStrategy<TSource, TType>(color), typeof(TSource).Name + "_" + typeof(TType).Name + "Connection");
            }
            return this;
        }
        public NodeConfig<TType> Color(NodeColor color)
        {
            base.Color = color;
            return this;
        }

        public NodeConfig<TType> Section<TChildItem>(string header, Func<TType,IEnumerable<TChildItem>> selector = null)
        {
            var section = new NodeConfigSection<TType>()
            {
                ChildType = typeof (TChildItem),
                Name = header,
                IsProxy = true
            };
            if (selector != null)
            {
                section.Selector = p=>selector(p).Cast<IGraphItem>();
            }
            Sections.Add(section);
            return this;
        }
        public NodeConfig<TType> Proxy<TChildItem>(string header, Func<TType, IEnumerable<TChildItem>> selector = null)
        {
            var section = new NodeConfigSection<TType>()
            {
                ChildType = typeof(TChildItem),
                Name = header,
                IsProxy = true,
                AllowAdding = false
            };
            if (selector != null)
            {
                section.Selector = p => selector(p).Cast<IGraphItem>();
            }
            Sections.Add(section);
            return this;
        }

        //public NodeConfig<TType> AddChildItem<TChildItem>(string header = null, Func<TType, IEnumerable<TChildItem>> selector = null  ) where TChildItem : IDiagramNodeItem
        //{
        //    Container.RegisterNodeSection<TType, TChildItem>(header, selector);
        //    return this;
        //}
        public NodeConfig<TType> ReferenceSection<TSourceType, TReferenceItem>(string header, Func<TType, IEnumerable<TSourceType>> selector, bool manual = false)
            where TReferenceItem : GenericReferenceItem<TSourceType> where TSourceType : IGraphItem
        {
            Container.RegisterGraphItem<TReferenceItem, ScaffoldNodeChildItem<TReferenceItem>.ViewModel, ScaffoldNodeChildItem<TReferenceItem>.Drawer>();
            var section = new NodeConfigSection<TType>()
            {
                ChildType = typeof(TReferenceItem),
                Name = header,
                AllowAdding = manual,
                ReferenceType = typeof(TSourceType)
            };
            if (selector != null)
            {
                section.Selector = p => selector(p).Cast<IGraphItem>();
            }
            else
            {
                throw new Exception("Reference Section must have a selector");
            }
            Sections.Add(section);
            return this;
        }
        //public NodeConfig<TType> AddChildItem<TChildItem>(string header = null) where TChildItem : GenericNodeChildItem
        //{
        //    if (!string.IsNullOrEmpty(header))
        //        Container.RegisterNodeSection<TType, TChildItem>(header);

        //    Container.RegisterGraphItem<TChildItem, ScaffoldNodeChildItem<TChildItem>.ViewModel, ScaffoldNodeChildItem<TChildItem>.Drawer>();
        //    return this;
        //}
        public NodeConfig<TType> TypedSection<TChildItem>(string header = null, bool allowNone = false, bool primitiveOnly = false, bool includeUnityEngine = true) where TChildItem : ITypedItem
        {
           
                Section<TChildItem>(header);
                Container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = allowNone, PrimitiveOnly = primitiveOnly, IncludeUnityEngine = includeUnityEngine }, typeof(TChildItem).Name + "TypeSelection");
            

            
            return this;
        }
        //public NodeConfig<TType> AddChildItem<TChildItem, TChildItemViewModel, TChildItemDrawer>(string header = null) where TChildItem : GenericNodeChildItem
        //{
        //    if (!string.IsNullOrEmpty(header))
        //        Container.RegisterNodeSection<TType, TChildItem>(header);

        //    Container.RegisterGraphItem<TChildItem, TChildItemViewModel, TChildItemDrawer>();
        //    return this;
        //}
        public NodeConfig<TType> SubNodeOf<TNodeType>()
        {
            Container.RegisterFilterNode<TNodeType, TType>();
            return this;
        }
        public NodeConfig<TType> HasSubNode<TSubNodeType>()
        {
            Container.RegisterFilterNode<TType, TSubNodeType>();
            return this;
        }
        public NodeConfig<TType> AddGenerator(
            Func<ConfigCodeGeneratorSettings, CodeGenerator> createGeneratorFunc)
        {
            CodeGenerators.Add(createGeneratorFunc);
            return this;
        }
        public NodeConfig<TType> AddDesignerGenerator<TCodeGenerator>(string designerFile) where TCodeGenerator : NodeCodeGenerator<TType>, new()
        {
            AddGenerator(args =>
            new TCodeGenerator()
            {
                Data = args.Data,
                Filename = args.PathStrategy.GetDesignerFilePath("GameNodes"),
                IsDesignerFile = true
            });
            return this;
        }
        public NodeConfig<TType> AddEditableGenerator<TCodeGenerator>(string folder) where TCodeGenerator : NodeCodeGenerator<TType>, new()
        {
            AddGenerator(args =>
            new TCodeGenerator()
            {
                Data = args.Data,
                Filename = Path.Combine(folder, args.Data.Name + ".cs"),
                IsDesignerFile = false
            });
            return this;
        }

        public NodeConfig<TType> AddGeneratorWithDesigner<TCodeGenerator>(string name) where TCodeGenerator : NodeCodeGenerator<TType>, new()
        {
            AddDesignerGenerator<TCodeGenerator>(name);
            AddEditableGenerator<TCodeGenerator>(name);
            return this;
        }

        public NodeGeneratorConfig<TType> GetGeneratorConfig<TCodeGenerator>(string name = null)
            where TCodeGenerator : NodeCodeGenerator<TType>
        {

            if (TypeGeneratorConfigs.ContainsKey(typeof(TCodeGenerator)))
            {
                return TypeGeneratorConfigs[typeof(TCodeGenerator)] as NodeGeneratorConfig<TType>;
            }
            var config = new NodeGeneratorConfig<TType>();
            TypeGeneratorConfigs.Add(typeof(TCodeGenerator), config);
            return config;
        }

        public NodeConfig<TType> AddContextCommand(Action<TType> action, string name)
        {
            Container.RegisterInstance<IDiagramNodeCommand>(new SimpleEditorCommand<TType>(action, name), name);
            return this;
        }

        public Dictionary<Type, NodeGeneratorConfig> TypeGeneratorConfigs
        {
            get { return _typeGeneratorConfigs ?? (_typeGeneratorConfigs = new Dictionary<Type, NodeGeneratorConfig>()); }
            set { _typeGeneratorConfigs = value; }
        }

        public List<string> Tags
        {
            get { return _tags ?? (_tags = new List<string>()); }
            set { _tags = value; }
        }


        public NodeConfig<TType> Input<T>(string inputName)
        {
            var config = new NodeInputConfig()
            {
                Name = inputName,
                IsInput = true,
                IsOutput = false
            };
            Inputs.Add(config);
            return this;
        }
        public NodeConfig<TType> AddOutput<T>(string outputName)
        {
            var config = new NodeInputConfig()
            {
                Name = outputName,
                IsInput = false,
                IsOutput = true
            };
            Inputs.Add(config);
            return this;
        }
        public NodeConfig<TType> AddInputOutput<T>(string inputName, string outputName)
        {
            var config = new NodeInputConfig()
            {
                Name = inputName,
                OutputName = outputName,
                IsInput = true,
                IsOutput = true
            };
            Inputs.Add(config);
            return this;
        }
    }

    public class TagConfig
    {
        public string Name { get; set; }
        
    }
    public class NodeGeneratorConfig
    {
        private List<IMemberGenerator> _memberGenerators = new List<IMemberGenerator>();
        private List<NodeChildGeneratorConfig> _childItemMemberGenerators = new List<NodeChildGeneratorConfig>();

        public Type GeneratorType { get; set; }

        public List<IMemberGenerator> MemberGenerators
        {
            get { return _memberGenerators; }
        }

        public List<NodeChildGeneratorConfig> ChildItemMemberGenerators
        {
            get { return _childItemMemberGenerators; }
        }

        //public NodeGeneratorConfig<TType> AddMemberGenerator<TMemberGenerator>()
        //    where TMemberGenerator : IMemberGenerator<TType>
        //{
        //    MemberGenerators.Add(typeof(TMemberGenerator));
        //    return this;
        //}

    }

    public class NodeChildGeneratorConfig<TNode> : NodeChildGeneratorConfig
    {
        public Func<TNode, IEnumerable<IGraphItem>> Selector { get; set; }
    }


}

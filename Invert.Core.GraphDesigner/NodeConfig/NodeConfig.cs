using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.uFrame;

namespace Invert.Core.GraphDesigner
{
    public class NodeConfig<TNode> : NodeConfigBase where TNode : GenericNode, IConnectable
    {
        public NodeConfig<TNode> AddRefactoring(Func<TNode, bool> condition, Func<TNode, Refactorer> refactorer)
        {
            this.Refactorers.Add(node =>
            {
                if (condition(node as TNode))
                {
                    return refactorer(node as TNode);
                }
                return null;
            });
            return this;
        }

        public NodeConfig<TNode> Validator(Func<TNode,bool> validate, string message, ValidatorType validatorType = ValidatorType.Warning)
        {
            Validators.Add(new NodeValidator<TNode>()
            {
                Validate = validate,
                Message = message,
                Type = validatorType
                
            });
            return this;
        }
        public List<NodeValidator<TNode>> Validators
        {
            get { return _validators ?? (_validators =new List<NodeValidator<TNode>>()); }
            set { _validators = value; }
        }

        public IEnumerable<NodeValidator<TNode>> Validate(TNode node)
        {
            return Validators.Where(p => p.Validate != null && p.Validate(node));
        }

    
        private List<Func<ConfigCodeGeneratorSettings, CodeGenerator>> _codeGenerators;
        private Dictionary<Type, NodeGeneratorConfigBase> _typeGeneratorConfigs;

     

        public ConfigProperty<TNode, NodeColor> NodeColor
        {
            get { return _nodeColor; }
            set { _nodeColor = value; }
        }

        private ConfigProperty<TNode, NodeColor> _nodeColor;
        private List<NodeValidator<TNode>> _validators;

        public NodeConfig<TNode> ColorConfig(ConfigProperty<TNode, NodeColor> color)
        {
            NodeColor = color;
            return this;
        }
        public NodeConfig<TNode> Color(NodeColor literal)
        {
            NodeColor = new ConfigProperty<TNode, NodeColor>(literal);
            return this;
        }
        public NodeConfig<TNode> ColorConfig(NodeColor literal)
        {
            NodeColor = new ConfigProperty<TNode, NodeColor>(literal);
            return this;
        }

        public NodeConfig<TNode> ColorConfig(Func<TNode, NodeColor> selector)
        {
            NodeColor = new ConfigProperty<TNode, NodeColor>(selector);
            return this;
        }

        public List<Func<ConfigCodeGeneratorSettings, CodeGenerator>> CodeGenerators
        {
            get
            {
                return _codeGenerators ?? (_codeGenerators = new List<Func<ConfigCodeGeneratorSettings, CodeGenerator>>());
            }
            set { _codeGenerators = value; }
        }

        public Dictionary<Type, NodeGeneratorConfigBase> TypeGeneratorConfigs
        {
            get { return _typeGeneratorConfigs ?? (_typeGeneratorConfigs = new Dictionary<Type, NodeGeneratorConfigBase>()); }
            set { _typeGeneratorConfigs = value; }
        }



        public NodeConfig(IUFrameContainer container)
            : base(container)
        {
            container.Register<DesignerGeneratorFactory, CodeGeneratorFactory>("Config_" + typeof(TNode).Name + "_Generator");
        }

        public NodeConfig<TNode> AddContextCommand(Action<TNode> action, string name)
        {
            Container.RegisterInstance<IDiagramNodeCommand>(new SimpleEditorCommand<TNode>(action, name), name);
            return this;
        }

        public NodeConfig<TNode> AddDesignerGenerator<TCodeGenerator>(string designerFile, bool isEditorExtension = false) where TCodeGenerator : CodeGenerator, new()
        {
            AddGenerator(args =>
                new TCodeGenerator()
                {
                    ObjectData = args.Data,
                    Filename = args.PathStrategy.GetDesignerFilePath(designerFile),
                    IsDesignerFile = true
                });
            return this;
        }

        public NodeGeneratorConfig<TNode> AddDesignerOnlyClass<TCodeGenerator>(string name) where TCodeGenerator : CodeGenerator, new()
        {
            AddDesignerGenerator<TCodeGenerator>(name);
            return GetGeneratorConfig<TCodeGenerator>();
        }

        public NodeGeneratorConfig<TNode> AddEditableClass<TCodeGenerator>(string name) where TCodeGenerator : CodeGenerator, new()
        {
            UnityEngine.Debug.Log("Adding editable class " + typeof(TCodeGenerator).Name);
            AddDesignerGenerator<TCodeGenerator>(name);
            AddEditableGenerator<TCodeGenerator>(name);
            return GetGeneratorConfig<TCodeGenerator>();
        }

        public NodeConfig<TNode> AddEditableGenerator<TCodeGenerator>(string folder, bool isEditorExtension = false) where TCodeGenerator : CodeGenerator, new()
        {
            AddGenerator(args =>
                new TCodeGenerator()
                {
                    ObjectData = args.Data,
                    Filename = Path.Combine(folder, args.Data.Name + ".cs"),
                    IsDesignerFile = false
                });
            return this;
        }

        //public NodeConfig<TNode> AddEnum<TChildItem>(Func<TNode, IEnumerable<TChildItem>> selector,
        //    string designerFile = "Enums") where TChildItem : IDiagramNodeItem
        //{
        //    AddGenerator(args =>
        //       new GenericEnumCodeGenerator<TNode, TChildItem>()
        //       {
        //           Data = args.Data,
        //           Filename = args.PathStrategy.GetDesignerFilePath(designerFile),
        //           IsDesignerFile = true
        //       });
        //    return this;
        //}

        public NodeConfig<TNode> AddGenerator(
            Func<ConfigCodeGeneratorSettings, CodeGenerator> createGeneratorFunc)
        {
            CodeGenerators.Add(createGeneratorFunc);
            return this;
        }

        public NodeConfig<TNode> AddTag(string name)
        {
            Tags.Add(name);
            return this;
        }

        //public NodeConfig<TType> ConnectsFrom<TSource>(bool oneToMany, Color color) where TSource : class, IConnectable
        //{
        //    if (oneToMany)
        //        Container.RegisterInstance<IConnectionStrategy>(new OneToManyConnectionStrategy<TSource, TType>(color)
        //        , typeof(TSource).Name + "_" + typeof(TType).Name + "Connection");
        //    else
        //    {
        //        Container.RegisterInstance<IConnectionStrategy>(new OneToOneConnectionStrategy<TSource, TType>(color)
        //        , typeof(TSource).Name + "_" + typeof(TType).Name + "Connection");
        //    }
        //    return this;
        //}
       
        //public NodeConfig<TNode> ConnectsTo<TTarget>(bool oneToMany) where TTarget : class, IConnectable
        //{
        //    return ConnectsTo<TTarget>(oneToMany, UnityEngine.Color.white);
        //}

        //public NodeConfig<TNode> ConnectsTo<TTarget>(bool oneToMany, Color color) where TTarget : class, IConnectable
        //{
        //    if (oneToMany)
        //        Container.RegisterInstance<IConnectionStrategy>(new OneToManyConnectionStrategy<TNode, TTarget>(color), typeof(TNode).Name + "_" + typeof(TTarget).Name + "OneToManyConnection");
        //    else
        //    {
        //        Container.RegisterInstance<IConnectionStrategy>(new OneToOneConnectionStrategy<TNode, TTarget>(color), typeof(TNode).Name + "_" + typeof(TTarget).Name + "OneToOneConnection");
        //    }
        //    return this;
        //}

        public NodeGeneratorConfig<TNode> GetGeneratorConfig<TCodeGenerator>()
            where TCodeGenerator : CodeGenerator
        {
            if (TypeGeneratorConfigs.ContainsKey(typeof(TCodeGenerator)))
            {
                return TypeGeneratorConfigs[typeof(TCodeGenerator)] as NodeGeneratorConfig<TNode>;
            }
            var config = new NodeGeneratorConfig<TNode>();
            TypeGeneratorConfigs.Add(typeof(TCodeGenerator), config);
            return config;
        }

        public NodeConfig<TNode> HasSubNode<TSubNodeType>()
        {
            Container.RegisterFilterNode<TNode, TSubNodeType>();
            return this;
        }
        public NodeConfig<TNode> HasSubNode(Type subNodeType)
        {
            Container.RegisterFilterNode(typeof(TNode),subNodeType);
            return this;
        }

        public NodeConfig<TNode> Input<TSourceType, TReferenceType>(Func<IDiagramNodeItem, string> name, bool allowMultiple, Func<IDiagramNodeItem, IDiagramNodeItem, bool> validator = null)
            where TReferenceType : GenericSlot, new()
            where TSourceType : class, IConnectable
        {
            var config = new NodeInputConfig()
            {
                Name = new ConfigProperty<IDiagramNodeItem, string>(name),
                IsInput = true,
                IsOutput = false,
                ReferenceType = typeof(TReferenceType),
                SourceType = typeof(TSourceType),
                AllowMultiple = allowMultiple,
                Validator = validator
            };
            GraphItemConfigurations.Add(config);
            return this;
        }

        public NodeConfig<TNode> Input<TSourceType, TReferenceType>(string inputName, bool allowMultiple, Func<IDiagramNodeItem, IDiagramNodeItem, bool> validator = null)
            where TReferenceType : GenericSlot, new()
            where TSourceType : class, IConnectable
        {
            var config = new NodeInputConfig()
            {
                Name = new ConfigProperty<IDiagramNodeItem, string>(inputName),
                IsInput = true,
                IsOutput = false,
                ReferenceType = typeof(TReferenceType),
                SourceType = typeof(TSourceType),
                AllowMultiple = allowMultiple,
                Validator = validator
            };
            GraphItemConfigurations.Add(config);
            return this;
            
        }

        public NodeConfig<TNode> InputAlias(string inputName)
        {
            var config = new NodeInputConfig()
            {
                Name = new ConfigProperty<IDiagramNodeItem, string>(inputName),
                IsInput = true,
                IsOutput = false,
                SourceType = typeof(TNode),
                ReferenceType = typeof(TNode),
                IsAlias = true
            };
            GraphItemConfigurations.Add(config);
            return this;
        }


        public NodeConfig<TNode> Output<TSourceType, TReferenceType>(string inputName, bool allowMultiple, Func<IDiagramNodeItem, IDiagramNodeItem, bool> validator = null)
            where TReferenceType : GenericSlot, new()
            where TSourceType : class, IConnectable
        {
            var config = new NodeInputConfig()
            {
                Name = new ConfigProperty<IDiagramNodeItem, string>(inputName),
                IsInput = false,
                IsOutput = true,
                ReferenceType = typeof(TReferenceType),
                SourceType = typeof(TSourceType),
                AllowMultiple = allowMultiple,
                Validator = validator
            };
            GraphItemConfigurations.Add(config);
           
            return this;
        }

        public NodeConfig<TNode> Output<TSourceType, TReferenceType>(Func<IDiagramNodeItem,string> inputName, bool allowMultiple, Func<IDiagramNodeItem, IDiagramNodeItem, bool> validator = null)
            where TReferenceType : GenericSlot, new()
            where TSourceType : class, IConnectable
        {
            var config = new NodeInputConfig()
            {
                Name = new ConfigProperty<IDiagramNodeItem, string>(inputName),
                IsInput = false,
                IsOutput = true,
                ReferenceType = typeof(TReferenceType),
                SourceType = typeof(TSourceType),
                AllowMultiple = allowMultiple,
                Validator = validator
            };
            GraphItemConfigurations.Add(config);

            return this;
        }

        public NodeConfig<TNode> OutputAlias(string outputName)
        {
            var config = new NodeInputConfig()
            {
                Name = new ConfigProperty<IDiagramNodeItem, string>(outputName),
                IsInput = false,
                IsOutput = true,
                SourceType = typeof(TNode),
                ReferenceType = typeof(TNode),
                IsAlias = true
            };
            GraphItemConfigurations.Add(config);

            return this;
        }
        public NodeConfig<TNode> OutputAlias<TType>(string outputName)
        {
            var config = new NodeInputConfig()
            {
                Name = new ConfigProperty<IDiagramNodeItem, string>(outputName),
                IsInput = false,
                IsOutput = true,
                SourceType = typeof(TType),
                ReferenceType = typeof(TType),
                IsAlias = true
            };
            GraphItemConfigurations.Add(config);

            return this;
        }
        public NodeConfigSection<TNode> Proxy<TChildItem>(string header, Func<TNode, IEnumerable<TChildItem>> selector = null)
        {
            var section = new NodeConfigSection<TNode>()
            {
                SourceType = typeof(TChildItem),
                Name = header,
                IsProxy = true,
                AllowAdding = false
            };
            if (selector != null)
            {
                section.Selector = p => selector(p).Cast<IGraphItem>();
            }
            GraphItemConfigurations.Add(section);
            return section;
        }

        public NodeConfigSection<TNode> ReferenceSection<TSourceType, TReferenceItem>(string header, 
            Func<TNode, IEnumerable<TSourceType>> selector,
            bool manual = false,
            bool allowDuplicates = false)
            where TReferenceItem : GenericReferenceItem
            //where TSourceType : IGraphItem
        {
            //Container.RegisterGraphItem<TReferenceItem, ScaffoldNodeChildItem<TReferenceItem>.ViewModel, ScaffoldNodeChildItem<TReferenceItem>.Drawer>();
            var section = new NodeConfigSection<TNode>()
            {
                SourceType = typeof(TReferenceItem),
                Name = header,
                AllowAdding = manual,
                ReferenceType = typeof(TSourceType),
                AllowDuplicates = allowDuplicates
            };
            if (selector != null)
            {
                section.Selector = p => selector(p).Cast<IGraphItem>();
            }
            else
            {
                throw new Exception("Reference Section must have a selector");
            }
            GraphItemConfigurations.Add(section);
            return section;
        }

        public NodeConfigSection<TNode> Section<TChildItem>(string header, Func<TNode, IEnumerable<TChildItem>> selector = null, bool allowAdding = true, Action<TChildItem> onAdd = null)
        {
            var section = new NodeConfigSection<TNode>()
            {
                SourceType = typeof(TChildItem),
                Name = header,
         
                IsProxy = false,
                AllowAdding = allowAdding
            };
            if (onAdd != null)
            {
                section.OnAdd = p => onAdd((TChildItem) p);
            }
            if (selector != null)
            {
                section.Selector = p => selector(p).Cast<IGraphItem>();
            }
            GraphItemConfigurations.Add(section);
            return section;
        }

        public NodeConfig<TNode> SubNodeOf<TNodeType>()
        {
            Container.RegisterFilterNode<TNodeType, TNode>();
            return this;
        }

        public NodeConfigSection<TNode> TypedSection<TChildItem>(string header, SelectItemTypeCommand selectTypeCommand) where TChildItem : ITypedItem
        {
          
            Container.RegisterInstance<IEditorCommand>(selectTypeCommand, typeof(TChildItem).Name + "TypeSelection");
            return Section<TChildItem>(header);
        }

        public class CodeGeneratorFactory : DesignerGeneratorFactory<TNode>
        {
            public override IEnumerable<CodeGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
                TNode item)
            {
                if (!item.IsValid) yield break;

                var config = Container.GetNodeConfig<TNode>();
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
                        if (config.TypeGeneratorConfigs != null && config.TypeGeneratorConfigs.ContainsKey(result.GetType()))
                        {
                            var generatorConfig =
                                config.TypeGeneratorConfigs[result.GetType()] as NodeGeneratorConfig<TNode>;

                            if (generatorConfig != null)
                            {
                                if (generatorConfig.Condition != null && !generatorConfig.Condition(item)) continue;
                                if (result.IsDesignerFile)
                                {
                                    if (generatorConfig.DesignerFilename != null)
                                        result.Filename = generatorConfig.DesignerFilename.GetValue(item);
                                }
                                else
                                {
                                    if (generatorConfig.Filename != null)
                                        result.Filename = generatorConfig.Filename.GetValue(item);
                                }
                            }
                        }

                        yield return result;
                    }
                }
            }
        }

        public class ConfigCodeGeneratorSettings
        {
            public TNode Data { get; set; }

            public ICodePathStrategy PathStrategy { get; set; }

            public INodeRepository Repository { get; set; }

            public GeneratorSettings Settings { get; set; }
        }

        public NodeConfig<TNode> AddFlag(string inheritable)
        {
            Container.RegisterInstance<IDiagramNodeCommand>(new NodeFlagCommand<TNode>(inheritable,inheritable),typeof(TNode).Name + inheritable + "FlagCommand");
            return this;
        }
        public NodeConfig<TNode> AddFlag(string inheritable, Func<TNode,bool> get, Action<TNode,bool> set )
        {
            Container.RegisterInstance<IDiagramNodeCommand>(new NodeFlagCommand<TNode>(inheritable, inheritable)
            {
                IsProperty = true,
                Get = get,
                Set = set
            }, inheritable + "Command");
            return this;
        }

        public override bool IsValid(GenericNode node)
        {
            return !Validate(node as TNode).Any();
        }
    }
}
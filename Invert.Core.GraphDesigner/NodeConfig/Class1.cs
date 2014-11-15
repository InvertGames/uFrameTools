using Invert.Common;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class NodeChildGeneratorConfig<TNode> : NodeChildGeneratorConfig
    {
        public Func<TNode, IEnumerable<IGraphItem>> Selector { get; set; }
    }

    public class NodeConfig<TNode> : NodeConfig where TNode : GenericNode, IConnectable
    {
        private List<Func<ConfigCodeGeneratorSettings, CodeGenerator>> _codeGenerators;
        private List<string> _tags;
        private Dictionary<Type, NodeGeneratorConfig> _typeGeneratorConfigs;

         public GUIStyle GetNodeColorStyle(TNode node)
        {
             if (NodeColor == null)
             {
                 return ElementDesignerStyles.NodeHeader1;
             }
                switch (NodeColor.GetValue(node))
                {
                    case Core.GraphDesigner.NodeColor.DarkGray:
                        return ElementDesignerStyles.NodeHeader1;
                    case Core.GraphDesigner.NodeColor.Blue:
                        return ElementDesignerStyles.NodeHeader2;
                    case Core.GraphDesigner.NodeColor.Gray:
                        return ElementDesignerStyles.NodeHeader3;
                    case Core.GraphDesigner.NodeColor.LightGray:
                        return ElementDesignerStyles.NodeHeader4;
                    case Core.GraphDesigner.NodeColor.Black:
                        return ElementDesignerStyles.NodeHeader5;
                    case Core.GraphDesigner.NodeColor.DarkDarkGray:
                        return ElementDesignerStyles.NodeHeader6;
                    case Core.GraphDesigner.NodeColor.Orange:
                        return ElementDesignerStyles.NodeHeader7;
                    case Core.GraphDesigner.NodeColor.Red:
                        return ElementDesignerStyles.NodeHeader8;
                    case Core.GraphDesigner.NodeColor.YellowGreen:
                        return ElementDesignerStyles.NodeHeader9;
                    case Core.GraphDesigner.NodeColor.Green:
                        return ElementDesignerStyles.NodeHeader10;
                    case Core.GraphDesigner.NodeColor.Purple:
                        return ElementDesignerStyles.NodeHeader11;
                    case Core.GraphDesigner.NodeColor.Pink:
                        return ElementDesignerStyles.NodeHeader12;
                    case Core.GraphDesigner.NodeColor.Yellow:
                        return ElementDesignerStyles.NodeHeader13;

                }
                return ElementDesignerStyles.NodeHeader1;
            
        }

        public ConfigProperty<TNode, NodeColor> NodeColor
        {
            get { return _nodeColor; }
            set { _nodeColor = value; }
        }

        private ConfigProperty<TNode, NodeColor> _nodeColor;

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

        public List<string> Tags
        {
            get { return _tags ?? (_tags = new List<string>()); }
            set { _tags = value; }
        }

        public Dictionary<Type, NodeGeneratorConfig> TypeGeneratorConfigs
        {
            get { return _typeGeneratorConfigs ?? (_typeGeneratorConfigs = new Dictionary<Type, NodeGeneratorConfig>()); }
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

        public NodeConfig<TNode> AddDesignerGenerator<TCodeGenerator>(string designerFile, bool isEditorExtension = false) where TCodeGenerator : NodeCodeGenerator<TNode>, new()
        {
            AddGenerator(args =>
            new TCodeGenerator()
            {
                Data = args.Data,
                Filename = args.PathStrategy.GetDesignerFilePath(designerFile),
                IsDesignerFile = true
            });
            return this;
        }

        public NodeGeneratorConfig<TNode> AddDesignerOnlyClass<TCodeGenerator>(string name) where TCodeGenerator : NodeCodeGenerator<TNode>, new()
        {
            AddDesignerGenerator<TCodeGenerator>(name);
            return GetGeneratorConfig<TCodeGenerator>();
        }

        public NodeGeneratorConfig<TNode> AddEditableClass<TCodeGenerator>(string name) where TCodeGenerator : NodeCodeGenerator<TNode>, new()
        {
            AddDesignerGenerator<TCodeGenerator>(name);
            AddEditableGenerator<TCodeGenerator>(name);
            return GetGeneratorConfig<TCodeGenerator>();
        }

        public NodeConfig<TNode> AddEditableGenerator<TCodeGenerator>(string folder, bool isEditorExtension = false) where TCodeGenerator : NodeCodeGenerator<TNode>, new()
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

        public NodeConfig<TNode> AddEnum<TChildItem>(Func<TNode, IEnumerable<TChildItem>> selector,
            string designerFile = "Enums") where TChildItem : IDiagramNodeItem
        {
            AddGenerator(args =>
               new GenericEnumCodeGenerator<TNode, TChildItem>()
               {
                   Data = args.Data,
                   Filename = args.PathStrategy.GetDesignerFilePath(designerFile),
                   IsDesignerFile = true
               });
            return this;
        }

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
       
        public NodeConfig<TNode> ConnectsTo<TTarget>(bool oneToMany) where TTarget : class, IConnectable
        {
            return ConnectsTo<TTarget>(oneToMany, UnityEngine.Color.white);
        }

        public NodeConfig<TNode> ConnectsTo<TTarget>(bool oneToMany, Color color) where TTarget : class, IConnectable
        {
            //if (oneToMany)
            //    Container.RegisterInstance<IConnectionStrategy>(new OneToManyConnectionStrategy<TNode, TTarget>(color), typeof(TNode).Name + "_" + typeof(TTarget).Name + "OneToManyConnection");
            //else
            //{
            //    Container.RegisterInstance<IConnectionStrategy>(new OneToOneConnectionStrategy<TNode, TTarget>(color), typeof(TNode).Name + "_" + typeof(TTarget).Name + "OneToOneConnection");
            //}
            return this;
        }

        public NodeGeneratorConfig<TNode> GetGeneratorConfig<TCodeGenerator>()
            where TCodeGenerator : NodeCodeGenerator<TNode>
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

        public NodeConfig<TNode> Input<TSourceType, TReferenceType>(string inputName, bool allowMultiple, Func<IDiagramNodeItem, IDiagramNodeItem, bool> validator = null)
            where TReferenceType : GenericConnectionReference, new()
            where TSourceType : class, IConnectable
        {
            var config = new NodeInputConfig()
            {
                Name = inputName,
                IsInput = true,
                IsOutput = false,
                ReferenceType = typeof(TReferenceType),
                SourceType = typeof(TSourceType),
                AllowMultiple = allowMultiple,
                Validator = validator
            };
            Inputs.Add(config);

            //var cs = new GenericConnectionStrategy<TSourceType, TReferenceType>()
            //{
            //    IsConnected = (output, input) =>
            //    {
            //        var result = input.ConnectedGraphItemIds.Contains(input.Identifier);
            //        //var genericNode = input.Node as GenericNode;
            //        //var inputItem = genericNode.GetConnectionReference<TReferenceType>();

            //        //var result = inputItem.ConnectedGraphItemIds.Contains(output.Identifier);
            //       // Debug.Log(string.Format("Testing Connection::{0}:{1}:{2}", inputItem.GetType().Name, output.Label,result));
            //        //return result;
            //        return result;
            //    },
            //    Apply = (output, input) =>
            //    {

            //        if (!input.ConnectedGraphItemIds.Contains(input.Identifier))
            //        {
            //            input.ConnectedGraphItemIds.Add(input.Identifier);
            //        }



            //        //var genericNode = input.Node as GenericNode;
            //        //var inputItem = genericNode.GetConnectionReference<TReferenceType>();
            //        //if (!allowMultiple && inputItem.ConnectedGraphItemIds.Count > 0)
            //        //{
            //        //    inputItem.ConnectedGraphItemIds.Clear();
            //        //}
            //        //if (!inputItem.ConnectedGraphItemIds.Contains(output.Identifier))
            //        //{
            //        //    inputItem.ConnectedGraphItemIds.Add((output.Identifier));
            //        //}

            //        //Debug.Log(string.Format("{0}:{1}", output.GetType().Name, input.GetType().Name));
            //    },
            //    Remove = (output, input) =>
            //    {
            //        input.ConnectedGraphItemIds.Remove(output.Identifier);
            //        //var genericNode = input.Node as GenericNode;
            //        //genericNode.GetConnectionReference<TReferenceType>().ConnectedGraphItemIds.Remove((output.Identifier));
            //    },
            //};
            //Container.RegisterInstance<IConnectionStrategy>(cs, inputName + typeof(IConnectable).Name + "to" + typeof(IConnectable).Name + "Connection");
            //Container.RegisterInstance<IConnectionStrategy>(new InputReferenceConnectionStrategy<TSourceType, TReferenceType>(UnityEngine.Color.white),
            //    typeof(TSourceType).Name + "_" + typeof(TReferenceType).Name + "InputConnection");
            //Container.Connectable<TSourceType, TReferenceType>(false);
            return this;
        }

        public NodeConfig<TNode> InputAlias(string inputName)
        {
            var config = new NodeInputConfig()
            {
                Name = inputName,
                IsInput = true,
                IsOutput = false,
                SourceType = typeof(TNode),
                ReferenceType = typeof(TNode),
                IsAlias = true
            };
            Inputs.Add(config);
            return this;
        }


        public NodeConfig<TNode> Output<TSourceType, TReferenceType>(string inputName, bool allowMultiple, Func<IDiagramNodeItem, IDiagramNodeItem, bool> validator = null)
            where TReferenceType : GenericConnectionReference, new()
            where TSourceType : class, IConnectable
        {
            var config = new NodeInputConfig()
            {
                Name = inputName,
                IsInput = false,
                IsOutput = true,
                ReferenceType = typeof(TReferenceType),
                SourceType = typeof(TSourceType),
                AllowMultiple = allowMultiple,
                Validator = validator
            };
            Inputs.Add(config);
            //var allowMultiple1 = allowMultiple;
            //var cs = new GenericConnectionStrategy<TReferenceType, TSourceType>()
            //{
            //    IsConnected = (output, input) =>
            //    {
            //        //var genericNode = output.Node as GenericNode;
            //        //var inputItem = genericNode.GetConnectionReference<TReferenceType>();

            //        var result = output.ConnectedGraphItemIds.Contains(input.Identifier);
            //        //Debug.Log(string.Format("Testing Connection::{0}:{1}:{2}", inputItem.GetType().Name, output.Label,result));
            //        //return result;
            //        return result;
            //        return false;
            //    },
            //    Apply = (output, input) =>
            //    {
            //        //var referenceType = output.ConnectedGraphItems.OfType<TReferenceType>();
                  
            //        //var genericNode = output.Node as GenericNode;
            //        //var inputItem = genericNode.GetConnectionReference<TReferenceType>();
             
            //        if (!output.ConnectedGraphItemIds.Contains(input.Identifier))
            //        {
            //              output.ConnectedGraphItemIds.Add(input.Identifier);
            //        }

            //        Debug.Log(string.Format("Applied -> {0}:{1}", output.GetType().Name, input.GetType().Name));
            //    },
            //    Remove = (output, input) =>
            //    {
            //        //var genericNode = output.Node as GenericNode;
            //        //genericNode.GetConnectionReference<TReferenceType>().ConnectedGraphItemIds.Remove((input.Identifier));
            //        output.ConnectedGraphItemIds.Remove(input.Identifier);
            //    },
            //    AllowMultipleInputs = false,
            //    AllowMultipleOutputs = allowMultiple
            //};
            //Container.RegisterInstance<IConnectionStrategy>(cs, inputName + typeof(IConnectable).Name + "to" + typeof(IConnectable).Name + "Connection");
            //Container.RegisterInstance<IConnectionStrategy>(new InputReferenceConnectionStrategy<TSourceType, TReferenceType>(UnityEngine.Color.white),
            //    typeof(TSourceType).Name + "_" + typeof(TReferenceType).Name + "InputConnection");
            //Container.Connectable<TSourceType, TReferenceType>(false);
            return this;
        }

        public NodeConfig<TNode> OutputAlias(string outputName)
        {
            var config = new NodeInputConfig()
            {
                Name = outputName,
                IsInput = false,
                IsOutput = true,
                SourceType = typeof(TNode),
                ReferenceType = typeof(TNode),
                IsAlias = true
            };
            Inputs.Add(config);

            return this;
        }
        public NodeConfig<TNode> OutputAlias<TType>(string outputName)
        {
            var config = new NodeInputConfig()
            {
                Name = outputName,
                IsInput = false,
                IsOutput = true,
                SourceType = typeof(TType),
                ReferenceType = typeof(TType),
                IsAlias = true
            };
            Inputs.Add(config);

            return this;
        }
        public NodeConfig<TNode> Proxy<TChildItem>(string header, Func<TNode, IEnumerable<TChildItem>> selector = null)
        {
            var section = new NodeConfigSection<TNode>()
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
        public NodeConfig<TNode> ReferenceSection<TSourceType, TReferenceItem>(string header, Func<TNode, IEnumerable<TSourceType>> selector, bool manual = false)
            where TReferenceItem : GenericReferenceItem<TSourceType>
            where TSourceType : IGraphItem
        {
            Container.RegisterGraphItem<TReferenceItem, ScaffoldNodeChildItem<TReferenceItem>.ViewModel, ScaffoldNodeChildItem<TReferenceItem>.Drawer>();
            var section = new NodeConfigSection<TNode>()
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

        public NodeConfig<TNode> Section<TChildItem>(string header, Func<TNode, IEnumerable<TChildItem>> selector = null, bool allowAdding = true)
        {
            var section = new NodeConfigSection<TNode>()
            {
                ChildType = typeof(TChildItem),
                Name = header,
                IsProxy = false,
                AllowAdding = allowAdding
            };
            if (selector != null)
            {
                section.Selector = p => selector(p).Cast<IGraphItem>();
            }
            Sections.Add(section);
            return this;
        }

        //    Container.RegisterGraphItem<TChildItem, TChildItemViewModel, TChildItemDrawer>();
        //    return this;
        //}
        public NodeConfig<TNode> SubNodeOf<TNodeType>()
        {
            Container.RegisterFilterNode<TNodeType, TNode>();
            return this;
        }

        //    Container.RegisterGraphItem<TChildItem, ScaffoldNodeChildItem<TChildItem>.ViewModel, ScaffoldNodeChildItem<TChildItem>.Drawer>();
        //    return this;
        //}
        public NodeConfig<TNode> TypedSection<TChildItem>(string header = null, bool allowNone = false, bool primitiveOnly = false, bool includeUnityEngine = true) where TChildItem : ITypedItem
        {
            Section<TChildItem>(header);
            Container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = allowNone, PrimitiveOnly = primitiveOnly, IncludeUnityEngine = includeUnityEngine }, typeof(TChildItem).Name + "TypeSelection");

            return this;
        }

        public class CodeGeneratorFactory : DesignerGeneratorFactory<TNode>
        {
            public override IEnumerable<CodeGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
                TNode item)
            {
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

        //public NodeConfig<TType> AddChildItem<TChildItem>(string header = null) where TChildItem : GenericNodeChildItem
        //{
        //    if (!string.IsNullOrEmpty(header))
        //        Container.RegisterNodeSection<TType, TChildItem>(header);
        //public NodeConfig<TType> AddChildItem<TChildItem, TChildItemViewModel, TChildItemDrawer>(string header = null) where TChildItem : GenericNodeChildItem
        //{
        //    if (!string.IsNullOrEmpty(header))
        //        Container.RegisterNodeSection<TType, TChildItem>(header);
        //public NodeConfig<TType> AddInputOutput<T>(string inputName, string outputName)
        //{
        //    var config = new NodeInputConfig()
        //    {
        //        Name = inputName,
        //        OutputName = outputName,
        //        IsInput = true,
        //        IsOutput = true,
        //    };
        //    Inputs.Add(config);
        //    return this;
        //}
        public NodeConfig<TNode> AddFlag(string inheritable)
        {
            Container.RegisterInstance<IDiagramNodeCommand>(new NodeFlagCommand<TNode>(inheritable,inheritable),inheritable + "Command");
            return this;
        }
    }

    public class NodeGeneratorConfig
    {
        private List<NodeChildGeneratorConfig> _childItemMemberGenerators = new List<NodeChildGeneratorConfig>();
        private List<IMemberGenerator> _memberGenerators = new List<IMemberGenerator>();

        public List<NodeChildGeneratorConfig> ChildItemMemberGenerators
        {
            get { return _childItemMemberGenerators; }
        }

        public Type GeneratorType { get; set; }

        public List<IMemberGenerator> MemberGenerators
        {
            get { return _memberGenerators; }
        }

        //public NodeGeneratorConfig<TType> AddMemberGenerator<TMemberGenerator>()
        //    where TMemberGenerator : IMemberGenerator<TType>
        //{
        //    MemberGenerators.Add(typeof(TMemberGenerator));
        //    return this;
        //}
    }

    public class TagConfig
    {
        public string Name { get; set; }
    }
}
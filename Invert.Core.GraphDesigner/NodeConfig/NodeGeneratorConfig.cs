using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Invert.uFrame.Editor
{
   
    public class NodeGeneratorConfig<TNode> : NodeGeneratorConfig
        where TNode : GenericNode
    {


        public ConfigProperty<TNode, IEnumerable<string>> Namespaces
        {
            get { return _namespaces; }
            set { _namespaces = value; }
        }

        private ConfigProperty<TNode, IEnumerable<string>> _namespaces;

        public NodeGeneratorConfig<TNode> NamespacesConfig(ConfigProperty<TNode, IEnumerable<string>> namespaces)
        {
            Namespaces = namespaces;
            return this;
        }

        public NodeGeneratorConfig<TNode> NamespacesConfig(IEnumerable<string> literal)
        {
            Namespaces = new ConfigProperty<TNode, IEnumerable<string>>(literal);
            return this;
        }

        public NodeGeneratorConfig<TNode> NamespacesConfig(Func<TNode, IEnumerable<string>> selector)
        {
            Namespaces = new ConfigProperty<TNode, IEnumerable<string>>(selector);
            return this;
        }
        public ConfigProperty<TNode, CodeTypeDeclaration> DesignerDeclaration
        {
            get { return _designerDeclaration; }
            set { _designerDeclaration = value; }
        }

        private ConfigProperty<TNode, CodeTypeDeclaration> _designerDeclaration;

        public NodeGeneratorConfig<TNode> DesignerDeclarationConfig(ConfigProperty<TNode, CodeTypeDeclaration> designerDeclaration)
        {
            DesignerDeclaration = designerDeclaration;
            return this;
        }

        public NodeGeneratorConfig<TNode> DesignerDeclarationConfig(CodeTypeDeclaration literal)
        {
            DesignerDeclaration = new ConfigProperty<TNode, CodeTypeDeclaration>(literal);
            return this;
        }

        public NodeGeneratorConfig<TNode> DesignerDeclarationConfig(Func<TNode, CodeTypeDeclaration> selector)
        {
            DesignerDeclaration = new ConfigProperty<TNode, CodeTypeDeclaration>(selector);
            return this;
        }
        public ConfigProperty<TNode, CodeTypeDeclaration> Declaration
        {
            get { return _declaration; }
            set { _declaration = value; }
        }

        private ConfigProperty<TNode, CodeTypeDeclaration> _declaration;

        public NodeGeneratorConfig<TNode> DeclarationConfig(ConfigProperty<TNode, CodeTypeDeclaration> declaration)
        {
            Declaration = declaration;
            return this;
        }

        public NodeGeneratorConfig<TNode> DeclarationConfig(CodeTypeDeclaration literal)
        {
            Declaration = new ConfigProperty<TNode, CodeTypeDeclaration>(literal);
            return this;
        }

        public NodeGeneratorConfig<TNode> DeclarationConfig(Func<TNode, CodeTypeDeclaration> selector)
        {
            Declaration = new ConfigProperty<TNode, CodeTypeDeclaration>(selector);
            return this;
        }

        public ConfigProperty<TNode, string> DesignerFilename
        {
            get { return _designerFilename; }
            set { _designerFilename = value; }
        }

        private ConfigProperty<TNode, string> _designerFilename;

        public NodeGeneratorConfig<TNode> DesignerFilenameConfig(ConfigProperty<TNode, string> designerFilename)
        {
            DesignerFilename = designerFilename;
            return this;
        }

        public NodeGeneratorConfig<TNode> DesignerFilenameConfig(string literal)
        {
            DesignerFilename = new ConfigProperty<TNode, string>(literal);
            return this;
        }

        public NodeGeneratorConfig<TNode> DesignerFilenameConfig(Func<TNode, string> selector)
        {
            DesignerFilename = new ConfigProperty<TNode, string>(selector);
            return this;
        }
        public ConfigProperty<TNode, string> Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        private ConfigProperty<TNode, string> _filename;

        public NodeGeneratorConfig<TNode> FilenameConfig(ConfigProperty<TNode, string> filename)
        {
            Filename = filename;
            return this;
        }

        public NodeGeneratorConfig<TNode> FilenameConfig(string literal)
        {
            Filename = new ConfigProperty<TNode, string>(literal);
            return this;
        }

        public NodeGeneratorConfig<TNode> FilenameConfig(Func<TNode, string> selector)
        {
            Filename = new ConfigProperty<TNode, string>(selector);
            return this;
        }



        private ConfigProperty<TNode, CodeTypeReference> _baseType;

        public ConfigProperty<TNode, CodeTypeReference> BaseType
        {
            get { return _baseType; }
            set { _baseType = value; }
        }

        public NodeGeneratorConfig<TNode> BaseTypeConfig(ConfigProperty<TNode, CodeTypeReference> baseType)
        {
            BaseType = baseType;
            return this;
        }
        public NodeGeneratorConfig<TNode> BaseTypeConfig(CodeTypeReference literal)
        {
            BaseType = new ConfigProperty<TNode, CodeTypeReference>(literal);
            return this;
        }
        public NodeGeneratorConfig<TNode> BaseTypeConfig(Func<TNode, CodeTypeReference> selector)
        {
            BaseType = new ConfigProperty<TNode, CodeTypeReference>(selector);
            return this;
        }

        public NodeGeneratorConfig<TNode> AsDerivedEditableClass(
            string classNameFormat, 
            string designerFilename,
            Func<TNode,CodeTypeReference> getRootBaseType)
        {
            this.InheritanceBaseTypeConfig(getRootBaseType)
                .ClassNameConfig(node => string.Format(classNameFormat, node.Name))
                .FilenameConfig(node =>  string.Format(classNameFormat + ".cs", node.Name));
                //.DesignerFilenameConfig(node => Path.Combine("_DesignerFiles", Path.Combine("Editor", designerFilename + ".cs")));
            return this;
        }
        public NodeGeneratorConfig<TNode> AsDerivedEditorEditableClass(
            string nameFormat,
            string designerFilename,
            Func<TNode, CodeTypeReference> getRootBaseType)
        {
            this.InheritanceBaseTypeConfig(getRootBaseType)
                .ClassNameConfig(node => string.Format(nameFormat, node.Name))
                .FilenameConfig(node => Path.Combine("Editor", string.Format(nameFormat + ".cs", node.Name)))
                .DesignerFilenameConfig(node => Path.Combine("_DesignerFiles", Path.Combine("Editor", designerFilename + ".cs")));

            return this;
        }

        public NodeGeneratorConfig<TNode> InheritanceBaseTypeConfig(Func<TNode,CodeTypeReference> getRootBaseType)
            
        {
            BaseTypeConfig(
                delegate(TNode node)
                {
                    var inheritanceNode = node as GenericInheritableNode;

                    if (inheritanceNode != null && inheritanceNode.BaseNode != null)
                    {
                        return new CodeTypeReference(string.Format("{0}Node", inheritanceNode.BaseNode.Name));
                    }
                    return getRootBaseType(node);
                });
            return this;
        }
        private ConfigProperty<TNode, string> _className = new ConfigProperty<TNode,string>(_=>_.Name);

        public ConfigProperty<TNode, string> ClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        public NodeGeneratorConfig<TNode> ClassNameConfig(ConfigProperty<TNode, string> className)
        {
            ClassName = className;
            return this;
        }
        public NodeGeneratorConfig<TNode> ClassNameConfig(string literal)
        {
            ClassName = new ConfigProperty<TNode, string>(literal);
            return this;
        }
        public NodeGeneratorConfig<TNode> ClassNameConfig(Func<TNode, string> selector)
        {
            ClassName = new ConfigProperty<TNode, string>(selector);
            return this;
        }


        public NodeGeneratorConfig<TNode> Member(Func<LambdaMemberGenerator<TNode>, CodeTypeMember> generate, MemberGeneratorLocation location = MemberGeneratorLocation.DesignerFile)
        {
            MemberGenerators.Add(new LambdaMemberGenerator<TNode>(generate)
            {
                Location = location
            });
            return this;
        }

        public NodeGeneratorConfig<TNode> Member(IMemberGenerator generator)
        {
            MemberGenerators.Add(generator);
            return this;
        }

        public NodeGeneratorConfig<TNode> MethodsFor<TChildItem>()
        {
            return this;
        }
     
        public NodeGeneratorConfig<TNode> MembersFor<TChildItem>(Func<TNode, IEnumerable<TChildItem>> selector, IMemberGenerator generater)
            where TChildItem : IGraphItem
        {
            ChildItemMemberGenerators.Add(new NodeChildGeneratorConfig<TNode>()
            {
                ChildType = typeof(TChildItem),
                Selector = p => selector(p).Cast<IGraphItem>(),
                Generator = generater
            });
            return this;
        }

        public NodeGeneratorConfig<TNode> MembersFor<TChildItem>(Func<TNode, IEnumerable<TChildItem>> selector, Func<LambdaMemberGenerator<TChildItem>, CodeTypeMember> generate)
            where TChildItem : IGraphItem
        {
            ChildItemMemberGenerators.Add(new NodeChildGeneratorConfig<TNode>()
            {
                ChildType = typeof(TChildItem),
                Selector = p => selector(p).Cast<IGraphItem>(),
                Generator = new LambdaMemberGenerator<TChildItem>(generate)
            });
            return this;
        }

        public NodeGeneratorConfig<TNode> MembersPerChild<TChildItem>(IMemberGenerator generator)
            where TChildItem : GenericNodeChildItem
        {
            ChildItemMemberGenerators.Add(new NodeChildGeneratorConfig<TNode>()
            {
                ChildType = typeof(TChildItem),
                Generator = generator
            });
            return this;
        }

        public NodeGeneratorConfig<TNode> MembersPerChild<TChildItem>(Func<LambdaMemberGenerator<TChildItem>, CodeTypeMember> generate)
            where TChildItem : GenericNodeChildItem
        {
            ChildItemMemberGenerators.Add(new NodeChildGeneratorConfig<TNode>()
            {
                ChildType = typeof(TChildItem),
                Generator = new LambdaMemberGenerator<TChildItem>(generate)
            });
            return this;
        }

        public IEnumerable<IMemberGenerator> GetMemberGenerators(CodeTypeDeclaration decleration, GenericNode data, MemberGeneratorLocation location)
        {
            foreach (var generator in MemberGenerators)
            {

                if (generator.Location == location)
                {

                    generator.Decleration = decleration;
                    generator.Location = location;
                    generator.DataObject = data;
                    //var itemGenerator = generator as IMemberItemGenerator;
                    //if (itemGenerator != null)
                    //{
                    //    itemGenerator.ItemObject
                    //}
                    yield return generator;
                }
            }
        }

        public IEnumerable<IMemberGenerator> GetChildGenerators(CodeTypeDeclaration decleration, TNode data, MemberGeneratorLocation location)
        {
            foreach (var generatorConfig in ChildItemMemberGenerators)
            {
                var selectorConfig = generatorConfig as NodeChildGeneratorConfig<TNode>;
                if (selectorConfig == null)
                    Debug.Log("SelectorCOnfig == null");
                var items = selectorConfig.Selector == null ? data.ChildItems.Cast<IGraphItem>() : selectorConfig.Selector(data);
                foreach (var item in items)
                {
                    if (generatorConfig.ChildType != item.GetType())
                    {
                        Debug.Log(generatorConfig.ChildType.Name + " -----> " +item.GetType().Name);
                        continue;
                    }
                    if (generatorConfig.Generator.Location == location)
                    {
                        var generator = generatorConfig.Generator as IMemberGenerator;
                        generator.Decleration = decleration;
                        generator.Location = location;
                        generator.DataObject = item;
                        //generator.DataObject = data;
                        yield return generator;
                    }
                }
            }

        }


        public NodeGeneratorConfig<TNode> OverrideTypedMethod(Type type, string methodName,Action<TNode,CodeMemberMethod> fillMethod, bool callBase = true, MemberGeneratorLocation location = MemberGeneratorLocation.DesignerFile)
        {
            MemberGenerators.Add(new LambdaMemberGenerator<TNode>(_ =>
            {
                var method = CodeDomHelpers.MethodFromTypeMethod(type, methodName, callBase);
                if (fillMethod != null)
                {
                    fillMethod(_.Data, method);
                }
                return method;
            })
            {
                Location = location
            });
            return this;
        }
    }

    public class ConfigProperty<TData, TType>
    {
        public TType Literal { get; set; }
        public Func<TData, TType> Selector { get; set; }

        public ConfigProperty(TType literal)
        {
            Literal = literal;
        }

        public ConfigProperty(Func<TData, TType> selector)
        {
            Selector = selector;
        }

        public TType GetValue(TData data)
        {
            if (Selector != null)
            {
                return Selector(data);
            }
            return Literal;
        }
    }



}
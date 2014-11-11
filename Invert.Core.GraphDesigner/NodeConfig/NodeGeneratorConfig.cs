using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class NodeGeneratorConfig<TNode> : NodeGeneratorConfig
        where TNode : GenericNode
    {
        public NodeGeneratorConfig<TNode> AddMemberGenerator(Func<LambdaMemberGenerator<TNode>, CodeTypeMember> generate, MemberGeneratorLocation location = MemberGeneratorLocation.DesignerFile)
        {
            MemberGenerators.Add(new LambdaMemberGenerator<TNode>(generate)
            {
                Location = location
            });
            return this;
        }

        public NodeGeneratorConfig<TNode> AddMemberGenerator(IMemberItemGenerator generator)
        {
            MemberGenerators.Add(generator);
            return this;
        }

        public NodeGeneratorConfig<TNode> AddCustomChildMemberGenerator<TChildItem>(Func<TNode, IEnumerable<TChildItem>> selector, IMemberItemGenerator generater)
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

        public NodeGeneratorConfig<TNode> AddCustomChildMemberGenerator<TChildItem>(Func<TNode, IEnumerable<TChildItem>> selector, Func<LambdaMemberItemGenerator<TNode, TChildItem>, CodeTypeMember> generate)
            where TChildItem : IGraphItem
        {
            ChildItemMemberGenerators.Add(new NodeChildGeneratorConfig<TNode>()
            {
                ChildType = typeof(TChildItem),
                Selector = p => selector(p).Cast<IGraphItem>(),
                Generator = new LambdaMemberItemGenerator<TNode, TChildItem>(generate)
            });
            return this;
        }

        public NodeGeneratorConfig<TNode> AddChildMemberGenerator<TChildItem>(IMemberItemGenerator generator)
            where TChildItem : GenericNodeChildItem
        {
            ChildItemMemberGenerators.Add(new NodeChildGeneratorConfig<TNode>()
            {
                ChildType = typeof(TChildItem),
                Generator = generator
            });
            return this;
        }

        public NodeGeneratorConfig<TNode> AddChildMemberGenerator<TChildItem>(Func<LambdaMemberItemGenerator<TNode, TChildItem>, CodeTypeMember> generate)
            where TChildItem : GenericNodeChildItem
        {
            ChildItemMemberGenerators.Add(new NodeChildGeneratorConfig<TNode>()
            {
                ChildType = typeof(TChildItem),
                Generator = new LambdaMemberItemGenerator<TNode, TChildItem>(generate)
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
                    yield return generator;
                }
            }
        }

        public IEnumerable<IMemberItemGenerator> GetChildGenerators(CodeTypeDeclaration decleration, TNode data, MemberGeneratorLocation location)
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
                        var generator = generatorConfig.Generator as IMemberItemGenerator;
                        generator.Decleration = decleration;
                        generator.Location = location;
                        generator.ItemObject = item;
                        generator.DataObject = data;
                        yield return generator;
                    }
                }
            }

        }
    }
}
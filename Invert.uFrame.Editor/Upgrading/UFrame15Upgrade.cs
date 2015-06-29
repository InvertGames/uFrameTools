using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Data.Upgrading;
using UnityEditor;

namespace Invert.uFrame.Editor.Upgrading
{
    public class UFrame15Upgrade : IUpgradeProcessor
    {
        public double Version { get; set; }

        public void Upgrade(INodeRepository repository, IGraphData graphData)
        {
            var nodes = graphData.NodeItems.ToArray();
            foreach (var node in nodes.OfType<ElementData>())
            {

                var foundElement =
                    nodes.OfType<ElementData>().FirstOrDefault(p => p.AssemblyQualifiedName == node.BaseType);

                if (foundElement != null)
                    node.BaseIdentifier = foundElement.Identifier;

                foreach (var item in node.PersistedItems.OfType<IBindableTypedItem>())
                {
                    var uFrameType = graphData.NodeItems.FirstOrDefault(p => p.Name == item.RelatedType);
                    if (uFrameType != null)
                        item.RelatedType = uFrameType.Identifier;
                }

                foreach (var command in node.Commands)
                {
                    command.IsYield = false;
                }

            }

            foreach (var subsystem in nodes.OfType<SubSystemData>())
            {
                if (subsystem.Instances.Count > 0) continue;

                var containing = subsystem.GetContainingNodes(repository).OfType<ElementData>();
                foreach (var node in containing)
                {
                    if (!node.IsMultiInstance)
                    {
                        subsystem.Instances.Add(new RegisteredInstanceData()
                        {
                            Node = subsystem,
                            Name = node.Name,
                            RelatedType = node.Identifier
                        });
                    }
                }
            }
            foreach (var subsystem in nodes.OfType<SceneManagerData>())
            {
                subsystem.Transitions.RemoveAll(p => p.Command == null || string.IsNullOrEmpty(p.ToIdentifier));
            }
            foreach (var viewData in nodes.OfType<ViewData>())
            {
                var newElement = nodes.OfType<ElementData>().FirstOrDefault(p => p.AssemblyQualifiedName == viewData.ForAssemblyQualifiedName);
                if (newElement != null)
                {
                    viewData.ForElementIdentifier = newElement.Identifier;
                }

                if (viewData.ViewForElement == null) continue;
                var generators = uFrameEditor.GetPossibleBindingGenerators(viewData, true, false, true, false).ToArray();

                viewData.Bindings.Clear();
                // Upgrade bindings
                foreach (var item in viewData.ReflectionBindingMethods)
                {
                    var generator = generators.FirstOrDefault(p => p.MethodName == item.Name);
                    if (generator == null || generator.MethodName.EndsWith("Added") || generator.MethodName.EndsWith("Removed"))
                    {
                        //Debug.Log("Generator not found for " + item.Name + " Method. You might need to re-add the binding.");
                        continue;
                    }
                    if (string.IsNullOrEmpty(generator.Item.Identifier))
                    {
                        InvertApplication.Log("Error item is null on binding generator. Can't upgrade item.");
                        continue;
                    }
                    var bindingGenerator = new ViewBindingData()
                    {
                        PropertyIdentifier = generator.Item.Identifier,
                        GeneratorType = generator.GetType().Name,
                        Name = item.Name,
                        Node = viewData
                    };
                    viewData.Bindings.Add(bindingGenerator);
                    if (generator.Item.RelatedNode() is ElementData && generator.Item is ViewModelPropertyData)
                    {
                        bindingGenerator.GeneratorType = "StandardPropertyBindingGenerator";
                    }
                }
            }

            graphData.Version = InvertGraphEditor.CURRENT_VERSION_NUMBER.ToString();
            AssetDatabase.SaveAssets();

            var assetPath = graphData.Project.SystemDirectory;
            var dir = new DirectoryInfo(assetPath);
            var newDirectory = dir.CreateSubdirectory("_DesignerFiles");
            foreach (var file in dir.GetFiles(".designer.cs"))
            {
                file.MoveTo(Path.Combine(newDirectory.Name, file.Name));
            }
            AssetDatabase.Refresh();
        }
    }

    public class UFrame16Upgrade : IUpgradeProcessor
    {
        public double Version
        {
            get { return 1.6; }
        }

        public void Upgrade(INodeRepository repository, IGraphData graphData)
        {
            // TODO
        }
    }
}

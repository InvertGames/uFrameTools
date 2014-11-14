using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public class GenericNodeViewModel<TData> : DiagramNodeViewModel<TData> where TData : GenericNode
    {
        private NodeConfig<TData> _nodeConfig;

        
        public GenericNodeViewModel(TData graphItemObject, DiagramViewModel diagramViewModel)
            : base(graphItemObject, diagramViewModel)
        {
        }

        public NodeConfig<TData> NodeConfig
        {
            get { return _nodeConfig ?? (_nodeConfig = InvertGraphEditor.Container.GetNodeConfig<TData>()); }
        }
        protected override void DataObjectChanged()
        {
            ContentItems.Clear();
            InputConnectorType = typeof (TData);
            OutputConnectorType = typeof (TData);
            IsLocal = InvertGraphEditor.CurrentProject.CurrentGraph.NodeItems.Contains(GraphItemObject);

            foreach (var inputConfig in NodeConfig.Inputs.Where(p => p.IsInput))
            {
                if (!InvertGraphEditor.CurrentProject.CurrentFilter.IsAllowed(null, inputConfig.SourceType)) continue;
                ContentItems.Add(new ConnectorHeaderViewModel()
                {
                    Name = inputConfig.Name,
                    DataObject = inputConfig.IsAlias ? DataObject : GraphItem.GetConnectionReference(inputConfig.ReferenceType),
                    InputConnectorType = inputConfig.ReferenceType,
                    IsInput = true
                });
            }
     
            foreach (var section in NodeConfig.Sections)
            {
                if (InvertGraphEditor.CurrentProject.CurrentFilter.IsAllowed(null, section.ChildType)) continue;
                NodeConfigSection<TData> section1 = section as NodeConfigSection<TData>;
                //if (typeof(IDiagramNode).IsAssignableFrom(section.ChildType) && !InvertGraphEditor.CurrentProject.CurrentFilter.IsAllowed(null, section.ChildType)))
                //continue;

                if (!string.IsNullOrEmpty(section.Name))
                {
                    ContentItems.Add(new GenericItemHeaderViewModel()
                    {
                        Name = section.Name,
                        NodeViewModel = this,
                        NodeConfig = this.NodeConfig,
                        
                        SectionConfig = section1,
                        AddCommand = section1.AllowAdding ? new SimpleEditorCommand<DiagramNodeViewModel>((vm) =>
                        {
                            if (section1.AllowAdding && section1.ReferenceType != null)
                            {
                                if (section1.AllowDuplicates)
                                {
                                    InvertGraphEditor.WindowManager.InitItemWindow(section1.Selector(GraphItem),
                                        (selected) =>
                                        {
                                            GraphItem.AddReferenceItem(selected, section1);
                                        });
                                }
                                else
                                {
                                    InvertGraphEditor.WindowManager.InitItemWindow(section1.Selector(GraphItem).Where(p=>!GraphItem.ChildItems.OfType<GenericReferenceItem>().Select(x=>x.SourceIdentifier).Contains(p.Identifier)),
                                   (selected) =>
                                   {
                                       GraphItem.AddReferenceItem(selected, section1);
                                   });
                                }
                                
                            }
                            else
                            {
                               
                                var item = Activator.CreateInstance(section1.ChildType) as GenericNodeChildItem;
                                item.Node = vm.GraphItemObject as DiagramNode;
                                item.Name = item.Node.Project.GetUniqueName(section1.Name);
                                var node = vm as GenericNodeViewModel<TData>;
                                node.GraphItem.Project.AddItem(item);
                                item.IsEditing = true;
                                OnAdd(section1, item);
                            }
                            
                            
                        }) : null
                    });
                }

                if (section1.Selector != null && section1.ReferenceType == null)
                {
                
                    foreach (var item in section1.Selector(GraphItem))
                    {

                        if (section.ChildType.IsAssignableFrom(item.GetType()))
                        {
                            
                            var vm = GetDataViewModel(item);
                            
                            if (vm == null)
                            {
                                Debug.LogError(string.Format("Couldn't find view-model for {0} in section {1} with child type {2}", item.GetType(), section1.Name,section1.ChildType.Name));
                                continue;
                            }
                            ContentItems.Add(vm);
                        }
                        else
                        {
                            Debug.LogError(string.Format("Types do not match {0} and {1}", section.ChildType, item.GetType()));
                        }
                   
                    }
                }
                else
                {
                    foreach (var item in GraphItem.ChildItems)
                    {
                        if (section.ChildType.IsAssignableFrom(item.GetType()))
                        {
                            var vm = GetDataViewModel(item);
                            if (vm == null)
                            {
                                Debug.LogError(string.Format("Couldn't find view-model for {0}", item.GetType()));
                                continue;
                            }
                            ContentItems.Add(vm);
                        }
                    }
                }
            }
            foreach (var inputConfig in NodeConfig.Inputs.Where(p => p.IsOutput))
            {
                if (!InvertGraphEditor.CurrentProject.CurrentFilter.IsAllowed(null, inputConfig.SourceType)) continue;
                ContentItems.Add(new ConnectorHeaderViewModel()
                {
                    Name = inputConfig.Name,
                    DataObject = inputConfig.IsAlias ? DataObject : GraphItem.GetConnectionReference(inputConfig.ReferenceType),
                    OutputConnectorType = inputConfig.ReferenceType,
                    IsInput = false,
                    IsOutput = true
                });
            }

        }

        protected virtual void OnAdd(NodeConfigSection configSection, GenericNodeChildItem item)
        {

        }
    }
}
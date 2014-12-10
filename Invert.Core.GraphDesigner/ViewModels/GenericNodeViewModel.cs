using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{ 
    public class GenericNodeViewModel<TData> : DiagramNodeViewModel<TData> where TData : GenericNode
    {
        private NodeConfig<TData> _nodeConfig;


        public GenericNodeViewModel(TData graphItemObject, DiagramViewModel diagramViewModel)
            : base(graphItemObject, diagramViewModel)
        {
        }

        public virtual NodeConfig<TData> NodeConfig
        {
            get { return _nodeConfig ?? (
                _nodeConfig = InvertGraphEditor.Container.GetNodeConfig<TData>()); }
        }

        public override IEnumerable<string> Tags
        {
            get
            {
                foreach (var item in GraphItem.Flags)
                {
                    if (item.Key.StartsWith("_")) continue;
                    if (item.Value)
                        yield return item.Key;
                }
                foreach (var item in NodeConfig.Tags)
                {
                    yield return item;
                }
            }
        }

        public override IEnumerable<KeyValuePair<string, ValidatorType>> Issues
        {
            get
            {
                foreach (var item in NodeConfig.Validate(GraphItem))
                {
                    yield return new KeyValuePair<string, ValidatorType>(item.Message, item.Type);
                }
            }
        }

        public override NodeColor Color
        {
            get
            {
                if (NodeConfig.NodeColor == null) return NodeColor.LightGray;
                return NodeConfig.NodeColor.GetValue(GraphItem);
            }
        }

        public override Func<IDiagramNodeItem, IDiagramNodeItem, bool> InputValidator
        {
            get { return GraphItem.ValidateInput; }
        }

        public override Func<IDiagramNodeItem, IDiagramNodeItem, bool> OutputValidator
        {
            get { return GraphItem.ValidateOutput; }
        }

        protected override void CreateContent()
        {
            //base.CreateContent();
            InputConnectorType = NodeConfig.SourceType;
            OutputConnectorType = NodeConfig.SourceType;

            //IsLocal = InvertGraphEditor.CurrentProject.CurrentGraph.NodeItems.Contains(GraphItemObject);
            if (NodeConfig.IsInput)
                ApplyInputConfiguration(NodeConfig,DataObject as IGraphItem,InputConnector,NodeConfig.AllowMultipleInputs);
            if (NodeConfig.IsOutput)
                ApplyOutputConfiguration(NodeConfig, DataObject as IGraphItem, InputConnector, NodeConfig.AllowMultipleOutputs);

            CreateContentByConfiguration(NodeConfig.GraphItemConfigurations, GraphItem);

            AddPropertyFields();
           
            

        }
        
        protected void CreateContentByConfiguration(IEnumerable<GraphItemConfiguration> graphItemConfigurations, GenericNode node = null)
        {
            foreach (var item in graphItemConfigurations.OrderBy(p => p.OrderIndex))
            {
                var proxyConfig = item as ConfigurationProxyConfiguration;
                if (proxyConfig != null)
                {
                    if (!IsVisible(proxyConfig.Visibility)) continue;
                    CreateContentByConfiguration(proxyConfig.ConfigSelector(DataObject as GenericNode));
                    continue;
                }
                var inputConfig = item as NodeInputConfig;
                if (inputConfig != null)
                {
                    if (inputConfig.IsOutput)
                    {
                        AddOutput(inputConfig, node);
                    }
                    else if (inputConfig.IsInput)
                    {
                        AddInput(inputConfig, node);
                    }
                }
                var sectionConfig = item as NodeConfigSectionBase;
                if (sectionConfig != null)
                {
                    AddSection(sectionConfig);
                }
            }
        }

        private void AddSection(NodeConfigSectionBase section)
        {
            if (InvertGraphEditor.CurrentProject.CurrentFilter.IsAllowed(null, section.SourceType)) return;
            var section1 = section as NodeConfigSectionBase;
            if (!IsVisible(section.Visibility)) return;

            if (!string.IsNullOrEmpty(section.Name))
            {
               
                ContentItems.Add(new GenericItemHeaderViewModel()
                {
                    Name = section.Name,
                    NodeViewModel = this,
                    NodeConfig = NodeConfig,
                    SectionConfig = section1,
                    AddCommand = section1.AllowAdding
                        ? new SimpleEditorCommand<DiagramNodeViewModel>((vm) =>
                        {
                            if (section1.AllowAdding && section1.ReferenceType != null && !section1.HasPredefinedOptions)
                            {
                                if (section1.AllowDuplicates)
                                {
                                    InvertGraphEditor.WindowManager.InitItemWindow(section1.GenericSelector(GraphItem).ToArray(),
                                        (selected) => { GraphItem.AddReferenceItem(selected, section1); });
                                }
                                else
                                {
                                    InvertGraphEditor.WindowManager.InitItemWindow(
                                        section1.GenericSelector(GraphItem).ToArray()
                                            .Where(
                                                p =>
                                                    !GraphItem.ChildItems.OfType<GenericReferenceItem>()
                                                        .Select(x => x.SourceIdentifier)
                                                        .Contains(p.Identifier)),
                                        (selected) => { GraphItem.AddReferenceItem(selected, section1); });
                                }
                            }
                            else
                            {
                                if (section1.GenericSelector != null && section1.HasPredefinedOptions)
                                {
                                    InvertGraphEditor.WindowManager.InitItemWindow(section1.GenericSelector(GraphItem).ToArray(),
                                        (selected) =>
                                        {
                                            var item = selected as GenericNodeChildItem;
                                            item.Node = vm.GraphItemObject as DiagramNode;

                                            if (section1.OnAdd != null)
                                                section1.OnAdd(item);
                                            else
                                            {
                                                item.Name = item.Node.Project.GetUniqueName(section1.Name);
                                            }

                                            var node = vm as GenericNodeViewModel<TData>;
                                            node.GraphItem.Project.AddItem(item);
                                            item.IsEditing = true;
                                            OnAdd(section1, item);
                                        });
                                }
                                else
                                {
                                    var item = Activator.CreateInstance(section1.SourceType) as GenericNodeChildItem;
                                    item.Node = vm.GraphItemObject as DiagramNode;
                                    item.Name = item.Node.Project.GetUniqueName(section1.Name);
                                    var node = vm as GenericNodeViewModel<TData>;
                                    node.GraphItem.Project.AddItem(item);
                                    item.IsEditing = true;
                                    OnAdd(section1, item);
                                }
                            }
                        })
                        : null
                });
            }

            if (section1.GenericSelector != null && section1.ReferenceType == null && section1.IsProxy)
            {
                
                foreach (var item in section1.GenericSelector(GraphItem).OfType<IDiagramNodeItem>())
                {
                    
                    if (section.SourceType.IsAssignableFrom(item.GetType()))
                    {
                        var vm = GetDataViewModel(item) as GraphItemViewModel;
                        var itemViewModel = vm as ItemViewModel;
                        if (itemViewModel != null)
                        {
                            itemViewModel.IsEditable = section1.IsEditable;
                            ApplyInputConfiguration(section, item, vm.InputConnector, section.AllowMultipleInputs);
                            ApplyOutputConfiguration(section, item, vm.OutputConnector, section.AllowMutlipleOutputs);
                        }
                        
                        if (vm == null)
                        {
                            Debug.LogError(
                                string.Format(
                                    "Couldn't find view-model for {0} in section {1} with child type {2}",
                                    item.GetType(), section1.Name, section1.SourceType.Name));
                            continue;
                        }
                        
                        
                        ContentItems.Add(vm);
                    }
                    else
                    {
                        Debug.LogError(string.Format("Types do not match {0} and {1}", section.SourceType,
                            item.GetType()));
                    }
                }
            }
            else
            {
                foreach (var item in GraphItem.ChildItems)
                {
                    if (section.SourceType.IsAssignableFrom(item.GetType()))
                    {
                        var vm = GetDataViewModel(item) as ItemViewModel;
                       

                        if (vm == null)
                        {
                            Debug.LogError(string.Format("Couldn't find view-model for {0}", item.GetType()));
                            continue;
                        }
                        vm.IsEditable = section1.IsEditable;
                        if (section1.HasPredefinedOptions)
                        {
                            vm.IsEditable = false;
                        }
                        ApplyInputConfiguration(section, item, vm.InputConnector, section.AllowMultipleInputs);
                        ApplyOutputConfiguration(section, item, vm.OutputConnector, section.AllowMutlipleOutputs);
                        ContentItems.Add(vm);
                    }
                }
            }
        }

        

        private void AddOutput(NodeInputConfig inputConfig, GenericNode node = null)
        {
            if (!IsVisible(inputConfig.Visibility)) return;
            var nodeToUse = node ?? GraphItem;
            var header = new InputOutputViewModel()
            {
                Name = inputConfig.Name.GetValue(node),
                DataObject =
                    inputConfig.IsAlias
                        ? DataObject
                        : inputConfig.GetDataObject(nodeToUse),
                OutputConnectorType = inputConfig.SourceType,
                IsInput = false,
                IsOutput = true
            };

            ContentItems.Add(header);
            ApplyOutputConfiguration(inputConfig, header.DataObject as IGraphItem, header.OutputConnector, inputConfig.AllowMultiple, true);
            header.OutputConnector.Configuration = inputConfig;
        }

        private static void ApplyOutputConfiguration(GraphItemConfiguration inputConfig, IGraphItem dataItem, ConnectorViewModel connector, bool allowMultiple, bool alwaysVisible = false)
        {
            connector.AlwaysVisible = alwaysVisible;
            connector.AllowMultiple = allowMultiple;
            var slot = dataItem as IDiagramNodeItem;
            if (slot != null)
            {
                connector.Validator = slot.ValidateOutput;
            }
        }

        private void AddInput(NodeInputConfig inputConfig, GenericNode node = null)
        {
            if (!IsVisible(inputConfig.Visibility)) return;
            var nodeToUse = node ?? GraphItem;
            var header = new InputOutputViewModel()
            {
                Name = inputConfig.Name.GetValue(nodeToUse),
                DataObject =
                    inputConfig.IsAlias ? DataObject : inputConfig.GetDataObject(nodeToUse),
                InputConnectorType = inputConfig.SourceType,
                IsInput = true
            };
            ContentItems.Add(header);

            ApplyInputConfiguration(inputConfig, header.DataObject as IGraphItem,header.InputConnector, inputConfig.AllowMultiple, true);

            header.InputConnector.Configuration = inputConfig;
        }

        private static void ApplyInputConfiguration(GraphItemConfiguration inputConfig, IGraphItem dataItem, ConnectorViewModel connector,bool allowMultiple, bool alwaysVisible = false)
        {

            connector.AlwaysVisible = alwaysVisible;
            connector.AllowMultiple = allowMultiple;
            var slot = dataItem as IDiagramNodeItem;
            if (slot != null)
            {
                connector.Validator = slot.ValidateInput;
            }
       
        }

        protected bool IsVisible(SectionVisibility section)
        {
            if (section == SectionVisibility.Always) return true;
            if (section == SectionVisibility.WhenNodeIsFilter)
            {
                return InvertGraphEditor.CurrentProject.CurrentFilter == GraphItem;
            }
            return InvertGraphEditor.CurrentProject.CurrentFilter != GraphItem;
        }


        protected virtual void OnAdd(NodeConfigSectionBase configSection, GenericNodeChildItem item)
        {

        }
    }
}
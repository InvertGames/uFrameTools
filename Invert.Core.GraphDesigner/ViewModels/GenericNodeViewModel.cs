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
            get { return NodeConfig.InputValidator; }
        }

        public override Func<IDiagramNodeItem, IDiagramNodeItem, bool> OutputValidator
        {
            get { return NodeConfig.OutputValidator; }
        }

        protected override void CreateContent()
        {
            //base.CreateContent();
            InputConnectorType = typeof(TData);
            OutputConnectorType = typeof(TData);

            IsLocal = InvertGraphEditor.CurrentProject.CurrentGraph.NodeItems.Contains(GraphItemObject);

            foreach (var inputConfig in NodeConfig.Inputs.Where(p => p.IsInput))
            {
                //if (typeof(GenericNode).IsAssignableFrom(inputConfig.SourceType) &&
                //  !InvertGraphEditor.CurrentProject.CurrentFilter.IsAllowed(null, inputConfig.SourceType))
                //{
                //    if (DiagramViewModel.CurrentRepository.CurrentFilter != inputConfig.SourceType)
                //        continue;
                //}
                var header = new InputOutputViewModel()
                {
                    Name = inputConfig.Name.GetValue(GraphItem),
                    DataObject =
                        inputConfig.IsAlias ? DataObject : GraphItem.GetConnectionReference(inputConfig.ReferenceType),
                    InputConnectorType = inputConfig.SourceType,
                    IsInput = true
                };
                ContentItems.Add(header);

                header.InputConnector.AllowMultiple = inputConfig.AllowMultiple;
                header.InputConnector.Validator = inputConfig.Validator;
                header.InputConnector.Configuration = inputConfig;
            }
            foreach (var inputConfig in NodeConfig.Inputs.Where(p => p.IsOutput))
            {
                if (typeof(GenericNode).IsAssignableFrom(inputConfig.SourceType) &&
                    !InvertGraphEditor.CurrentProject.CurrentFilter.IsAllowed(null, inputConfig.SourceType))
                {
                    //if (DiagramViewModel.CurrentRepository.CurrentFilter != inputConfig.SourceType)
                    continue;
                }

                var header = new InputOutputViewModel()
                {
                    Name = inputConfig.Name.GetValue(GraphItem),
                    DataObject =
                        inputConfig.IsAlias
                            ? DataObject
                            : GraphItem.GetConnectionReference(inputConfig.ReferenceType),
                    OutputConnectorType = inputConfig.SourceType,
                    IsInput = false,
                    IsOutput = true
                };
                ContentItems.Add(header);
                header.OutputConnector.AllowMultiple = inputConfig.AllowMultiple;
                header.OutputConnector.Validator = inputConfig.Validator;
                header.OutputConnector.Configuration = inputConfig;
            }

            foreach (var section in NodeConfig.Sections)
            {
                if (InvertGraphEditor.CurrentProject.CurrentFilter.IsAllowed(null, section.ChildType)) continue;
                var section1 = section as NodeConfigSection<TData>;
                //if (typeof(IDiagramNode).IsAssignableFrom(section.ChildType) && !InvertGraphEditor.CurrentProject.CurrentFilter.IsAllowed(null, section.ChildType)))
                //continue;

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
                                        InvertGraphEditor.WindowManager.InitItemWindow(
                                            section1.Selector(GraphItem)
                                                .Where(
                                                    p =>
                                                        !GraphItem.ChildItems.OfType<GenericReferenceItem>()
                                                            .Select(x => x.SourceIdentifier)
                                                            .Contains(p.Identifier)),
                                            (selected) =>
                                            {
                                                GraphItem.AddReferenceItem(selected, section1);
                                            });
                                    }

                                }
                                else
                                {
                                    if (section1.Selector != null && section1.HasPredefinedOptions)
                                    {
                                        InvertGraphEditor.WindowManager.InitItemWindow(section1.Selector(GraphItem),
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
                                        var item = Activator.CreateInstance(section1.ChildType) as GenericNodeChildItem;
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

                if (section1.Selector != null && section1.ReferenceType == null && section1.IsProxy)
                {

                    foreach (var item in section1.Selector(GraphItem))
                    {

                        if (section.ChildType.IsAssignableFrom(item.GetType()))
                        {

                            var vm = GetDataViewModel(item);
                            vm.InputValidator = section1.InputValidator;
                            vm.OutputValidator = section1.OutputValidator;
                            if (vm == null)
                            {
                                Debug.LogError(
                                    string.Format(
                                        "Couldn't find view-model for {0} in section {1} with child type {2}",
                                        item.GetType(), section1.Name, section1.ChildType.Name));
                                continue;
                            }
                            ContentItems.Add(vm);
                        }
                        else
                        {
                            Debug.LogError(string.Format("Types do not match {0} and {1}", section.ChildType,
                                item.GetType()));
                        }

                    }
                }
                else
                {
                    foreach (var item in GraphItem.ChildItems)
                    {
                        if (section.ChildType.IsAssignableFrom(item.GetType()))
                        {
                            var vm = GetDataViewModel(item) as ItemViewModel;
                            vm.InputValidator = section1.InputValidator;
                            vm.OutputValidator = section1.OutputValidator;
                            if (section1.HasPredefinedOptions)
                            {
                                vm.IsEditable = false;
                            }
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

            AddPropertyFields();
           

        }



        protected virtual void OnAdd(NodeConfigSectionBase configSection, GenericNodeChildItem item)
        {

        }
    }
}
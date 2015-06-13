//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace uFrameTestPlugin {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core;
    using Invert.Core.GraphDesigner;
    
    
    public class TestingPluginBase : Invert.Core.GraphDesigner.DiagramPlugin {
        
        private Invert.Core.GraphDesigner.NodeConfig<MyAmazingNodeTypeNode> _MyAmazingNodeType;
        
        private Invert.Core.GraphDesigner.NodeConfig<ClassNodeNode> _ClassNode;
        
        public Invert.Core.GraphDesigner.NodeConfig<MyAmazingNodeTypeNode> MyAmazingNodeType {
            get {
                return _MyAmazingNodeType;
            }
            set {
                _MyAmazingNodeType = value;
            }
        }
        
        public Invert.Core.GraphDesigner.NodeConfig<ClassNodeNode> ClassNode {
            get {
                return _ClassNode;
            }
            set {
                _ClassNode = value;
            }
        }
        
        public override void Initialize(Invert.Core.uFrameContainer container) {
            MyAmazingNodeType = container.AddGraph<MyAmazingGraphGraph, MyAmazingNodeTypeNode>("MyAmazingGraphGraph");
            MyAmazingNodeType.HasSubNode<ClassNodeNode>();
            ClassNode = container.AddNode<ClassNodeNode,ClassNodeNodeViewModel,ClassNodeNodeDrawer>("ClassNode");
            ClassNode.Color(NodeColor.Purple);
        }
    }
    
    public class MyAmazingGraphGraphBase : GenericGraphData<MyAmazingNodeTypeNode> {
    }
    
    public class MyAmazingNodeTypeNodeBase : Invert.Core.GraphDesigner.GenericNode {
        
        public virtual bool AllowMultipleInputs {
            get {
                return false;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return false;
            }
        }
    }
    
    public class ClassNodeNodeBase : Invert.Core.GraphDesigner.GenericNode {
        
        public virtual bool AllowMultipleInputs {
            get {
                return false;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return false;
            }
        }
    }
    
    public class MyAmazingNodeTypeNodeViewModelBase : Invert.Core.GraphDesigner.GenericNodeViewModel<MyAmazingNodeTypeNode> {
        
        public MyAmazingNodeTypeNodeViewModelBase(MyAmazingNodeTypeNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
    
    public class ClassNodeNodeViewModelBase : Invert.Core.GraphDesigner.GenericNodeViewModel<ClassNodeNode> {
        
        public ClassNodeNodeViewModelBase(ClassNodeNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
    
    public class MyAmazingNodeTypeNodeDrawerBase : GenericNodeDrawer<MyAmazingNodeTypeNode,MyAmazingNodeTypeNodeViewModel> {
        
        public MyAmazingNodeTypeNodeDrawerBase(MyAmazingNodeTypeNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
    
    public class ClassNodeNodeDrawerBase : GenericNodeDrawer<ClassNodeNode,ClassNodeNodeViewModel> {
        
        public ClassNodeNodeDrawerBase(ClassNodeNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}

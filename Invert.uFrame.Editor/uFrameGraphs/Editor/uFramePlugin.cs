using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Invert.Core;
using Invert.Core.GraphDesigner;
using UnityEngine;

namespace Invert.uFrame.Editor {
    public partial class uFramePlugin : GameFrameworkBase
    {
        public override void Initialize(uFrameContainer container)
        {
            base.Initialize(container);
            
            ElementViewComponent.Name = "View Component";
            ElementView.Name = "View";
            ElementComputedProperty.Name = "Computed Property";
            StateMachine.Name = "State Machine";
            State.Name = "State";
            SceneManager.Name = "Scene Manager";
            container.Connectable<ElementComponentNode, CommandChildItem>();
            container.Connectable<CommandChildItem, CommandChildItem>();
        }



        public static Invert.Core.RegisteredInstance[] BindingTypes { get; set; }
        public override void Loaded(uFrameContainer container)
        {
            base.Loaded(container);

           // BindingTypes = InvertGraphEditor.Container.Instances.Where(p => p.Base == typeof(uFrameBindingType)).ToArray();
        }

        public override SelectItemTypeCommand GetPropertySelectionCommand()
        {
            return new SelectItemTypeCommand()
            {
                AllowNone = false,
                IncludePrimitives = true,
                //AdditionalTypes = 
            };
            return base.GetPropertySelectionCommand();
        }
    }


}

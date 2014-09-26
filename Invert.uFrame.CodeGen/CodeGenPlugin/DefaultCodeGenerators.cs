using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.uFrame;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor;


public class DefaultCodeGenerators : DiagramPlugin
{
    public override bool EnabledByDefault
    {
        get { return true; }
    }

    public override decimal LoadPriority
    {
        get { return -1; }
    }

    public override bool Enabled
    {
        get { return true; }
    }

    public override void Initialize(uFrameContainer container)
    {
        container.Register<DesignerGeneratorFactory, ElementDataGeneratorFactory>("ElementData");
        container.Register<DesignerGeneratorFactory, EnumDataGeneratorFactory>("EnumData");
        container.Register<DesignerGeneratorFactory, ViewDataGeneratorFactory>("ViewData");
        container.Register<DesignerGeneratorFactory, ViewComponentDataGeneratorFactory>("ViewComponentData");
        container.Register<DesignerGeneratorFactory, SceneManagerDataGeneratorFactory>("SceneManagerData");

        container.Register<IBindingGenerator, StandardPropertyBindingGenerator>("StandardPropertyBindingGenerator");
        container.Register<IBindingGenerator, ComputedPropertyBindingGenerator>("ComputedPropertyBindingGenerator");
        //container.Register<IBindingGenerator, CollectionItemAddedBindingGenerator>("Added");
        //container.Register<IBindingGenerator, CollectionItemRemovedBindingGenerator>("Removed");
        container.Register<IBindingGenerator, ViewCollectionBindingGenerator>("ViewCollectionBindingGenerator");
        container.Register<IBindingGenerator, DefaultCollectionBindingGenerator>("DefaultCollectionBindingGenerator");
        container.Register<IBindingGenerator, InstantiateViewPropertyBindingGenerator>("InstantiateViewPropertyBindingGenerator");

        container.Register<DesignerGeneratorFactory, StateMachineCodeFactory>("StateMachineCodeFactory");
        container.Register<DesignerGeneratorFactory, StateMachineStateCodeFactory>("StateMachineStateCodeFactory");
        container.RegisterInstance<ITypeGeneratorPostProcessor>(new StateMachineViewModelProcessor(),
            "StateMachineViewModelPostProcessor");
    }
}

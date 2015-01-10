using System;
using System.Linq;
using System.Reflection;
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
        }



        public static Invert.Core.RegisteredInstance[] BindingTypes { get; set; }
        public override void Loaded(uFrameContainer container)
        {
            base.Loaded(container);

            BindingTypes = InvertGraphEditor.Container.Instances.Where(p => p.Base == typeof(uFrameBindingType)).ToArray();
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

    public static class uFramePluginContainerExtensions
    {


        public static uFrameBindingType AddBindingMethod(this IUFrameContainer container, Type type, MethodInfo method, Func<IBindableTypedItem, bool> canBind)
        {
            return AddBindingMethod(container, new uFrameBindingType(type, method, canBind), method.Name);
        }
        public static uFrameBindingType AddBindingMethod(this IUFrameContainer container, Type type, string methodName, Func<IBindableTypedItem, bool> canBind)
        {
            return AddBindingMethod(container, new uFrameBindingType(type,methodName,canBind), methodName);
        }

        public static uFrameBindingType AddBindingMethod(this IUFrameContainer container, uFrameBindingType info,
            string name)
        {
            container.RegisterInstance<uFrameBindingType>(info, name);
            return info;
        }
    }
    public class uFrameBindingType
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public Type Type { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public Func<IBindableTypedItem, bool> CanBind { get; set; }

        public uFrameBindingType(Type type, string methodName, Func<IBindableTypedItem, bool> canBind)
        {
            Type = type;
            CanBind = canBind;
            DisplayName = methodName;
            MethodInfo = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(p =>!p.IsDefined(typeof(ObsoleteAttribute),true) && p.Name == methodName);
            if (MethodInfo == null)
            {
                throw new Exception(string.Format("Couldn't register binding for method {0}.{1} because it was not found", type.Name, methodName));
            }
        }

        public uFrameBindingType(Type type, MethodInfo methodInfo, Func<IBindableTypedItem, bool> canBind)
        {
            Type = type;
            MethodInfo = methodInfo;
            CanBind = canBind;
            DisplayName = methodInfo.Name;
            //Description
        }

    }
}

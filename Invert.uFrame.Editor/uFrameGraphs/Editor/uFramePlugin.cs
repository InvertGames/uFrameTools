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
        private string _displayFormat = "{0}";
        public Action<BindingHandlerArgs> HandlerImplementation { get; set; }

        public string DisplayFormat
        {
            get { return _displayFormat; }
            set { _displayFormat = value; }
        }

        public string Description { get; set; }
        public Type Type { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public Func<IBindableTypedItem, bool> CanBind { get; set; }
        public static Type ObservablePropertyType { get; set; }
        public static Type UFGroupType { get; set; }

        public uFrameBindingType SetNameFormat(string format)
        {
            DisplayFormat = format;
            return this;
        }
        public uFrameBindingType SetDescription(string description)
        {
            Description = description;
            return this;
        }

        public uFrameBindingType ImplementWith(Action<BindingHandlerArgs> implement)
        {
            HandlerImplementation = implement;
            return this;
        }
        public uFrameBindingType(Type type, string methodFormat, Func<IBindableTypedItem, bool> canBind)
        {
            Type = type;
            CanBind = canBind;
            DisplayFormat = methodFormat;
            MethodInfo = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(p =>!p.IsDefined(typeof(ObsoleteAttribute),true) && p.Name == methodFormat);
            if (MethodInfo == null)
            {
                throw new Exception(String.Format("Couldn't register binding for method {0}.{1} because it was not found", type.Name, methodFormat));
            }
        }

        public uFrameBindingType(Type type, MethodInfo methodInfo, Func<IBindableTypedItem, bool> canBind)
        {
            Type = type;
            MethodInfo = methodInfo;
            CanBind = canBind;
            DisplayFormat = methodInfo.Name;
            //Description
        }

        public CodeExpression CreateBindingSignature(CodeTypeDeclaration context,  Func<Type, CodeTypeReference> convertGenericParameter, ElementViewNode elementView, ITypedItem sourceItem)
        {
            var elementName = elementView.Element.Name;
            var propertyName = sourceItem.Name;

            var methodInvoke = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), MethodInfo.Name);
            var isExtensionMethod = MethodInfo.IsDefined(typeof(ExtensionAttribute), true);

            for (int index = 0; index < MethodInfo.GetParameters().Length; index++)
            {
                var parameter = MethodInfo.GetParameters()[index];
                if (isExtensionMethod && index == 0) continue;

                var genericArguments = parameter.ParameterType.GetGenericArguments();
                if (typeof (Delegate).IsAssignableFrom(parameter.ParameterType))
                {
                    var method = CreateDelegateMethod(convertGenericParameter, parameter, genericArguments, propertyName);
               
                    methodInvoke.Parameters.Add(new CodeSnippetExpression(String.Format((string) "this.{0}", (object) method.Name)));
                    context.Members.Add(method);
                    if (HandlerImplementation != null)
                    {
                        HandlerImplementation(new BindingHandlerArgs() { View = elementView, SourceItem = sourceItem, Method = method, Decleration = context});
                    }
                }
                else if (ObservablePropertyType.IsAssignableFrom(parameter.ParameterType))
                {
                    methodInvoke.Parameters.Add(new CodeSnippetExpression(String.Format("this.{0}.{1}", elementName, propertyName)));
                }
                else
                {
                    var field = context._private_(parameter.ParameterType, "_{0}{1}", propertyName, parameter.Name);
                    field.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(UFGroupType),
                        new CodeAttributeArgument(new CodePrimitiveExpression(propertyName))));
                    field.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof (SerializeField))));
                    methodInvoke.Parameters.Add(new CodeSnippetExpression(field.Name));
                }
            }
            return methodInvoke;
        }

        public CodeMemberMethod CreateDelegateMethod(Func<Type, CodeTypeReference> convertGenericParameter, ParameterInfo parameter,
            Type[] genericArguments,string propertyName)
        {
            var method = new CodeMemberMethod()
            {
                Name = String.Format("{0}{1}{2}",propertyName, parameter.Name.Substring(0,1).ToUpper(),parameter.Name.Substring(1)),
                Attributes = MemberAttributes.Public
            };
            var isFunc = parameter.ParameterType.Name.Contains("Func");
            if (isFunc)
            {
                var returnType = genericArguments.LastOrDefault();
                if (returnType != null)
                {
                    method.ReturnType = new CodeTypeReference(returnType);
                }
            }
            var index = 1;
            foreach (var item in genericArguments)
            {
                if (isFunc && item == genericArguments.Last()) continue;
                var type = item;
                if (item.IsGenericParameter)
                {
                    method.Parameters.Add(new CodeParameterDeclarationExpression(convertGenericParameter(item), String.Format("arg{0}", index)));
                }
                else
                {
                    method.Parameters.Add(new CodeParameterDeclarationExpression(type, String.Format("arg{0}", index)));
                }
            
            }
            return method;
        }

        public static void CreateActionSignature(Type actionType)
        {
        
        }
    }

    public class BindingHandlerArgs
    {

        /// <summary>
        /// The method that should be properly decorated for the implementation
        /// </summary>
        public CodeMemberMethod Method { get; set; }
        /// <summary>
        /// The view that owns this binding.
        /// </summary>
        public ElementViewNode View { get; set; }
        /// <summary>
        /// The element that belongs to the view that has the binding.
        /// </summary>
        public ElementNode Element { get { return View.Element; } }
        /// <summary>
        /// The item being bound to, Property, Collection or Command.
        /// </summary>
        public ITypedItem SourceItem { get; set; }
        /// <summary>
        /// The current decleration at which the binding is being created inside.  Ie: The View class
        /// </summary>
        public CodeTypeDeclaration Decleration { get; set; }
    }
}

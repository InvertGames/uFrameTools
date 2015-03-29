using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class TypeClassGenerator<TData, TTemplateType> : CodeGenerator
        where TData : class, IDiagramNodeItem
        where TTemplateType : class, IClassTemplate<TData>, new()
    {
        private CodeTypeDeclaration _decleration;
        private TemplateContext<TData> _templateContext;
        private TTemplateType _templateClass;
        private TemplateClass _templateAttribute;

        public Type TemplateType
        {
            get { return typeof(TTemplateType); }
        }

       
        public override string Filename
        {
            get
            {
                var template = new TTemplateType { Ctx = new TemplateContext<TData>(TemplateType) { DataObject = Data,IsDesignerFile = IsDesignerFile} };
                if (IsDesignerFile)
                {
                    return template.OutputPath + ".designer.cs";
                }

                return Path.Combine(template.OutputPath,ClassName(Data) + ".cs");
            }
            set { base.Filename = value; }
        }

        public TTemplateType TemplateClass
        {
            get
            {
                if (_templateClass == null)
                {
                    _templateClass = new TTemplateType();
                    _templateClass.Ctx = TemplateContext;

                }
                return _templateClass;
            }
            set { _templateClass = value; }
        }

        public TemplateContext<TData> TemplateContext
        {
            get
            {
                if (_templateContext == null)
                {
                    _templateContext = CreateTemplateContext();
                }
                return _templateContext;
            }
            set { _templateContext = value; }
        }

        protected virtual TemplateContext<TData> CreateTemplateContext()
        {
            var context = new TemplateContext<TData>(TemplateType);

            context.DataObject = Data;
            context.Namespace = Namespace;
            context.CurrentDecleration = Decleration;
            context.IsDesignerFile = IsDesignerFile;
            return context;
        }

        public virtual bool IsDesignerFileOnly
        {
            get { return false; }
        }

        public TData Data
        {
            get { return ObjectData as TData; }
            set { ObjectData = value; }
        }

        public override bool IsValid()
        {
            
            var template = new TTemplateType {Ctx = new TemplateContext<TData>(TemplateType) {DataObject = Data}};
            return template.CanGenerate;
        }

        public CodeTypeDeclaration Decleration
        {
            get
            {
                return _decleration;
            }
            set { _decleration = value; }
        }

        public virtual string ClassNameFormat
        {
            get { return Attribute.ClassNameFormat; }
        }

        public virtual string ClassName(IDiagramNodeItem node)
        {
            if (!string.IsNullOrEmpty(ClassNameFormat))
            {
                return Regex.Replace(string.Format(ClassNameFormat, node.Name), @"[^a-zA-Z0-9_\.]+", "");
            }
            var typeNode = node as IClassTypeNode;
            if (typeNode != null)
            {
                return Regex.Replace(typeNode.ClassName, @"[^a-zA-Z0-9_\.]+", "");
            }
            return Regex.Replace(node.Name, @"[^a-zA-Z0-9_\.]+", "");
        }

        public virtual string ClassNameBase(IDiagramNodeItem node)
        {
            if (IsDesignerFileOnly)
                return ClassName(node);

            return ClassName(node) + "Base";
        }

        public TemplateClass Attribute
        {
            get
            {
                return _templateAttribute ?? (_templateAttribute = typeof(TTemplateType).GetCustomAttributes(typeof(TemplateClass), true)
                .OfType<TemplateClass>()
                .FirstOrDefault());
            }
        }

        public override Type GeneratorFor
        {
            get { return typeof(TData); }
            set
            {

            }
        }

        public string[] FilterToMembers { get; set; }

        public override void Initialize(CodeFileGenerator codeFileGenerator)
        {
            base.Initialize(codeFileGenerator);
            //if (!string.IsNullOrEmpty(TemplateType.Namespace))
            //    TryAddNamespace(TemplateType.Namespace);
            Decleration = TemplateType.ToClassDecleration();

            var inheritable = Data as GenericInheritableNode;
            if (!Attribute.AutoInherit)
            {
                inheritable = null;
            }
            if (IsDesignerFile && Attribute.Location != MemberGeneratorLocation.DesignerFile)
            {
                Decleration.Name = ClassNameBase(Data);
                if (inheritable != null && inheritable.BaseNode != null)
                {

                    Decleration.BaseTypes.Clear();
                    Decleration.BaseTypes.Add(ClassName(inheritable.BaseNode));
                }

            }
            else
            {
                Decleration.Name = ClassName(Data);
                if (Attribute.Location != MemberGeneratorLocation.DesignerFile)
                {
                    Decleration.BaseTypes.Clear();
                    Decleration.BaseTypes.Add(ClassNameBase(Data));
                }

            }

            Namespace.Types.Add(Decleration);

            ProcessTemplate();
            return; // Skip the stuff below for now

            if (IsDesignerFile)
            {
                // base.Initialize(fileGenerator);

                if (IsDesignerFile)
                {
                    InitializeDesignerFile();
                }
                else
                {
                    InitializeEditableFile();
                }
            }

        }

        protected virtual void InitializeEditableFile()
        {

        }

        protected virtual void InitializeDesignerFile()
        {

        }

        protected void OverrideMethod<TItem>(string name, IEnumerable<TItem> selector, Func<CodeMemberMethod, TItem, CodeMemberMethod> postProcess)
        {
            foreach (var item in selector)
            {
                TItem item1 = item;
                var i = item1 as IDiagramNodeItem;

                OverrideMethod(name, (m) =>
                {
                    if (i != null)
                    {
                        m.Name = i.Name;
                    }
                    return postProcess(m, item1);
                });

            }
        }

        protected void OverrideMethod(string name, Func<CodeMemberMethod, CodeMemberMethod> postProcess)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (postProcess == null) throw new ArgumentNullException("postProcess");

            var method = TemplateType.MethodFromTypeMethod(name, false);
            var result = postProcess(method);
            if (result != null)
            {
                Decleration.Members.Add(result);
            }

        }

        protected void OverrideProperty(string name, Func<CodeMemberProperty, CodeMemberProperty> postProcess)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (postProcess == null) throw new ArgumentNullException("postProcess");

            var oroperty = TemplateType.PropertyFromTypeProperty(name, true);
            var result = postProcess(oroperty);

            if (result != null)
            {

                if (oroperty.HasGet && oroperty.GetStatements.Count < 1)
                {
                    var field = Decleration._private_(oroperty.Type.ToString(), "_" + oroperty.Name);
                    oroperty._get("return {0}", field.Name);

                    if (oroperty.HasSet && oroperty.SetStatements.Count < 1)
                    {
                        oroperty._set("{0} = value", field.Name);
                    }
                }
                Decleration.Members.Add(result);
            }

        }

        protected void OverrideProperty<TItem>(string name, IEnumerable<TItem> selector, Func<CodeMemberProperty, TItem, CodeMemberProperty> postProcess)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (postProcess == null) throw new ArgumentNullException("postProcess");
            foreach (var item in selector)
            {
                var property = TemplateType.PropertyFromTypeProperty(name, true);
                var typed = item as GenericTypedChildItem;
                if (!IsDesignerFile && property.Attributes != MemberAttributes.Final)
                {
                    property.Attributes |= MemberAttributes.Override;
                }
                if (typed != null)
                {

                    if (property.Type.TypeArguments.Count > 0)
                        property.Type.TypeArguments[0] = new CodeTypeReference(typed.RelatedTypeName);
                    else
                    {
                        property.Type = new CodeTypeReference(typed.RelatedTypeName);
                    }
                }
                var graphItem = item as IDiagramNodeItem;
                if (graphItem != null)
                {
                    property.Name = graphItem.Name;
                }
                var result = postProcess(property, item);
                if (result != null)
                {
                    if (property.HasGet && property.GetStatements.Count < 1)
                    {
                        var field = Decleration._private_(property.Type, "_" + property.Name);
                        property._get("return {0}", field.Name);

                        if (property.HasSet && property.SetStatements.Count < 1)
                        {
                            property._set("{0} = value", field.Name);
                        }
                    }
                    Decleration.Members.Add(result);
                }
            }

        }

        public void CallPropertyGet(string name)
        {
            TemplateType.GetProperty(name).GetValue(TemplateClass, null);
            if (TemplateContext.CurrentIterator != null)
            {

            }
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public void ProcessTemplate()
        {
            // Initialize the template
            TemplateContext.Iterators.Clear();
            TemplateClass.TemplateSetup();
            InvertApplication.SignalEvent<ICodeTemplateEvents>(_ => _.TemplateGenerating(TemplateClass, TemplateContext));
            var templateProperties =
                TemplateType.GetPropertiesWithAttribute<TemplateProperty>(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var templateProperty in templateProperties)
            {
                if (FilterToMembers != null && !FilterToMembers.Contains(templateProperty.Key.Name)) continue;
                TemplateContext.RenderTemplateProperty(TemplateClass, templateProperty);
            }
            var templateMethods =
                TemplateType.GetMethodsWithAttribute<TemplateMethod>(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var templateMethod in templateMethods)
            {
                if (FilterToMembers != null && !FilterToMembers.Contains(templateMethod.Key.Name)) continue;
                TemplateContext.RenderTemplateMethod(TemplateClass, templateMethod);
            }

            var templateConstructors =
                TemplateType.GetMethodsWithAttribute<TemplateConstructor>(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var templateConstructor in templateConstructors)
            {
                if (FilterToMembers != null && !FilterToMembers.Contains(templateConstructor.Key.Name)) continue;
                TemplateContext.RenderTemplateConstructor(TemplateClass, templateConstructor);

            }

        }

        //private CodeMemberProperty DoTemplateProperty(KeyValuePair<PropertyInfo, TemplateProperty> templateProperty)
        //{
        //    var domObject = TemplateType.PropertyFromTypeProperty(templateProperty.Key.Name);
        //    TemplateContext.CurrentMember = domObject;
        //    if (templateProperty.Value.AutoFill != AutoFillType.None)
        //    {
        //        domObject.Name = string.Format(templateProperty.Value.NameFormat, this.TemplateContext.Item.Name);
        //    }

        //    if (templateProperty.Value.AutoFill == AutoFillType.NameAndType ||
        //        templateProperty.Value.AutoFill == AutoFillType.NameAndTypeWithBackingField)
        //    {
        //        var typedItem = this.TemplateContext.Item as ITypedItem;
        //        if (typedItem != null)
        //        {
        //            if (domObject.Type.TypeArguments.Count > 0)
        //            {
        //                domObject.Type.TypeArguments.Clear();
        //                domObject.Type.TypeArguments.Add(typedItem.RelatedTypeName);
        //            }
        //            else
        //            {
        //                domObject.Type = new CodeTypeReference(typedItem.RelatedTypeName);
        //            }
        //        }
        //    }
        //    if (templateProperty.Value.AutoFill == AutoFillType.NameAndTypeWithBackingField ||
        //        templateProperty.Value.AutoFill == AutoFillType.NameOnlyWithBackingField)
        //    {  
        //        templateProperty.Key.GetValue(TemplateClass, null);
        //        var field = Decleration._private_(domObject.Type, "_{0}", domObject.Name);
        //        domObject.GetStatements._("return {0}", field.Name);
        //        domObject.SetStatements._("{0} = value", field.Name);

        //    }
        //    else
        //    {
        //        this.TemplateContext.CurrentStatements = domObject.GetStatements;
        //        templateProperty.Key.GetValue(TemplateClass, null);
        //        if (templateProperty.Key.CanWrite)
        //        {
        //            this.TemplateContext.CurrentStatements = domObject.SetStatements;
        //            templateProperty.Key.SetValue(TemplateClass,
        //                GetDefault(templateProperty.Key.PropertyType),
        //                null);
        //        }
        //        if (!IsDesignerFile && domObject.Attributes != MemberAttributes.Final && templateProperty.Value.Location == MemberGeneratorLocation.Both)
        //        {
        //            domObject.Attributes |= MemberAttributes.Override;
        //        }
        //    }
        //    return domObject;
        //}

        //private void DoTemplateConstructor(KeyValuePair<MethodInfo, TemplateConstructor> templateMethod, IDiagramNodeItem data)
        //{
        //    var info = templateMethod.Key;
        //    var dom = templateMethod.Key.ToCodeConstructor();

        //    TemplateContext.CurrentConstructor = dom;
        //    TemplateContext.CurrentStatements = dom.Statements;
        //    var args = new List<object>();
        //    var parameters = info.GetParameters();
        //    foreach (var arg in parameters)
        //    {
        //        args.Add(GetDefault(arg.ParameterType));
        //    }
        //    foreach (var item in templateMethod.Value.BaseCallArgs)
        //    {
        //        dom.BaseConstructorArgs.Add(new CodeSnippetExpression(item));
        //    }


        //    info.Invoke(TemplateClass, args.ToArray());
        //    Decleration.Members.Add(dom);
        //}
        //private void DoTemplateMethod(KeyValuePair<MethodInfo, TemplateMethod> templateMethod, IDiagramNodeItem data)
        //{
        //    MethodInfo info;
        //    var dom = TemplateType.MethodFromTypeMethod(templateMethod.Key.Name, out info, false);
        //    TemplateContext.CurrentMember = dom;
        //    TemplateContext.CurrentStatements = dom.Statements;
        //    var args = new List<object>();
        //    var parameters = info.GetParameters();
        //    foreach (var arg in parameters)
        //    {
        //        args.Add(GetDefault(arg.ParameterType));
        //    }
        //    if (templateMethod.Value.AutoFill != AutoFillType.None)
        //    {
        //        dom.Name = string.Format(templateMethod.Value.NameFormat, data.Name);
        //    }


        //    info.Invoke(TemplateClass, args.ToArray());

        //    var isOverried = false;
        //    if (!IsDesignerFile && dom.Attributes != MemberAttributes.Final && templateMethod.Value.Location == MemberGeneratorLocation.Both)
        //    {
        //        dom.Attributes |= MemberAttributes.Override;
        //        isOverried = true;
        //    }
        //    if ((info.IsVirtual && !IsDesignerFile) || info.IsOverride() )
        //    {
        //        if (templateMethod.Value.CallBase)
        //        {
        //            dom.invoke_base(true);
        //        }
        //    }


        //    Decleration.Members.Add(dom);
        //}
    }


    public class TemplateContext<TData> : TemplateContext
    {
        private Dictionary<string, Func<TData, IEnumerable>> _iterators;
        private Dictionary<string, Func<TData, bool>> _conditions;

        public Type TemplateType { get; set; }

        public TemplateContext(Type templateType)
        {
            TemplateType = templateType;
        }

        public CodeAttributeDeclaration AddAttribute(object type, params string[] parameters)
        {
            var attribute = new CodeAttributeDeclaration(type.ToCodeReference(),
                parameters.Select(p => new CodeAttributeArgument(new CodeSnippetExpression(p))).ToArray());
            if (CurrentMember == null)
            {
                if (CurrentConstructor == null)
                {
                    CurrentDecleration.CustomAttributes.Add(attribute);
                }
                else
                {
                    CurrentConstructor.CustomAttributes.Add(attribute);
                }

            }
            else
            {
                CurrentMember.CustomAttributes.Add(attribute);
            }

            return attribute;

        }

        public TData Data
        {
            get { return (TData)DataObject; }
        }

        public IEnumerable CurrentIterator { get; set; }

        public Action<TemplateContext> CurrentIteratorAction { get; set; }
        public Func<TemplateContext> CurrentIteratorCreateContext { get; set; }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
        public Dictionary<string, Func<TData, IEnumerable>> Iterators
        {
            get { return _iterators ?? (_iterators = new Dictionary<string, Func<TData, IEnumerable>>()); }
            set { _iterators = value; }
        }

        public CodeConstructor CurrentConstructor { get; set; }

        public Dictionary<string, Func<TData, bool>> Conditions
        {
            get { return _conditions ?? (_conditions = new Dictionary<string, Func<TData, bool>>()); }
            set { _conditions = value; }
        }

        public void AddIterator(string memberName, Func<TData, IEnumerable> iterator)
        {
            Iterators.Add(memberName, iterator);
        }
        public void AddCondition(string memberName, Func<TData, bool> condition)
        {

            AddIterator(memberName, _ => condition(_) ? Enumerable.Repeat(_, 1) : Enumerable.Empty<TData>());
        }

        public T ItemAs<T>()
        {
            return (T)Item;
        }

        public CodeMemberProperty DoTemplateProperty(object instance, KeyValuePair<PropertyInfo, TemplateProperty> templateProperty)
        {
            var domObject = TemplateType.PropertyFromTypeProperty(templateProperty.Key.Name);
            CurrentMember = domObject;
            CurrentAttribute = templateProperty.Value;
            if (templateProperty.Value.AutoFill != AutoFillType.None)
            {
                domObject.Name = string.Format(templateProperty.Value.NameFormat, this.Item.Name.Clean());
            }

            if (templateProperty.Value.AutoFill == AutoFillType.NameAndType ||
                templateProperty.Value.AutoFill == AutoFillType.NameAndTypeWithBackingField)
            {
                var typedItem = Item as ITypedItem;
                if (typedItem != null)
                {
                    if (domObject.Type.TypeArguments.Count > 0)
                    {
                        domObject.Type.TypeArguments.Clear();
                        domObject.Type.TypeArguments.Add(typedItem.RelatedTypeName);
                    }
                    else
                    {
                        domObject.Type = new CodeTypeReference(typedItem.RelatedTypeName);
                    }
                }
            }
            PushStatements(domObject.GetStatements);
            templateProperty.Key.GetValue(instance, null);
            PopStatements();
            if (templateProperty.Key.CanWrite)
            {
                PushStatements(domObject.SetStatements);
                //CurrentStatements = domObject.SetStatements;
                templateProperty.Key.SetValue(instance,
                    GetDefault(templateProperty.Key.PropertyType),
                    null);
                PopStatements();
            }

            if (templateProperty.Value.AutoFill == AutoFillType.NameAndTypeWithBackingField ||
                templateProperty.Value.AutoFill == AutoFillType.NameOnlyWithBackingField)
            {
                //templateProperty.Key.GetValue(instance, null);
                var field = CurrentDecleration._private_(domObject.Type, "_{0}", domObject.Name.Clean());
                domObject.GetStatements._("return {0}", field.Name);
                domObject.SetStatements._("{0} = value", field.Name);

            }
            else
            {

                if (!IsDesignerFile && domObject.Attributes != MemberAttributes.Final && templateProperty.Value.Location == MemberGeneratorLocation.Both)
                {
                    domObject.Attributes |= MemberAttributes.Override;
                }
            }
            return domObject;
        }


        public void DoTemplateConstructor(object instance, KeyValuePair<MethodInfo, TemplateConstructor> templateMethod, IDiagramNodeItem data)
        {
            var info = templateMethod.Key;
            var dom = templateMethod.Key.ToCodeConstructor();
            CurrentAttribute = templateMethod.Value;
            CurrentConstructor = dom;
            PushStatements(dom.Statements);
//            CurrentStatements = dom.Statements;
            var args = new List<object>();
            var parameters = info.GetParameters();
            foreach (var arg in parameters)
            {
                args.Add(GetDefault(arg.ParameterType));
            }
            foreach (var item in templateMethod.Value.BaseCallArgs)
            {
                dom.BaseConstructorArgs.Add(new CodeSnippetExpression(item));
            }


            info.Invoke(instance, args.ToArray());
            PopStatements();
            CurrentDecleration.Members.Add(dom);
            InvertApplication.SignalEvent<ICodeTemplateEvents>(_ => _.ConstructorAdded(instance, this, dom));
        }
        public void RenderTemplateProperty(object instance, string propertyName)
        {
            RenderTemplateProperty(instance, instance.GetType().GetProperty(propertyName));
        }

        public void RenderTemplateProperty(object instance, PropertyInfo info)
        {
            RenderTemplateProperty(instance, new KeyValuePair<PropertyInfo, TemplateProperty>(info, info.GetCustomAttributes(typeof(TemplateProperty), true).OfType<TemplateProperty>().FirstOrDefault()));
        }
        public void RenderTemplateProperty(object instance, KeyValuePair<PropertyInfo, TemplateProperty> templateProperty)
        {
            if (templateProperty.Value.Location == MemberGeneratorLocation.DesignerFile &&
                templateProperty.Value.Location != MemberGeneratorLocation.Both && !IsDesignerFile) return;
            if (templateProperty.Value.Location == MemberGeneratorLocation.EditableFile &&
                templateProperty.Value.Location != MemberGeneratorLocation.Both && IsDesignerFile) return;


            //Conditions.ContainsKey(templateProperty.Key.Name);


            if (Iterators.ContainsKey(templateProperty.Key.Name))
            {
                //Debug.Log("Found iterators for " + templateProperty.Key.Name);
                var iterator = Iterators[templateProperty.Key.Name];
                var items = iterator(Data).OfType<IDiagramNodeItem>().ToArray();

                foreach (var item in items)
                {
                    Item = item;
                    var domObject = DoTemplateProperty(instance, templateProperty);
                    CurrentDecleration.Members.Add(domObject);
                    InvertApplication.SignalEvent<ICodeTemplateEvents>(_ => _.PropertyAdded(instance, this, domObject));
                }
            }
            else
            {
                Item = Data as IDiagramNodeItem;
                var domObject = DoTemplateProperty(instance, templateProperty);
                CurrentDecleration.Members.Add(domObject);
                InvertApplication.SignalEvent<ICodeTemplateEvents>(_=>_.PropertyAdded(instance, this, domObject));
            }
        }
        public void RenderTemplateMethod(object instance, string methodName)
        {
            RenderTemplateMethod(instance, instance.GetType().GetMethod(methodName));
        }

        public void RenderTemplateMethod(object instance, MethodInfo info)
        {
            RenderTemplateMethod(instance, new KeyValuePair<MethodInfo, TemplateMethod>(info, info.GetCustomAttributes(typeof(TemplateMethod), true).OfType<TemplateMethod>().FirstOrDefault()));
        }

        public void RenderTemplateMethod(object instance, KeyValuePair<MethodInfo, TemplateMethod> templateMethod)
        {
            if (templateMethod.Value.Location == MemberGeneratorLocation.DesignerFile &&
                templateMethod.Value.Location != MemberGeneratorLocation.Both && !IsDesignerFile) return;
            if (templateMethod.Value.Location == MemberGeneratorLocation.EditableFile &&
                templateMethod.Value.Location != MemberGeneratorLocation.Both && IsDesignerFile) return;
            if (Iterators.ContainsKey(templateMethod.Key.Name))
            {
                var iterator = Iterators[templateMethod.Key.Name];
                var items = iterator(Data).OfType<IDiagramNodeItem>().ToArray();

                foreach (var item in items)
                {
                    Item = item;
                    RenderMethod(instance, templateMethod, item);
                    
                }
            }
            else
            {
                Item = Data as IDiagramNodeItem;
                RenderMethod(instance, templateMethod, Data as IDiagramNodeItem);
            }
        }
        public void RenderTemplateConstructor(object instance, string methodName)
        {
            RenderTemplateConstructor(instance, instance.GetType().GetMethod(methodName));
        }

        public void RenderTemplateConstructor(object instance, MethodInfo info)
        {
            RenderTemplateConstructor(instance, new KeyValuePair<MethodInfo, TemplateConstructor>(info, info.GetCustomAttributes(typeof(TemplateConstructor), true).OfType<TemplateConstructor>().FirstOrDefault()));
        }
        public void RenderTemplateConstructor(object instance, KeyValuePair<MethodInfo, TemplateConstructor> templateConstructor)
        {
            if (templateConstructor.Value.Location == MemberGeneratorLocation.DesignerFile &&
                  templateConstructor.Value.Location != MemberGeneratorLocation.Both && !IsDesignerFile) return;
            if (templateConstructor.Value.Location == MemberGeneratorLocation.EditableFile &&
                templateConstructor.Value.Location != MemberGeneratorLocation.Both && IsDesignerFile) return;
            if (Iterators.ContainsKey(templateConstructor.Key.Name))
            {
                var iterator = Iterators[templateConstructor.Key.Name];
                var items = iterator(Data).OfType<IDiagramNodeItem>().ToArray();

                foreach (var item in items)
                {
                    Item = item;
                    DoTemplateConstructor(instance, templateConstructor, item);
                }
            }
            else
            {
                Item = Data as IDiagramNodeItem;
                DoTemplateConstructor(instance, templateConstructor, Data as IDiagramNodeItem);
            }
        }


        protected void RenderMethod(object instance, KeyValuePair<MethodInfo, TemplateMethod> templateMethod, IDiagramNodeItem data)
        {
            MethodInfo info;
            var dom = TemplateType.MethodFromTypeMethod(templateMethod.Key.Name, out info, false);
            
            CurrentMember = dom;
            CurrentAttribute = templateMethod.Value;
            PushStatements(dom.Statements);
            
            var args = new List<object>();
            var parameters = info.GetParameters();
            foreach (var arg in parameters)
            {
                args.Add(GetDefault(arg.ParameterType));
            }
            if (templateMethod.Value.AutoFill != AutoFillType.None)
            {
                dom.Name = string.Format(templateMethod.Value.NameFormat, data.Name.Clean());
            }

            CurrentDecleration.Members.Add(dom);

            var result = info.Invoke(instance, args.ToArray());
            var a = result as IEnumerable;
            if (a != null)
            {
                var dummyIteraters = a.Cast<object>().ToArray();
                foreach (var item in dummyIteraters)
                {

                }
            }

            PopStatements();

            var isOverried = false;
            if (!IsDesignerFile && dom.Attributes != MemberAttributes.Final && templateMethod.Value.Location == MemberGeneratorLocation.Both)
            {
                dom.Attributes |= MemberAttributes.Override;
                isOverried = true;
            }
            if ((info.IsVirtual && !IsDesignerFile) || (info.IsOverride() && !info.GetBaseDefinition().IsAbstract && IsDesignerFile))
            {
                if (templateMethod.Value.CallBase)
                {
                    //if (!info.IsOverride() || !info.GetBaseDefinition().IsAbstract && IsDesignerFile)
                    //{ 
                    dom.invoke_base(true);
                    //}

                }
            }
            InvertApplication.SignalEvent<ICodeTemplateEvents>(_ => _.MethodAdded(instance, this, dom));


        }
    }

    public interface ICodeTemplateEvents
    {
        void PropertyAdded(object template, TemplateContext templateContext, CodeMemberProperty codeMemberProperty);
        void MethodAdded(object template, TemplateContext templateContext, CodeMemberMethod codeMemberMethod);

        void ConstructorAdded(object template, TemplateContext templateContext, CodeConstructor codeConstructor);
        void TemplateGenerating(object templateClass, TemplateContext templateContext);
    }

    public class TemplateContext
    {
        private Stack<CodeStatementCollection> _contextStatements;
        private CodeStatementCollection _currentStatements;
        public bool IsDesignerFile { get; set; }
        public IDiagramNodeItem DataObject { get; set; }
        public IDiagramNodeItem Item { get; set; }

        public ITypedItem TypedItem
        {
            get { return Item as ITypedItem; }
        }

        public CodeTypeMember CurrentMember { get; set; }

        public CodeMemberEvent CurrentEvent
        {
            get { return CurrentMember as CodeMemberEvent; }
        }

        public CodeMemberProperty CurrentProperty
        {
            get { return CurrentMember as CodeMemberProperty; }
        }

        public CodeMemberMethod CurrentMethod
        {
            get { return CurrentMember as CodeMemberMethod; }
        }
        public TemplateMethod CurrentMethodAttribute
        {
            get { return CurrentAttribute as TemplateMethod; }
        }
        public TemplateProperty CurrentPropertyAttribute
        {
            get { return CurrentAttribute as TemplateProperty; }
        }
        public TemplateConstructor CurrentConstructorAttribute
        {
            get { return CurrentAttribute as TemplateConstructor; }
        }
        public TemplateMember CurrentAttribute { get; set; }
        public CodeTypeDeclaration CurrentDecleration { get; set; }
        public CodeNamespace Namespace { get; set; }
        public void TryAddNamespace(string ns)
        {
            if (string.IsNullOrEmpty(ns) || string.IsNullOrEmpty(ns.Trim())) return;
            foreach (CodeNamespaceImport n in Namespace.Imports)
            {
                if (n.Namespace == ns)
                    return;
            }
            Namespace.Imports.Add(new CodeNamespaceImport(ns));
        }

        public CodeStatementCollection CurrentStatements
        {
            get
            {
                if (ContextStatements.Count < 1)
                    return null;
                return ContextStatements.Peek();
            }
            //set
            //{
            //    _currentStatements = value;
                
            //}
        }

        public void _(string formatString, params object[] args)
        {
            if (CurrentStatements == null)
            {
                CurrentDecleration.Members.Add(new CodeSnippetTypeMember(string.Format(formatString, args)));
                return;
            }
            CurrentStatements._(formatString, args);
        }
        public void _comment(string formatString, params object[] args)
        {
            CurrentStatements.Add(new CodeCommentStatement(string.Format(formatString, args)));
        }
        public CodeConditionStatement _if(string formatString, params object[] args)
        {
            return CurrentStatements._if(formatString, args);
        }

        public void AddInterface(object type, params object[] args)
        {
            CurrentDecleration.BaseTypes.Add(type.ToCodeReference(args));
        }
        public void SetBaseType(object type, params object[] args)
        {
            CurrentDecleration.BaseTypes.Clear();
            CurrentDecleration.BaseTypes.Add(type.ToCodeReference(args));
        }
        public void SetBaseTypeArgument(object type, params object[] args)
        {
            CurrentDecleration.BaseTypes[0].TypeArguments.Clear();
            CurrentDecleration.BaseTypes[0].TypeArguments.Add(type.ToCodeReference(args));
        }
        public void SetTypeArgument(object type, params object[] args)
        {


            if (CurrentProperty != null)
            {
                CurrentProperty.SetTypeArgument(type, args);
            }

            else if (CurrentMethod != null)
            {
                CurrentMethod.ReturnType.TypeArguments.Clear();
                CurrentMethod.ReturnType.TypeArguments.Add(type.ToCodeReference(args));
            }
        }
        public void SetType(object type, params object[] args)
        {
            if (CurrentProperty != null)
            {
                CurrentProperty.Type = type.ToCodeReference(args);
            }

            else if (CurrentMethod != null)
            {
                CurrentMethod.ReturnType = type.ToCodeReference(args);
            }
            else
            {

            }


        }

        public void LazyGet(string fieldName, string createExpression, params object[] args)
        {
            _if("{0}==null", fieldName)
                .TrueStatements._("{0} = {1}", fieldName, string.Format(createExpression, args).ToString());
            _("return {0}", fieldName);
        }

        protected Stack<CodeStatementCollection> ContextStatements
        {
            get { return _contextStatements ?? (_contextStatements = new Stack<CodeStatementCollection>()); }
            set { _contextStatements = value; }
        }

        public void PushStatements(CodeStatementCollection codeStatementCollection)
        {
            ContextStatements.Push(codeStatementCollection);
        }

        public void PopStatements()
        {
            ContextStatements.Pop();
        }

    }
}
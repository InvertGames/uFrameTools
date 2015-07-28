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
    public interface ITemplateClassGenerator
    {
        CodeNamespace Namespace { get; }
        Predicate<IDiagramNodeItem> ItemFilter { get; set; }
        CodeCompileUnit Unit { get; }
        
        Type TemplateType { get; }
        
        Type GeneratorFor { get; }
        
        TemplateContext Context { get; }
        
        string Filename { get; }
        
        List<TemplateMemberResult> Results { get; set; }

        void Initialize(CodeFileGenerator codeFileGenerator);

        IClassTemplate Template { get; } 
    }

    public class TemplateClassGenerator<TData, TTemplateType> : CodeGenerator, ITemplateClassGenerator where TData : class, IDiagramNodeItem
        where TTemplateType : class, IClassTemplate<TData>, new()
    {
        private CodeTypeDeclaration _decleration;
        private TemplateContext<TData> _templateContext;
        private TTemplateType _templateClass;
        private TemplateClass _templateAttribute;
        private KeyValuePair<MethodInfo, GenerateConstructor>[] _templateConstructors;
        private KeyValuePair<MethodInfo, GenerateMethod>[] _templateMethods;
        private KeyValuePair<PropertyInfo, GenerateProperty>[] _templateProperties;
        private List<TemplateMemberResult> _results;
        public Predicate<IDiagramNodeItem> ItemFilter { get; set; }

        public Type TemplateType
        {
            get { return typeof(TTemplateType); }
        }

       
        public override string Filename
        {
            get
            {
                var template = new TTemplateType { Ctx = new TemplateContext<TData>(TemplateType) { DataObject = Data,IsDesignerFile = IsDesignerFile} };
                var customFilenameTemplate = template as ITemplateCustomFilename;
                if (customFilenameTemplate != null)
                {
                    return customFilenameTemplate.Filename;
                }
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
            context.Generator = this;
            context.DataObject = Data;
            context.Namespace = Namespace;
            context.CurrentDeclaration = Decleration;
            context.IsDesignerFile = IsDesignerFile;
            context.ItemFilter = ItemFilter;
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

        public TemplateContext Context
        {
            get { return TemplateContext; }
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
            if (IsDesignerFile && Attribute.Location != TemplateLocation.DesignerFile)
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
                if (Attribute.Location != TemplateLocation.DesignerFile)
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

        public IClassTemplate Template
        {
            get
            {
                return TemplateClass;
            }
        }


        protected virtual void InitializeEditableFile()
        {

        }

        protected virtual void InitializeDesignerFile()
        {

        }
        
        public void ProcessTemplate()
        {
            // Initialize the template
            TemplateContext.Iterators.Clear();
            TemplateClass.TemplateSetup();

            foreach (
                var item in
                    TemplateClass.GetType()
                        .GetCustomAttributes(typeof (TemplateAttribute), true)
                        .OfType<TemplateAttribute>().OrderBy(p=>p.Priority))
            {
                item.Modify(TemplateClass,null,TemplateContext);
            }

            var initializeMethods = TemplateClass.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(p=>p.IsDefined(typeof(TemplateSetup),true)).ToArray();
            foreach (var item in initializeMethods)
            {
                item.Invoke(TemplateClass, null);
            }

            InvertApplication.SignalEvent<ICodeTemplateEvents>(_ => _.TemplateGenerating(TemplateClass, TemplateContext));
          

            foreach (var templateProperty in TemplateProperties)
            {
                if (FilterToMembers != null && !FilterToMembers.Contains(templateProperty.Key.Name)) continue;
                foreach (var item in TemplateContext.RenderTemplateProperty(TemplateClass, templateProperty))
                {
                    Results.Add(new TemplateMemberResult(this,templateProperty.Key,templateProperty.Value,item, Decleration));
                }
            }

            foreach (var templateMethod in TemplateMethods)
            {
                if (FilterToMembers != null && !FilterToMembers.Contains(templateMethod.Key.Name)) continue;
                foreach (var item in TemplateContext.RenderTemplateMethod(TemplateClass, templateMethod))
                {
                    Results.Add(new TemplateMemberResult(this, templateMethod.Key, templateMethod.Value, item, Decleration));
                }
            }

            foreach (var templateConstructor in TemplateConstructors)
            {
                if (FilterToMembers != null && !FilterToMembers.Contains(templateConstructor.Key.Name)) continue;
                foreach (var item in TemplateContext.RenderTemplateConstructor(TemplateClass, templateConstructor))
                {
                    Results.Add(new TemplateMemberResult(this, templateConstructor.Key, templateConstructor.Value, item, Decleration));
                }
            }
        }

        public List<TemplateMemberResult> Results
        {
            get { return _results ?? (_results = new List<TemplateMemberResult>()); }
            set { _results = value; }
        }

        public void Initialize(CodeFileGenerator codeFileGenerator, Predicate<IDiagramNodeItem> itemFilter = null)
        {
            
        }

        public KeyValuePair<MethodInfo, GenerateConstructor>[] TemplateConstructors
        {
            get { return _templateConstructors ?? (_templateConstructors = TemplateType.GetMethodsWithAttribute<GenerateConstructor>(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToArray()); }
            set { _templateConstructors = value; }
        }

        public KeyValuePair<MethodInfo, GenerateMethod>[] TemplateMethods
        {
            get { return _templateMethods ?? (_templateMethods = TemplateType.GetMethodsWithAttribute<GenerateMethod>(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToArray()); }
            set { _templateMethods = value; }
        }

        public KeyValuePair<PropertyInfo, GenerateProperty>[] TemplateProperties
        {
            get { return _templateProperties ?? (_templateProperties = TemplateType.GetPropertiesWithAttribute<GenerateProperty>(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToArray()); }
            set { _templateProperties = value; }
        }
    }

    public class TemplateMemberResult
    {
        public CodeTypeDeclaration Decleration { get; private set; }

        public TemplateMemberResult(ITemplateClassGenerator templateClass, MemberInfo memberInfo, GenerateMember memberAttribute, CodeTypeMember memberOutput, CodeTypeDeclaration decleration)
        {
            Decleration = decleration;
            TemplateClass = templateClass;
            MemberInfo = memberInfo;
            MemberAttribute = memberAttribute;
            MemberOutput = memberOutput;
        }

        public ITemplateClassGenerator TemplateClass { get; set; }
        public MemberInfo MemberInfo { get; set; }
        public GenerateMember MemberAttribute { get; set; }
        public CodeTypeMember MemberOutput { get; set; }
    }

    public interface ITemplateCustomFilename
    {
        string Filename { get; }
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
                    CurrentDeclaration.CustomAttributes.Add(attribute);
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

        public IEnumerable<CodeConstructor> RenderTemplateConstructor(object instance, string methodName)
        {
            return RenderTemplateConstructor(instance, instance.GetType().GetMethod(methodName));
        }

        public IEnumerable<CodeConstructor> RenderTemplateConstructor(object instance, MethodInfo info)
        {
            return RenderTemplateConstructor(instance, new KeyValuePair<MethodInfo, GenerateConstructor>(info, info.GetCustomAttributes(typeof(GenerateConstructor), true).OfType<GenerateConstructor>().FirstOrDefault()));
        }
        public IEnumerable<CodeConstructor> RenderTemplateConstructor(object instance, KeyValuePair<MethodInfo, GenerateConstructor> templateConstructor)
        {
            CurrentAttribute = templateConstructor.Value;
            var attributes = templateConstructor.Key.GetCustomAttributes(typeof(TemplateAttribute), true).OfType<TemplateAttribute>().OrderBy(p => p.Priority).ToArray();

            bool success = true;
            foreach (var attribute in attributes)
            {
                if (!attribute.CanGenerate(instance, templateConstructor.Key, this))
                {
                    success = false;
                }
            }
            if (!success)
            {
                yield break;
            }


            // Default to designer file only
            if (!attributes.OfType<Inside>().Any())
            {
                if (!IsDesignerFile) yield break;
            }

            if (Iterators.ContainsKey(templateConstructor.Key.Name))
            {
                var iterator = Iterators[templateConstructor.Key.Name];
                var items = iterator(Data).OfType<IDiagramNodeItem>().ToArray();

                foreach (var item in items)
                {
                    if (ItemFilter != null && !ItemFilter(item))
                        continue;
                    Item = item;
                    yield return RenderConstructor(instance, templateConstructor, item);
                }
            }
            else
            {
                Item = Data as IDiagramNodeItem;
                if (ItemFilter != null && !ItemFilter(Item))
                    yield break;
                yield return RenderConstructor(instance, templateConstructor, Data as IDiagramNodeItem);
            }
        }
        

        public IEnumerable<CodeMemberProperty> RenderTemplateProperty(object instance, string propertyName)
        {
            return RenderTemplateProperty(instance, instance.GetType().GetProperty(propertyName));
        }

        public IEnumerable<CodeMemberProperty> RenderTemplateProperty(object instance, PropertyInfo info)
        {
            return RenderTemplateProperty(instance, new KeyValuePair<PropertyInfo, GenerateProperty>(info, info.GetCustomAttributes(typeof(GenerateProperty), true).OfType<GenerateProperty>().FirstOrDefault()));
        }
        public IEnumerable<CodeMemberProperty> RenderTemplateProperty(object instance, KeyValuePair<PropertyInfo, GenerateProperty> templateProperty)
        {
            CurrentAttribute = templateProperty.Value;
            var attributes = templateProperty.Key.GetCustomAttributes(typeof (TemplateAttribute), true).OfType<TemplateAttribute>().OrderBy(p=>p.Priority).ToArray();

            bool success = true;
            foreach (var attribute in attributes)
            {
                if (!attribute.CanGenerate(instance, templateProperty.Key, this))
                {
                    success = false;
                }
            }
            if (!success)
            {
                yield break;
            }

            // Default to designer file only
            if (!attributes.OfType<Inside>().Any())
            {
                if (!IsDesignerFile) yield break;
            }

            if (Iterators.ContainsKey(templateProperty.Key.Name))
            {


                var iterator = Iterators[templateProperty.Key.Name];
                var items = iterator(Data).OfType<IDiagramNodeItem>().ToArray();

                foreach (var item in items)
                {
                    if (ItemFilter != null && !ItemFilter(item))
                        continue;
                    Item = item;
                  
                    var domObject = RenderProperty(instance, templateProperty);
                    foreach (var attribute in attributes)
                    {
                        attribute.Modify(instance,templateProperty.Key,this);
                    }
                    CurrentDeclaration.Members.Add(domObject);
                    yield return domObject;
                    InvertApplication.SignalEvent<ICodeTemplateEvents>(_ => _.PropertyAdded(instance, this, domObject));
                }
                Item = null;
            }
            else
            {
                Item = Data as IDiagramNodeItem;
                if (ItemFilter != null && !ItemFilter(Item))
                    yield break;
                var domObject = RenderProperty(instance, templateProperty);
                foreach (var attribute in attributes)
                {
                    attribute.Modify(instance, templateProperty.Key, this);
                }
                CurrentDeclaration.Members.Add(domObject);
                yield return domObject;
                InvertApplication.SignalEvent<ICodeTemplateEvents>(_=>_.PropertyAdded(instance, this, domObject));
                Item = null;
            }
        }
        public IEnumerable<CodeMemberMethod> RenderTemplateMethod(object instance, string methodName)
        {
            return RenderTemplateMethod(instance, instance.GetType().GetMethod(methodName));
        }

        public IEnumerable<CodeMemberMethod> RenderTemplateMethod(object instance, MethodInfo info)
        {
           return RenderTemplateMethod(instance, new KeyValuePair<MethodInfo, GenerateMethod>(info, info.GetCustomAttributes(typeof(GenerateMethod), true).OfType<GenerateMethod>().FirstOrDefault()));
        }
        
        public IEnumerable<CodeMemberMethod> RenderTemplateMethod(object instance, KeyValuePair<MethodInfo, GenerateMethod> templateMethod)
        {
            CurrentAttribute = templateMethod.Value;
            var attributes = templateMethod.Key.GetCustomAttributes(typeof(TemplateAttribute), true).OfType<TemplateAttribute>().OrderBy(p => p.Priority).ToArray();

            bool success = true;
            foreach (var attribute in attributes)
            {
                if (!attribute.CanGenerate(instance, templateMethod.Key, this))
                {
                    success = false;
                }
            }
            if (!success)
            {
                yield break;
            }

            // Default to designer file only
            if (!attributes.OfType<Inside>().Any())
            {
                if (!IsDesignerFile) yield break;
            }
           // if (templateMethod.Value.Location == TemplateLocation.DesignerFile &&
           //     templateMethod.Value.Location != TemplateLocation.Both && !IsDesignerFile) yield break;
           // if (templateMethod.Value.Location == TemplateLocation.EditableFile &&
           //     templateMethod.Value.Location != TemplateLocation.Both && IsDesignerFile) yield break;

           // var forEachAttribute =
           //templateMethod.Key.GetCustomAttributes(typeof(TemplateForEach), true).FirstOrDefault() as
           //    TemplateForEach;

           // var iteratorName = templateMethod.Key.Name;
           // if (forEachAttribute != null)
           // {
           //     iteratorName = forEachAttribute.IteratorProperty;
           //     AddIterator(templateMethod.Key.Name,
           //         delegate(TData arg1)
           //         {
           //             return CreateIterator(instance, iteratorName, arg1);
           //         });
           // }

            if (Iterators.ContainsKey(templateMethod.Key.Name))
            {
                var iterator = Iterators[templateMethod.Key.Name];
                var items = iterator(Data).OfType<IDiagramNodeItem>().ToArray();

                foreach (var item in items)
                {
                    Item = item;
                    if (ItemFilter != null && !ItemFilter(item))
                        continue;
                    var result = RenderMethod(instance, templateMethod, item);
                    foreach (var attribute in attributes)
                    {
                        attribute.Modify(instance, templateMethod.Key, this);
                    }
                    yield return result;

                }
                Item = null;
            }
            else
            {
                Item = Data as IDiagramNodeItem;
                if (ItemFilter != null && !ItemFilter(Item))
                    yield break;
                var result = RenderMethod(instance, templateMethod, Data as IDiagramNodeItem);
                foreach (var attribute in attributes)
                {
                    attribute.Modify(instance, templateMethod.Key, this);
                }
                yield return result;
                Item = null;
            }
        }

        private static IEnumerable CreateIterator(object instance, string iteratorName, TData arg1)
        {
            var property = instance.GetType().GetProperty(iteratorName);
            if (property == null && arg1 != null)
            {
                property = arg1.GetType().GetProperty(iteratorName);
                return property.GetValue(arg1, null) as IEnumerable;
            }
            if (property != null)
            {
                return property.GetValue(instance, null) as IEnumerable;
            }

            throw new Exception(string.Format("ForEach on property '{0}' could not be found on the template, or the node.",
                iteratorName));
        }

        protected CodeConstructor RenderConstructor(object instance, KeyValuePair<MethodInfo, GenerateConstructor> templateMethod, IDiagramNodeItem data)
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
            CurrentDeclaration.Members.Add(dom);
            InvertApplication.SignalEvent<ICodeTemplateEvents>(_ => _.ConstructorAdded(instance, this, dom));
            return dom;
        }
        protected CodeMemberProperty RenderProperty(object instance, KeyValuePair<PropertyInfo, GenerateProperty> templateProperty)
        {
            var domObject = TemplateType.PropertyFromTypeProperty(templateProperty.Key.Name);
            CurrentMember = domObject;
            CurrentAttribute = templateProperty.Value;
            //if (templateProperty.Value.AutoFill != AutoFillType.None)
            //{
            //    domObject.Name = string.Format(templateProperty.Value.NameFormat, this.Item.Name.Clean());
            //}

            //if (templateProperty.Value.AutoFill == AutoFillType.NameAndType ||
            //    templateProperty.Value.AutoFill == AutoFillType.NameAndTypeWithBackingField)
            //{
            //    var typedItem = Item as ITypedItem;
            //    if (typedItem != null)
            //    {
            //        if (domObject.Type.TypeArguments.Count > 0)
            //        {
            //            domObject.Type.TypeArguments.Clear();
            //            domObject.Type.TypeArguments.Add(typedItem.RelatedTypeName);
            //        }
            //        else
            //        {
            //            domObject.Type = new CodeTypeReference(typedItem.RelatedTypeName);
            //        }
            //    }
            //}
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

                if (!IsDesignerFile && domObject.Attributes != MemberAttributes.Final && templateProperty.Value.Location == TemplateLocation.Both)
                {
                    domObject.Attributes |= MemberAttributes.Override;
                }
            
            return domObject;
        }
        protected CodeMemberMethod RenderMethod(object instance, KeyValuePair<MethodInfo, GenerateMethod> templateMethod, IDiagramNodeItem data)
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

            CurrentDeclaration.Members.Add(dom);

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

            //var isOverried = false;
            //if (!IsDesignerFile && dom.Attributes != MemberAttributes.Final && templateMethod.Value.Location == TemplateLocation.Both)
            //{
            //    dom.Attributes |= MemberAttributes.Override;
            //    isOverried = true;
            //}
            //if ((info.IsVirtual && !IsDesignerFile) || (info.IsOverride() && !info.GetBaseDefinition().IsAbstract && IsDesignerFile))
            //{
            //    if (templateMethod.Value.CallBase)
            //    {
            //        //if (!info.IsOverride() || !info.GetBaseDefinition().IsAbstract && IsDesignerFile)
            //        //{ 
            //        dom.invoke_base(true);
            //        //}

            //    }
            //}
            InvertApplication.SignalEvent<ICodeTemplateEvents>(_ => _.MethodAdded(instance, this, dom));
            return dom;

        }

        public void AddMemberOutput(IDiagramNodeItem data, TemplateMemberResult templateMemberResult)
        {
            if (ItemFilter != null && !ItemFilter(data)) 
                return;
            Results.Add(templateMemberResult);
        }

        public override void AddMemberIterator(string name, Func<object, IEnumerable> func)
        {
            base.AddMemberIterator(name, func);
            AddIterator(name, _=> { return func(_); });
        }
    }

    public interface ICodeTemplateEvents
    {
        void PropertyAdded(object template, TemplateContext templateContext, CodeMemberProperty codeMemberProperty);
        void MethodAdded(object template, TemplateContext templateContext, CodeMemberMethod codeMemberMethod);

        void ConstructorAdded(object template, TemplateContext templateContext, CodeConstructor codeConstructor);
        void TemplateGenerating(object templateClass, TemplateContext templateContext);
    }

    public class TemplateException :Exception
    {
        public TemplateException(string message) : base(message)
        {
        }

        public TemplateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    public class TemplateContext
    {

        public string ProcessType(Type t)
        {
            var genericParameter = t.GetGenericParameter();
            if (genericParameter != null)
            {
                var gt = ProcessType(genericParameter);
                if (gt == null) return null;
                return string.Format("{0}<{1}>", t.Name.Replace("`1",""), gt);
            }
            if (typeof(_TEMPLATETYPE_).IsAssignableFrom(t))
            {
                var type = Activator.CreateInstance(t) as _TEMPLATETYPE_;
                return type.TheType(this);
            }

            return null;

        }
        public object GetTemplateProperty(object templateInstance, string propertyName)
        {
            var property = templateInstance.GetType().GetProperty(propertyName);
            if (property == null)
            {
                property = (Item ?? DataObject).GetType()
                    .GetProperty(propertyName);

                if (property == null)
                {
                    throw new TemplateException(string.Format("Template Property Not Found {0}", propertyName));
                }

                return property.GetValue(Item ?? DataObject, null);
            }
            else
            {
                return property.GetValue(templateInstance, null);
            }
        }

        // TODO Remove this and add the Generator collections to here
        public ITemplateClassGenerator Generator { get; set; }

        protected List<TemplateMemberResult> Results
        {
            get { return Generator.Results; }
        }


        private Stack<CodeStatementCollection> _contextStatements;
        private CodeStatementCollection _currentStatements;
        private IDiagramNodeItem _item;
        public bool IsDesignerFile { get; set; }
        public IDiagramNodeItem DataObject { get; set; }

        public IDiagramNodeItem Item
        {
            get { return _item ?? DataObject; }
            set { _item = value; }
        }

        public ITypedItem TypedItem
        {
            get { return Item as ITypedItem; }
        }

        public Predicate<IDiagramNodeItem> ItemFilter { get; set; }

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
        public GenerateMethod CurrentMethodAttribute
        {
            get { return CurrentAttribute as GenerateMethod; }
        }
        public GenerateProperty CurrentPropertyAttribute
        {
            get { return CurrentAttribute as GenerateProperty; }
        }
        public GenerateConstructor CurrentConstructorAttribute
        {
            get { return CurrentAttribute as GenerateConstructor; }
        }
        public GenerateMember CurrentAttribute { get; set; }
        public CodeTypeDeclaration CurrentDeclaration { get; set; }
        public CodeNamespace Namespace { get; set; }
        public void TryAddNamespace(string ns)
        {
            if (Namespace == null) return;
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
                CurrentDeclaration.Members.Add(new CodeSnippetTypeMember(string.Format(formatString, args)));
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
            CurrentDeclaration.BaseTypes.Add(type.ToCodeReference(args));
        }
        public void SetBaseType(object type, params object[] args)
        {
            CurrentDeclaration.BaseTypes.Clear();
            CurrentDeclaration.BaseTypes.Add(type.ToCodeReference(args));
        }
        public void SetBaseTypeArgument(object type, params object[] args)
        {
            CurrentDeclaration.BaseTypes[0].TypeArguments.Clear();
            CurrentDeclaration.BaseTypes[0].TypeArguments.Add(type.ToCodeReference(args));
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



        public virtual void AddMemberIterator(string name, Func<object, IEnumerable> func)
        {
        
        }
    }
}
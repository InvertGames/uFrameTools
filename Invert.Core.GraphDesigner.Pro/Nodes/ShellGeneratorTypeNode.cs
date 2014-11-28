using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Pro;
using UnityEngine;

public class ShellGeneratorTypeNode : GenericInheritableNode
{
    private Type _baseType;
    private string _templateType;
    private string _classNameFormat = "{0}";
    private MemberInfo[] _templateMembers;
    private string _folderName;

    public ShellGeneratorTypeNode()
    {
        _templateType = typeof(System.Object).Name;
    }


    public Type BaseType
    {
        get
        {

            return _baseType ?? (BaseType = InvertApplication.FindTypeByName(TemplateType));
        }
        set
        {
            _baseType = value;
            TemplateMembers = null;
        }
    }

    [JsonProperty, InspectorProperty(InspectorType.TypeSelection)]
    public string TemplateType
    {
        get { return _templateType; }
        set
        {
            _templateType = value;
            _baseType = null;
            TemplateMembers = null;
        }
    }

    [JsonProperty, InspectorProperty]
    public bool IsEditorExtension { get; set; }

    [JsonProperty, InspectorProperty]
    public bool IsDesignerOnly { get; set; }

    [JsonProperty, InspectorProperty]
    public string ClassNameFormat
    {
        get { return _classNameFormat; }
        set { _classNameFormat = value; }
    }

    [JsonProperty, InspectorProperty]
    public string FolderName
    {
        get
        {
            if (string.IsNullOrEmpty(_folderName))
            {
                return Name;
            }
            return _folderName;
        }
        set { _folderName = value; }
    }

    //public ShellNodeGeneratorsSlot ShellNodeShellNodeGeneratorsSlot
    //{
    //    get
    //    {
    //        return this.InputFrom<ShellNodeGeneratorsSlot>();
    //    }
    //}
    public ShellNodeTypeNode GeneratorFor
    {
        get
        {
            var item = this.InputsFrom<MultiOutputSlot<ShellGeneratorTypeNode>>().FirstOrDefault();
            if (item == null) return null;
            return item.Node as ShellNodeTypeNode;
        }
    }
    public IEnumerable<TemplatePropertyReference> Overrides
    {
        get { return ChildItems.OfType<TemplatePropertyReference>(); }
    }

    public MemberInfo[] TemplateMembers
    {
        get
        {
            if (_templateMembers == null)
            {
                _templateMembers =  BaseType.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            }
            return _templateMembers;
        }
        set { _templateMembers = value; }
    }

    public IEnumerable<TemplatePropertyReference> TemplateProperties
    {
        get { return ChildItems.OfType<TemplatePropertyReference>(); }
    }
    public IEnumerable<TemplateMethodReference> TemplateMethods
    {
        get { return ChildItems.OfType<TemplateMethodReference>(); }
    }
    public IEnumerable<TemplateEventReference> TemplateEvents
    {
        get { return ChildItems.OfType<TemplateEventReference>(); }
    }
    public IEnumerable<TemplateFieldReference> TemplateFields
    {
        get { return ChildItems.OfType<TemplateFieldReference>(); }
    }
  
}

public class ShellGeneratorTypeNodeGenerator : GenericNodeGenerator<ShellGeneratorTypeNode>
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        TryAddNamespace("System.CodeDom");

        if (IsDesignerFile)
        {
            InitializeMethod = Decleration.public_override_func(null, "Initialize", "CodeFileGenerator", "generator")
                .invoke_base();
            InitializeDesignerMethod = Decleration.protected_override_func(null, "InitializeDesignerFile")
                .invoke_base();
            InitializeEditorMethod = Decleration.protected_override_func(null, "InitializeEditableFile")
                .invoke_base();

            Decleration.public_override_(typeof(string), "ClassNameFormat")
                ._get("return \"{0}\"", Data.ClassNameFormat);

            if (Data.IsDesignerOnly)
            {
                Decleration.public_override_(typeof(bool), "IsDesignerFileOnly")
                ._get("return true");
            }
            

        }

        //var type = Data.BaseType;

        DoTemplateItem<TemplateProperty,TemplatePropertyReference>(Data.TemplateProperties,typeof(CodeMemberProperty),"Property");
        DoTemplateItem<TemplateMethod,TemplateMethodReference>(Data.TemplateMethods,typeof(CodeMemberMethod),"Method");

        //DoTemplateMethods(type);

        //DoTemplateProperties(type);
    }

    private void DoTemplateItem<TAttribute, TReferenceItem>(
        IEnumerable<TReferenceItem> items, Type codeDomType, string referenceItemType)
        where TReferenceItem : TemplateReference
        where TAttribute : TemplateMember
    {
        foreach (var referenceItem in items)
        {
            var attribute =
                referenceItem.MemberInfo.GetCustomAttributes(typeof(TAttribute), true)
                    .OfType<TAttribute>()
                    .FirstOrDefault();
            if (attribute == null) continue;

            var locationMethod = GetMethodByLocation(attribute.Location);
            var itemArgName = referenceItemType.ToLower();
            var fillMethod = Decleration
               .public_virtual_func(codeDomType,
                   string.Format("Decorate{0}{1}", referenceItem.Name, referenceItemType),
                   codeDomType,
                   itemArgName
                   );
            var sourceType = referenceItem.SelectorItemSectionSourceType;

            if (sourceType != null)
            {
                fillMethod.Parameters.Add(new CodeParameterDeclarationExpression(sourceType,
                              "item"));
            }
            if (IsDesignerFile)
            {
                if (sourceType != null)
                {
                    locationMethod._("Override{0}(\"{1}\", Data.{2}, {3})", referenceItemType, referenceItem.Name,
                        referenceItem.SelectorItem.Name, fillMethod.Name);
                }
                else
                {
                    locationMethod._("Override{0}(\"{1}\", {2})", referenceItemType, referenceItem.Name, fillMethod.Name);
                }
            }
            else
            {
                fillMethod.Attributes |= MemberAttributes.Override;
                fillMethod.invoke_base();
                fillMethod.Statements.Add(new CodeCommentStatement("return null if conditions aren't met."));
                
            }

            fillMethod._("return {0}",itemArgName);
        }
    }
    //private void DoTemplateProperties(Type type)
    //{
    //    // Template Properties
    //    var templateProperties =
    //        type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);

    //    var templatePropertyRefs = Data.ChildItems.OfType<TemplatePropertyReference>().ToArray();
    //    foreach (var templateProperty in templateProperties)
    //    {
    //        var attribute =
    //            templateProperty.GetCustomAttributes(typeof(TemplateProperty), true)
    //                .OfType<TemplateProperty>()
    //                .FirstOrDefault();
    //        if (attribute == null) continue;
    //        var fillMethod = Decleration
    //            .public_virtual_func("CodeMemberProperty",
    //                string.Format("Decorate{0}Property", templateProperty.Name),
    //                "CodeMemberProperty",
    //                "property");
    //        // Add a property override statement
    //        var locationMethod = GetMethodByLocation(attribute.Location);

    //        var propertyRef = templatePropertyRefs.FirstOrDefault(p => p.Name == templateProperty.Name);
    //        var isForEach = false;
    //        if (propertyRef != null)
    //        {
    //            var connectedSelectorSlot = propertyRef.InputFrom<GenericReferenceItem>();
    //            if (connectedSelectorSlot != null)
    //            {
    //                var selectorType = connectedSelectorSlot.SourceItemObject as IShellReferenceType;

    //                if (selectorType != null)
    //                {
    //                    var sourceType = selectorType.ReferenceType;
    //                    if (sourceType != null)
    //                    {
    //                        fillMethod.Parameters.Add(new CodeParameterDeclarationExpression(sourceType.ClassName,
    //                            "item"));
    //                        isForEach = true;
                           
    //                    }
    //                }
    //            }
    //        }
    //        if (IsDesignerFile)
    //        {
    //            if (!isForEach)
    //            {
    //                locationMethod._("OverrideProperty(\"{0}\", {1})", templateProperty.Name, fillMethod.Name);
    //            }
    //            else
    //            {
    //                locationMethod._("OverrideProperty(\"{0}\", Data.{1}, {2} )", templateProperty.Name,
    //                           connectedSelectorSlot.Name, fillMethod.Name);
    //            }
    //        }
    //        else
    //        {
    //            fillMethod.Attributes |= MemberAttributes.Override;
    //            fillMethod.invoke_base();
    //            fillMethod.Statements.Add(new CodeCommentStatement("return null if conditions aren't met."));
    //        }
    //        fillMethod._("return property");
    //    }
    //}
    //private void DoTemplateMethods(Type type)
    //{
    //    // Template Properties
    //    var templateMethods =
    //        type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);

    //    var templateMethodRefs = Data.ChildItems.OfType<TemplateMethodReference>().ToArray();
    //    foreach (var templateMethod in templateMethods)
    //    {
    //        var attribute =
    //            templateMethod.GetCustomAttributes(typeof(TemplateMethod), true)
    //                .OfType<TemplateMethod>()
    //                .FirstOrDefault();
    //        if (attribute == null) continue;

    //        var fillMethod = Decleration
    //            .public_virtual_func("CodeMemberMethod",
    //                string.Format("Decorate{0}Method", templateMethod.Name),
    //                "CodeMemberMethod",
    //                "method");

    //        if (IsDesignerFile)
    //        {
    //            // Add a Method override statement
    //            var locationMethod = GetMethodByLocation(attribute.Location);

    //            var MethodRef = templateMethodRefs.FirstOrDefault(p => p.Name == templateMethod.Name);
    //            var isForEach = false;
    //            if (MethodRef != null)
    //            {
    //                var connectedSelectorSlot = MethodRef.InputFrom<GenericReferenceItem>();
    //                if (connectedSelectorSlot != null)
    //                {
    //                    var selectorType = connectedSelectorSlot.SourceItemObject as IShellReferenceType;

    //                    if (selectorType != null)
    //                    {
    //                        var sourceType = selectorType.ReferenceType;
    //                        if (sourceType != null)
    //                        {
    //                            fillMethod.Parameters.Add(new CodeParameterDeclarationExpression(sourceType.ClassName,
    //                                "item"));
    //                            isForEach = true;
    //                            locationMethod._("OverrideMethod(\"{0}\", Data.{1}, {2} )", templateMethod.Name,
    //                                connectedSelectorSlot.Name, fillMethod.Name);
    //                        }
    //                    }
    //                }
    //            }

    //            if (!isForEach)
    //            {
    //                locationMethod._("OverrideMethod(\"{0}\", {1})", templateMethod.Name, fillMethod.Name);
    //            }
    //        }
    //        else
    //        {
    //            fillMethod.Attributes |= MemberAttributes.Override;
    //            fillMethod.invoke_base();
    //            fillMethod.Statements.Add(new CodeCommentStatement("return null if conditions aren't met."));
    //        }
    //        fillMethod._("return method");
    //    }
    //}
    public CodeMemberMethod GetMethodByLocation(MemberGeneratorLocation location)
    {
        if (location == MemberGeneratorLocation.Both)
        {
            return InitializeMethod;
        }
        if (location == MemberGeneratorLocation.DesignerFile)
        {
            return InitializeDesignerMethod;
        }
        if (location == MemberGeneratorLocation.EditableFile)
        {
            return InitializeEditorMethod;
        }
        return InitializeMethod;
    }

    public CodeMemberMethod InitializeMethod { get; set; }

    public CodeMemberMethod InitializeEditorMethod { get; set; }

    public CodeMemberMethod InitializeDesignerMethod { get; set; }
}

public class TemplateReference : GenericNodeChildItem
{
    public ShellGeneratorTypeNode GeneratorNode
    {
        get { return this.Node as ShellGeneratorTypeNode; }
    }

    public MemberInfo MemberInfo
    {
        get { return GeneratorNode.TemplateMembers.FirstOrDefault(p => p.Name == this.Name); }
    }

    public IShellNodeItem SelectorItem
    {
        get
        {
            return this.InputFrom<IShellNodeItem>();
        }
    }

    //public IShellReferenceType SelectorItemSection
    //{
    //    get
    //    {
    //        if (SelectorItem == null)
    //        {
    //            Debug.Log("SelectorItem is null ");
    //            return null;
    //        }
    //        return SelectorItem.SourceItemObject as IShellReferenceType;
    //    }
    //}

    public string SelectorItemSectionSourceType
    {
        get
        {
            if (SelectorItem == null)
            {
                return null;
            }
            return SelectorItem.ReferenceClassName;
        }
    }


}
public class TemplatePropertyReference : TemplateReference
{
    public PropertyInfo PropertyInfo
    {
        get { return MemberInfo as PropertyInfo; }
    }
}

public class TemplateMethodReference : TemplateReference
{
    public MethodInfo MethodInfo
    {
        get { return MemberInfo as MethodInfo; }
    }
}
public class TemplateFieldReference : TemplateReference
{
    public FieldInfo FieldInfo
    {
        get { return MemberInfo as FieldInfo; }
    }

}
public class TemplateEventReference : TemplateReference
{
    public EventInfo EventInfo
    {
        get { return MemberInfo as EventInfo; }
    }
}
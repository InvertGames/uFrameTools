using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;

public class ShellNodeTypeClassGenerator : GenericNodeGenerator<ShellNodeTypeNode>
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);

    }
}

public class ShellNodeTypeGenerator : TypeClassGenerator<ShellNodeTypeNode, ShellNodeTypeTemplate>
{

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        TryAddNamespace("Invert.Core.GraphDesigner");

        return;
        OverrideProperty("CustomSelectorItems", Data.CustomSelectors, DecorateCustomSelectorItemsProperty);

        if (IsDesignerFile)
        {
            OverrideProperty("InputSlot", Data.InputSlots, DecorateInputSlotProperty);
            OverrideProperty("OutputSlot", Data.OutputSlots, DecorateOutputSlotProperty);

            if (Data.Inheritable)
            {
                if (Data.BaseNode == null)
                {
                    Decleration.BaseTypes.Clear();
                    Decleration.BaseTypes.Add(typeof(GenericInheritableNode));
                }
            }
            OverrideProperty("SectionItems", Data.Sections.Where(p => p.SourceItem is ShellNodeTypeReferenceSection), DecorateReferenceSectionItemsProperty);
            OverrideProperty("SectionItems", Data.Sections, DecorateSectionItemsProperty);
            foreach (var item in Data.InputSlots)
            {
                var item1 = item;
                OverrideProperty("InputItems", item.SourceItem.AcceptableTypes.Select(p => new KeyValuePair<ShellNodeInputsSlot, ShellAcceptableReferenceType>(item1, p)), DecorateInputItemsProperty);
            }
            foreach (var item in Data.OutputSlots)
            {
                var item1 = item;
                OverrideProperty("InputItems", item.SourceItem.AcceptableTypes.Select(p => new KeyValuePair<ShellNodeOutputsSlot, ShellAcceptableReferenceType>(item1, p)), DecorateOutputItemsProperty);
            }
            // OverrideProperty("InputItems", Data.InputSlots, DecorateInputItemsProperty);
            // OverrideProperty("OutputItems", Data.OutputSlots, DecorateOutputItemsProperty);


            //if (Data.IsClassType)
            //{
            //foreach (var generator in Data.Generators)
            //{
            //    Decleration.public_(typeof(string), "NameAs" + generator.Name)
            //       ._get("return string.Format(\"{0}\",Name)", generator.ClassNameFormat);
            //}
            //Decleration.BaseTypes.Add(new CodeTypeReference(typeof(IClassTypeNode)));

            //}

            foreach (var item in Data.IncludedInSections)
            {
                Decleration.BaseTypes.Add(item.ReferenceClassName);
            }
        }

    }

    private CodeMemberProperty DecorateOutputItemsProperty(CodeMemberProperty property, KeyValuePair<ShellNodeOutputsSlot, ShellAcceptableReferenceType> arg2)
    {
        var slot = arg2.Key;

        if (slot.SourceItem.AllowMultiple)
        {
            property.Type.TypeArguments.Clear();
            property.Type.TypeArguments.Add(arg2.Value.SourceItem.ClassName);
            property.Name = string.Format("{0}OutputSlot{1}s", slot.Name, arg2.Value.SourceItem.Name);
            property._get("return {0}OutputSlot.Items.OfType<{1}>()", slot.Name, arg2.Value.SourceItem.ClassName);
        }
        else
        {
            property.Type = new CodeTypeReference(arg2.Value.SourceItem.ClassName);
            property.Name = string.Format("{0}OutputSlot{1}", slot.Name, arg2.Value.SourceItem.Name);
            property._get("return {0}OutputSlot.Item as {1}", slot.Name, arg2.Value.SourceItem.ClassName);
        }
        return property;
    }

    private CodeMemberProperty DecorateInputItemsProperty(CodeMemberProperty property, KeyValuePair<ShellNodeInputsSlot, ShellAcceptableReferenceType> arg2)
    {

        var slot = arg2.Key;

        if (slot.SourceItem.AllowMultiple)
        {
            property.Type.TypeArguments.Clear();
            property.Type.TypeArguments.Add(arg2.Value.SourceItem.ClassName);
            property.Name = string.Format("{0}InputSlot{1}s", slot.Name, arg2.Value.SourceItem.Name);
            property._get("return {0}InputSlot.Items.OfType<{1}>()", slot.Name, arg2.Value.SourceItem.ClassName);
        }
        else
        {
            property.Type = new CodeTypeReference(arg2.Value.SourceItem.ClassName);
            property.Name = string.Format("{0}InputSlot{1}", slot.Name, arg2.Value.SourceItem.Name);
            property._get("return {0}InputSlot.Item as {1}", slot.Name, arg2.Value.SourceItem.ClassName);
        }
        return property;
    }

    private CodeMemberProperty DecorateReferenceSectionItemsProperty(CodeMemberProperty arg1, ShellNodeSectionsSlot arg2)
    {

        arg1.Type.TypeArguments.Clear();
        arg1.Type.TypeArguments.Add(arg2.SourceItem.ReferenceClassName);
        arg1.Name = "Possible" + arg2.Name;
        arg1._get("return this.Project.AllGraphItems.OfType<{0}>()", arg2.SourceItem.ReferenceClassName);
        return arg1;
    }

    private CodeMemberProperty DecorateOutputSlotProperty(CodeMemberProperty property, ShellNodeOutputsSlot slot)
    {
        property.Type = new CodeTypeReference(slot.SourceItem.ClassName);
        property.Name = property.Name + "OutputSlot";
        var attributeDecleration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(OutputSlot)), new CodeAttributeArgument(new CodePrimitiveExpression(slot.Name)));
        property.CustomAttributes.Add(attributeDecleration);
        attributeDecleration.Arguments.Add(
                new CodeAttributeArgument(new CodePrimitiveExpression(slot.SourceItem.AllowMultiple)));
        attributeDecleration.Arguments.Add(
            new CodeAttributeArgument(new CodeSnippetExpression(string.Format("SectionVisibility.{0}", slot.SourceItem.Visibility))));

        //property._get("return GetConnectionReference<{0}>()", arg2.SourceItem.ClassName);
        return property;
    }

    private CodeMemberProperty DecorateInputSlotProperty(CodeMemberProperty property, ShellNodeInputsSlot slot)
    {
        property.Type = new CodeTypeReference(slot.SourceItem.ClassName);
        property.Name = property.Name + "InputSlot";
        var attributeDecleration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(InputSlot)), new CodeAttributeArgument(new CodePrimitiveExpression(slot.Name)));
        attributeDecleration.Arguments.Add(
            new CodeAttributeArgument(new CodePrimitiveExpression(slot.SourceItem.AllowMultiple)));
        attributeDecleration.Arguments.Add(
            new CodeAttributeArgument(new CodeSnippetExpression(string.Format("SectionVisibility.{0}", slot.SourceItem.Visibility))));

        property.CustomAttributes.Add(attributeDecleration);
        //property._get("return GetConnectionReference<{0}>()", arg2.SourceItem.ClassName);
        return property;
    }

    private CodeMemberProperty DecorateCustomSelectorItemsProperty(CodeMemberProperty property, ShellPropertySelectorItem arg2)
    {
        property.Type.TypeArguments.Clear();
        property.Type.TypeArguments.Add(arg2.SelectorFor.ClassName);
        property._get("yield break");
        return property;
    }

    private CodeMemberProperty DecorateOutputItemsProperty(CodeMemberProperty property, ShellNodeOutputsSlot slot)
    {

        if (slot.SourceItem.AllowMultiple)
        {
            property.Type.TypeArguments.Clear();
            property.Type.TypeArguments.Add(slot.SourceItem.ReferenceType.ClassName);
            property.Name = "Connected" + slot.Name;
            property._get("return {0}OutputSlot.Items", slot.Name, slot.SourceItem.ReferenceType.ClassName);
        }
        else
        {
            property.Type = new CodeTypeReference(slot.SourceItem.ReferenceType.ClassName);
            property.Name = "Connected" + slot.Name;
            property._get("return {0}OutputSlot.Items", slot.Name, slot.SourceItem.ReferenceType.ClassName);
        }
        return property;
    }

    private CodeMemberProperty DecorateInputItemsProperty(CodeMemberProperty property, ShellNodeInputsSlot slot)
    {

        if (slot.SourceItem.AllowMultiple)
        {
            property.Type.TypeArguments.Clear();
            property.Type.TypeArguments.Add(slot.SourceItem.ReferenceType.ClassName);
            property.Name = "Connected" + slot.Name;
            property._get("return {0}InputSlot.Items", slot.Name, slot.SourceItem.ReferenceType.ClassName);
        }
        else
        {
            property.Type = new CodeTypeReference(slot.SourceItem.ReferenceType.ClassName);
            property.Name = "Connected" + slot.Name;
            property._get("return {0}InputSlot.Items", slot.Name, slot.SourceItem.ReferenceType.ClassName);
        }
        return property;
    }

    private CodeMemberProperty DecorateSectionItemsProperty(CodeMemberProperty property, ShellNodeSectionsSlot section)
    {
        var referenceSection = section.SourceItem as ShellNodeTypeReferenceSection;
        var attribute = new CodeAttributeDeclaration(new CodeTypeReference(referenceSection == null ? typeof(Section) : typeof(ReferenceSection)),
            new CodeAttributeArgument(new CodePrimitiveExpression(section.Name)));

        attribute.Arguments.Add(
            new CodeAttributeArgument(
                new CodeSnippetExpression(string.Format("SectionVisibility.{0}", section.SourceItem.Visibility))));

        property.Type.TypeArguments.Clear();
        if (referenceSection != null)
        {
            attribute.Arguments.Add(
                new CodeAttributeArgument(new CodePrimitiveExpression(referenceSection.AllowDuplicates)));
            attribute.Arguments.Add(
                new CodeAttributeArgument(new CodePrimitiveExpression(referenceSection.IsAutomatic)));

            attribute.Arguments.Add(
                new CodeAttributeArgument(
                    new CodeSnippetExpression(string.Format("typeof({0})", section.SourceItem.ReferenceClassName))));

            property.Type.TypeArguments.Add(section.SourceItem.ClassName);
            property._get("return ChildItems.OfType<{0}>()", section.SourceItem.ClassName);
        }
        else
        {
            var sectionItem = section.SourceItem as ShellSectionNode;
            property.Type.TypeArguments.Add(sectionItem.ReferenceType.ClassName);
            property._get("return ChildItems.OfType<{0}>()", sectionItem.ReferenceType.ClassName);
        }

        property.CustomAttributes.Add(attribute);



        return property;
    }
}
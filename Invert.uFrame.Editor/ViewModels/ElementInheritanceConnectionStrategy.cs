using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
   
    public class ElementInheritanceConnectionStrategy : DefaultConnectionStrategy<ElementData,ElementData>
    {
        public override Color ConnectionColor
        {
            get { return Color.green; }
        }

        protected override bool IsConnected(ElementData outputData, ElementData inputData)
        {
            return inputData.BaseTypeShortName == outputData.Name;
        }

        protected override void ApplyConnection(ElementData output, ElementData input)
        {
            input.SetBaseElement(output);
        }

        protected override void RemoveConnection(ElementData output, ElementData input)
        {
            input.RemoveBaseElement();
        }
    }
    public class ViewInheritanceConnectionStrategy : DefaultConnectionStrategy<ViewData, ViewData>
    {
        public override Color ConnectionColor
        {
            get { return Color.green; }
        }

        protected override bool IsConnected(ViewData outputData, ViewData inputData)
        {
            return inputData.BaseViewIdentifier == outputData.Identifier;
        }

        protected override void ApplyConnection(ViewData output, ViewData input)
        {
            input.SetBaseView(output);
        }

        protected override void RemoveConnection(ViewData output, ViewData input)
        {
            input.ClearBaseView();
        }
    }
    public class SubsystemConnectionStrategy : DefaultConnectionStrategy<SubSystemData, SubSystemData>
    {
        public override Color ConnectionColor
        {
            get { return Color.green; }
        }

        protected override bool IsConnected(SubSystemData outputData, SubSystemData inputData)
        {
            return inputData.Imports.Contains(outputData.Identifier);
        }

        protected override void ApplyConnection(SubSystemData output, SubSystemData input)
        {
            input.Imports.Add(output.Identifier);
        }

        protected override void RemoveConnection(SubSystemData output, SubSystemData input)
        {
            input.Imports.Remove(output.Identifier);
        }
    }
    public class SceneManagerSubsystemConnectionStrategy : DefaultConnectionStrategy<SubSystemData, SceneManagerData>
    {
        public override Color ConnectionColor
        {
            get { return Color.green; }
        }

        protected override bool IsConnected(SubSystemData outputData, SceneManagerData inputData)
        {
            return inputData.SubSystemIdentifier == outputData.Identifier;
        }

        protected override void ApplyConnection(SubSystemData output, SceneManagerData input)
        {
            input.SubSystemIdentifier = output.Identifier;
        }

        protected override void RemoveConnection(SubSystemData output, SceneManagerData input)
        {
            input.SubSystemIdentifier = null;
        }
    }
    public class ComputedPropertyInputStrategy : DefaultConnectionStrategy<ViewModelPropertyData, ViewModelPropertyData>
    {
        public override Color ConnectionColor
        {
            get { return Color.white; }
        }

        protected override bool IsConnected(ViewModelPropertyData outputData, ViewModelPropertyData inputData)
        {
            return inputData.DependantPropertyIdentifiers.Contains(outputData.Identifier);
        }

        protected override void ApplyConnection(ViewModelPropertyData output, ViewModelPropertyData input)
        {
            input.DependantPropertyIdentifiers.Add(output.Identifier);
        }

        protected override void RemoveConnection(ViewModelPropertyData output, ViewModelPropertyData input)
        {
            input.DependantPropertyIdentifiers.Remove(output.Identifier);
        }
    }

}
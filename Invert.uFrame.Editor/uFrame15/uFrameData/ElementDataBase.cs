using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using UnityEngine;

public abstract class ElementDataBase : DiagramNode, ISubSystemType
{
    public static Dictionary<string, string> TypeNameAliases = new Dictionary<string, string>()
    {
        {"Int32","int"},
        {"Boolean","bool"},
        {"Char","char"},
        {"Decimal","decimal"},
        {"Double","double"},
        {"Single","float"},
        {"String","string"},
    };

    [SerializeField] protected bool _isMultiInstance;

    [SerializeField] protected string _baseIdentifier;

    //[DiagramContextMenu("Print Items")]
    //public void Print()
    //{
    //    Debug.Log(BaseTypeName + ": " +string.Join(Environment.NewLine, AllBaseTypes.Select(p => p.Name).ToArray()));
    //}
    public IEnumerable<ElementData> AllBaseTypes
    {
        get
        {
            var baseType = BaseElement;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseElement;
            }
        }
    }

    public ElementData BaseElement { get { return Project.GetAllElements().FirstOrDefault(p => p.Identifier == this.BaseIdentifier); } }

    public abstract string BaseTypeName { get; }

    public abstract ICollection<ViewModelCollectionData> Collections { get; set; }

    public abstract ICollection<ViewModelCommandData> Commands { get; set; }

    public string ControllerName
    {
        get { return string.Format("{0}Controller", Name.Replace("ViewModel", "")); }
    }

    public Type ControllerType
    {
        get { return InvertApplication.FindType(NameAsController); }
    }

    public Type CurrentViewModelType
    {
        get
        {
            return InvertApplication.FindType(NameAsViewModel);
        }
    }

    public IEnumerable<ElementData> DerivedElements
    {
        get
        {
            var derived = Project.GetAllElements().Where(p => p.BaseIdentifier == Identifier);
            foreach (var derivedItem in derived)
            {
                yield return derivedItem;
                foreach (var another in derivedItem.DerivedElements)
                {
                    yield return another;
                }
            }
        }
    }

    public bool IsDerived
    {
        get { return BaseElement != null; }
    }

    [Obsolete]
    public bool IsForcedMultiInstance
    {
        get
        {
            return
                Graph.NodeItems.ToArray().OfType<ElementDataBase>()
                    .SelectMany(p => p.Collections)
                    .Any(p => p.RelatedType == Identifier) || AllBaseTypes.Any(p => p != this && p.IsMultiInstance);
        }
    }
    [Obsolete]
    public bool IsMultiInstance
    {
        get
        {
            return IsForcedMultiInstance || _isMultiInstance;
        }
        set
        {
            if (IsForcedMultiInstance && !value)
            {
                throw new Exception("This element belongs to a collection so it can NOT be a single instance element.");
            }
            _isMultiInstance = value;
        }
    }

    public override IEnumerable<IDiagramNodeItem> DisplayedItems
    {
        get
        {
            return Properties.Cast<IDiagramNodeItem>()
                .Concat(Collections.Cast<IDiagramNodeItem>())
                .Concat(Commands.Cast<IDiagramNodeItem>());
        }
    }

    public override string Label
    {
        get
        {
            return Name;
        }
    }

    public string NameAsController
    {
        get { return string.Format("{0}Controller", Name); }
    }

    public string NameAsControllerBase
    {
        get
        {
            //if (IsControllerDerived)
            //{
            //    return string.Format("{0}Controller", BaseTypeShortName.Replace("ViewModel", ""));
            //}
            return string.Format("{0}ControllerBase", Name.Replace("ViewModel", ""));
        }
    }

    public string NameAsTypeEnum
    {
        get { return string.Format("{0}Types", Name); }
    }

    public string NameAsVariable
    {
        get { return char.ToLower(Name.First()) + Name.Substring(1); }
    }

    public string NameAsView
    {
        get
        {
            return string.Format("{0}View", Name);
        }
    }

    public string NameAsViewBase
    {
        get
        {
            return string.Format("{0}ViewBase", Name);
        }
    }

    public string NameAsViewModel
    {
        get { return string.Format("{0}ViewModel", Name); }
    }

    public string NameAsViewModelBase
    {
        get { return string.Format("{0}ViewModelBase", Name); }
    }

    public string OldAssemblyName { get; set; }

    public abstract ICollection<ViewModelPropertyData> Properties { get; set; }

    public ElementData RootAbstractElement
    {
        get { return AllBaseTypes.LastOrDefault() ?? this as ElementData; }
    }
    public ElementData RootElement
    {
        get { return AllBaseTypes.LastOrDefault(p => !p.IsTemplate) ?? this as ElementData; }
    }
    public virtual string ViewModelAssemblyQualifiedName
    {
        get
        {
            return uFrameEditor.UFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", NameAsViewModel);
        }
    }

    public IEnumerable<IBindableTypedItem> ViewModelItems
    {
        get
        {
            foreach (var p in Properties)
                yield return p;
            foreach (var x in Collections)
                yield return x;
            foreach (var y in Commands)
                yield return y;
            foreach (var z in ComputedProperties)
                yield return z;
        }
    }

    public string BaseIdentifier
    {
        get { return _baseIdentifier; }
        set { _baseIdentifier = value; }
    }

    public IEnumerable<ComputedPropertyData> ComputedProperties
    {
        get { return this.GetContainingNodes(Graph).OfType<ComputedPropertyData>().Where(p=>p.DependantPropertyIdentifiers.Count > 0); }
    }

    public static string TypeAlias(string typeName)
    {
        if (typeName == null)
        {
            return " ";
        }
        if (TypeNameAliases.ContainsKey(typeName))
        {
            return TypeNameAliases[typeName];
        }
        return typeName;
    }

    public override void BeginEditing()
    {
        base.BeginEditing();
        
    }


    public override bool EndEditing()
    {
        if (!base.EndEditing()) return false;

        var newText = Name;

        if (Project.GetElements().Count(p => p.Name == newText || p.Name == OldName) > 1)
        {
            return false;
        }
        return true;
    }



    //[DiagramContextMenu("")]
    //public void CreateNewBehaviour()
    //{
    //}
}
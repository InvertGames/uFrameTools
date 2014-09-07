using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;
using UnityEngine;

public abstract class ElementDataBase : DiagramNode, ISubSystemType
{
    public static Dictionary<string, string> TypeNameAliases = new Dictionary<string, string>()
    {
        {"System.Int32","int"},
        {"System.Boolean","bool"},
        {"System.Char","char"},
        {"System.Decimal","decimal"},
        {"System.Double","double"},
        {"System.Single","float"},
        {"System.String","string"},
        {"UnityEngine.Vector2","Vector2"},
        {"UnityEngine.Vector3","Vector3"},
    };

    [SerializeField] protected bool _isMultiInstance;

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

    public ElementData BaseElement { get { return Data.GetAllElements().FirstOrDefault(p => p.Name == BaseTypeShortName); } }

    public abstract string BaseTypeName { get; set; }

    public string BaseTypeShortName
    {
        get
        {
            if (string.IsNullOrEmpty(BaseTypeName))
            {
                return uFrameEditor.uFrameTypes.ViewModel.AssemblyQualifiedName.Split(',').FirstOrDefault();
            }
            return BaseTypeName.Split(',').FirstOrDefault();
        }
    }

    public abstract ICollection<ViewModelCollectionData> Collections { get; set; }

    public abstract ICollection<ViewModelCommandData> Commands { get; set; }

    public Type ControllerBaseType
    {
        get
        {
            if (IsDerived)
            {
                return Type.GetType(uFrameEditor.uFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", BaseTypeShortName.Replace("ViewModel", "") + "ControllerBase"));
            }
            return Type.GetType(uFrameEditor.uFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", Name.Replace("ViewModel", "") + "ControllerBase"));
        }
    }

    public string ControllerName
    {
        get { return string.Format("{0}Controller", Name.Replace("ViewModel", "")); }
    }

    public Type ControllerType
    {
        get { return Type.GetType(uFrameEditor.uFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", Name.Replace("ViewModel", "") + "Controller")); }
    }

    public Type CurrentViewModelType
    {
        get
        {
            var name = uFrameEditor.uFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", Name.Replace("ViewModel", "") + "ViewModel");
            return Type.GetType(name);
        }
    }

    public IEnumerable<ElementData> DerivedElements
    {
        get
        {
            var derived = Data.GetAllElements().Where(p => p.BaseTypeShortName == Name);
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

    public bool IsForcedMultiInstance
    {
        get
        {
            return
                Data.NodeItems.OfType<ElementDataBase>()
                    .SelectMany(p => p.Collections)
                    .Any(p => p.RelatedTypeName == Name) || AllBaseTypes.Any(p => p.IsMultiInstance);
        }
    }

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

    public override IEnumerable<IDiagramNodeItem> Items
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
            return Name + (IsMultiInstance ? "*" : "");
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
        get { return string.Format("{0}ViewModel", Name.Replace("ViewModel", "")); }
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
            return uFrameEditor.uFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", NameAsViewModel);
        }
    }

    public IEnumerable<IViewModelItem> ViewModelItems
    {
        get
        {
            return Properties.Cast<IViewModelItem>()
                .Concat(Collections.Cast<IViewModelItem>())
                .Concat(Commands.Cast<IViewModelItem>());
        }
    }

    public static string TypeAlias(string typeName)
    {
        if (typeName == null)
        {
            return "[None]";
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
        OldAssemblyName = AssemblyQualifiedName;
    }


    public override bool EndEditing()
    {
        if (!base.EndEditing()) return false;

        var newText = Name;

        if (Data.GetElements().Count(p => p.Name == newText || p.Name == OldName) > 1)
        {
            return false;
        }
        foreach (var item in Data.GetElements().Where(p => p.BaseTypeShortName == OldName))
        {
            item.BaseTypeName = AssemblyQualifiedName;
        }
        foreach (var item in Data.GetElements().SelectMany(p => p.Properties).Where(p => p.RelatedTypeName == OldName))
        {
            item.RelatedType = AssemblyQualifiedName;
        }
        foreach (var item in Data.GetElements().SelectMany(p => p.Commands).Where(p => p.RelatedTypeName == OldName))
        {
            item.RelatedType = AssemblyQualifiedName;
        }
        foreach (var item in Data.GetElements().SelectMany(p => p.Collections).Where(p => p.RelatedTypeName == OldName))
        {
            item.RelatedType = AssemblyQualifiedName;
        }
        foreach (var result in Data.GetViews().Where(p => p.ForAssemblyQualifiedName == OldAssemblyName))
        {
            result.ForAssemblyQualifiedName = AssemblyQualifiedName;
        }
        return true;
    }



    //[DiagramContextMenu("")]
    //public void CreateNewBehaviour()
    //{
    //}
}
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;
public interface ITypeInfo
{
    bool IsArray { get; }
    bool IsList { get; }
    bool IsEnum { get; }
    ITypeInfo InnerType { get; }
    string TypeName { get; }
    string FullName { get; }
    IEnumerable<IMemberInfo> GetMembers();
    bool IsAssignableTo(ITypeInfo info);
}

public interface IMemberInfo
{
    string MemberName { get;  }
    ITypeInfo MemberType { get; }
}

public class DefaultMemberInfo : IMemberInfo
{
    public string MemberName { get; set; }
    public ITypeInfo MemberType { get; set; }
}


public class SystemTypeInfo : ITypeInfo
{

    public Type SystemType { get; set; }
    public ITypeInfo Other { get; set; }
    public SystemTypeInfo(Type systemType)
    {
        SystemType = systemType;
    }

    public SystemTypeInfo(Type systemType, ITypeInfo other)
    {
        SystemType = systemType;
        Other = other;
    }

    public bool IsArray { get { return SystemType.IsArray; } }

    public bool IsList
    {
        get { return typeof (IList).IsAssignableFrom(SystemType); }
    }

    public bool IsEnum
    {
        get { return SystemType.IsEnum; }
    }

    public ITypeInfo InnerType
    {
        get
        {
            var genericType = SystemType.GetGenericArguments().FirstOrDefault();
            if (genericType != null)
            {
                return new SystemTypeInfo(genericType);
            }
            return null;
        }
    }

    public string TypeName
    {
        get { return SystemType.Name; }
    }

    public string FullName
    {
        get { return SystemType.FullName; }
    }

    public virtual IEnumerable<IMemberInfo> GetMembers()
    {
        if (Other != null)
        {
            foreach (var item in Other.GetMembers())
            {
                yield return item;
            }
        }
        if (SystemType != null)
        {
            if (IsEnum)
            {
                foreach (var item in SystemType.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (item == null) continue;
                    yield return new SystemFieldMemberInfo(item);
                }
            }
            else
            {

                foreach (var item in SystemType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (item == null) continue;
                    yield return new SystemFieldMemberInfo(item);
                }
                foreach (var item in SystemType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    yield return new SystemPropertyMemberInfo(item);
                }
            }
         
        }
        
    }

    public bool IsAssignableTo(ITypeInfo info)
    {
        var systemInfo = info as SystemTypeInfo;
        if (systemInfo != null)
        {
            return systemInfo.SystemType.IsAssignableFrom(SystemType);
        }
        return info.FullName == FullName;
    }
}

public class SystemFieldMemberInfo : IMemberInfo
{
    private FieldInfo FieldInfo;

    public SystemFieldMemberInfo(FieldInfo fieldInfo)
    {
        FieldInfo = fieldInfo;
    }

    public string MemberName { get { return FieldInfo.Name; } }

    public ITypeInfo MemberType
    {
        get
        {
            return new SystemTypeInfo(FieldInfo.FieldType);
        }
    }
}
public class SystemPropertyMemberInfo : IMemberInfo
{
    private PropertyInfo PropertyInfo;

    public SystemPropertyMemberInfo(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
    }

    public string MemberName { get { return PropertyInfo.Name; } }

    public ITypeInfo MemberType
    {
        get
        {
            return new SystemTypeInfo(PropertyInfo.PropertyType);
        }
    }
}
public class GenericTypedChildItem : GenericNodeChildItem, IBindableTypedItem, ISerializeablePropertyData, IDataRecordRemoved, IMemberInfo
{
    protected string _type = string.Empty;

    public Type Type
    {
        get
        {
            if (string.IsNullOrEmpty(RelatedType)) return null;

            return InvertApplication.FindTypeByName(RelatedType);
        }
    }

    public string NameAsChangedMethod
    {
        get { return string.Format("{0}Changed", Name); }
    }

    [JsonProperty]
    public string RelatedType
    {
        get { return _type; }
        set
        {

            this.Changed("RelatedType", ref _type, value);
        }
    }

    public virtual string DefaultTypeName
    {
        get { return string.Empty; }
    }

    public virtual string RelatedTypeName
    {
        get
        {
            var outputClass = RelatedTypeNode;
            if (outputClass != null)
            {
                return outputClass.ClassName;
            }

            var relatedNode = this.RelatedNode();

            if (relatedNode != null)
                return relatedNode.Name;

            return string.IsNullOrEmpty(RelatedType) ? DefaultTypeName : RelatedType;
        }
    }

    public virtual IClassTypeNode RelatedTypeNode
    {
        get
        {

            var result = this.OutputTo<IClassTypeNode>();
            if (result == null)
            {
                return this.Repository.AllOf<IClassTypeNode>().FirstOrDefault(p => p.Identifier == RelatedType) as IClassTypeNode;
            }
            return result;
        }
    }

    public bool AllowEmptyRelatedType
    {
        get { return false; }
    }

    public string FieldName
    {
        get
        {
            return string.Format("_{0}Property", Name);
        }
    }

    public string ViewFieldName
    {
        get
        {
            return string.Format("_{0}", Name);
        }
    }

    public void SetType(IDesignerType input)
    {
        this.RelatedType = input.Identifier;
    }

    public void RemoveType()
    {
        this.RelatedType = typeof(string).Name;
    }



    public IDiagramNode TypeNode()
    {
        return this.RelatedNode();
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ItemType", new JSONData(_type ?? string.Empty));
    }

    public override void Deserialize(JSONClass cls)
    {
        base.Deserialize(cls);

        if (cls["ItemType"] != null)
            _type = cls["ItemType"].Value.Split(',')[0].Split('.').Last();
    }

    public override string FullLabel
    {
        get { return Name; }
    }

    public override string Label
    {
        get { return Name; }
    }

    public override void Remove(IDiagramNode diagramNode)
    {

    }

    public override void RecordRemoved(IDataRecord record)
    {
        if (RelatedType == record.Identifier)
        {
            RemoveType();
        }
    }

    public virtual string MemberName { get { return this.Name; } }
    public virtual ITypeInfo MemberType
    {
        get
        {
            var relatedNode = this.RelatedTypeNode as ITypeInfo;
            if (relatedNode != null)
            {
                return relatedNode;
            }
            return new SystemTypeInfo(InvertApplication.FindTypeByName(RelatedType));
        }
     
    }
}
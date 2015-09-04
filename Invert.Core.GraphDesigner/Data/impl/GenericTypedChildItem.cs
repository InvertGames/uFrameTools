using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;
public interface ITypeInfo
{
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
        foreach (var item in SystemType.GetFields())
        {
            yield return new DefaultMemberInfo() { MemberName = item.Name, MemberType = new SystemTypeInfo(item.FieldType) };
        }
        foreach (var item in SystemType.GetProperties())
        {
            yield return new DefaultMemberInfo() { MemberName = item.Name, MemberType = new SystemTypeInfo(item.PropertyType) };
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
            var relatedNode = this.OutputTo<ITypeInfo>();
            if (relatedNode != null)
            {
                return relatedNode;
            }
            return new SystemTypeInfo(InvertApplication.FindTypeByNameExternal(RelatedType));
        }
     
    }
}
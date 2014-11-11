using System;
using System.Reflection;
using Invert.uFrame.Editor;

public class UFType : IJsonObject
{
    private Assembly _assembly;
    private string _name;

    public Type SystemType
    {
        get
        {
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var foundType = a.GetType(FullName);
                if (foundType != null)
                {
                    return foundType;
                }
            }
            return null;
        }
    }

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public string Namespace { get; set; }

    public string FullName
    {
        get { return string.Format("{0}.{1}", Namespace, Name); }
    }

    public static bool operator ==(UFType point1, UFType point2)
    {
        if (System.Object.Equals(point1, point2))
        {
            return true;
        }
        if (System.Object.ReferenceEquals(point2, null)) return false;
        if (System.Object.ReferenceEquals(point1, null)) return false;

        if (point1.FullName == point2.FullName)
        {
            return true;
        }

        return false;
    }

    public static bool operator ==(UFType point1, Type point2)
    {
        if (System.Object.ReferenceEquals(point2, null)) return false;
        if (System.Object.ReferenceEquals(point1, null)) return false;

        if (point1.FullName == point2.FullName)
        {
            return true;
        }

        return false;
    }

    public static bool operator !=(UFType point1, Type point2)
    {
        return !(point1 == point2);
    }

    public static bool operator !=(UFType point1, UFType point2)
    {
        return !(point1 == point2);
    }

    public string GetAssemblyQualifiedName(Assembly assembly)
    {
        var assemblyName = assembly.GetName();
        return string.Format("{0}, {1}, Version={2}, Culture={3}, PublicKeyToken={4}",
            FullName,
            assemblyName.Name, assemblyName.Version, assemblyName.CultureInfo, assemblyName.GetPublicKeyToken());
    }

    public void Serialize(JSONClass cls)
    {
        cls["Name"] = Name;
        cls["Namespace"] = Namespace;
    }

    public void Deserialize(JSONClass cls, INodeRepository repository)
    {
        if (cls["Name"] != null)
            Name = cls["Name"].Value ?? string.Empty;
        if (cls["Namespace"] != null)
            Namespace = cls["Namespace"].Value ?? string.Empty;
    }
}
using System;

public interface ISerializeablePropertyData
{
    string Name { get; }
    Type Type { get; }
    string RelatedTypeName { get; }
    string FieldName { get; }
    IDiagramNode TypeNode();
}
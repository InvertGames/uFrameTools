using System.Collections.Generic;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;

public class ShellNodeConfigSectionPointer : GenericReferenceItem<ShellNodeConfigSection>, IShellNodeConfigItem
{
    private SectionVisibility _visibility;
    private int _column;
    private int _row;

    [InspectorProperty, JsonProperty]
    public int Row
    {
        get { return _row; }
        set
        {
            _row = value;
            this.Changed("Row", _row, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public int Column
    {
        get { return _column; }
        set
        {
            _column = value;
            this.Changed("Column", _column, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public SectionVisibility Visibility
    {
        get { return _visibility; }
        set
        {
            _visibility = value;
            this.Changed("Visibility",_visibility,value);
        }
    }

    public string ReferenceClassName
    {
        get { return this.SourceItem.ReferenceClassName; }
    }

    public string ClassName
    {
        get { return this.SourceItem.ClassName; }
    }

    public IEnumerable<IShellNodeConfigItem> IncludedInSections
    {
        get { return this.OutputsTo<IShellNodeConfigItem>(); }
    }

    public string TypeName
    {
        get { return SourceItem.TypeName; }
        set
        {
            
        }
    }

    public bool AllowMultiple { get; set; }
}
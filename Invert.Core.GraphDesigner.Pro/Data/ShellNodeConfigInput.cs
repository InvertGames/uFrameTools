using System.Text.RegularExpressions;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;

public class ShellNodeConfigInput : ShellNodeConfigItem, IShellSlotType
{
    private bool _allowMultiple;

    public bool IsOutput
    {
        get { return false; }
        set
        {

        }
    }

    [JsonProperty, InspectorProperty]
    public bool AllowMultiple
    {
        get { return _allowMultiple; }
        set
        {
            _allowMultiple = value;
            this.Changed("AllowMultiple", _allowMultiple, value);
        }
    }

    public override string TypeName
    {
        get
        {
            return Regex.Replace(Name, @"[^a-zA-Z0-9_\.]+", "");

        }
        set { }
    }

    public override string ClassName
    {
        get { return TypeName; }
    }

    [InspectorProperty, JsonProperty]
    public bool AllowSelection { get; set; }
}
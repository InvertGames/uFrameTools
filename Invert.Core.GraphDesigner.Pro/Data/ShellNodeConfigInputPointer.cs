using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;

public class ShellNodeConfigInputPointer : GenericReferenceItem<ShellNodeConfigInput>
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
            this.Changed("Visibility", _visibility, value);
        }
    }
    public string ClassName
    {
        get { return this.SourceItem.TypeName; }
    }
}
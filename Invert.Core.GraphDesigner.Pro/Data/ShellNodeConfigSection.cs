using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;

public class ShellNodeConfigSection : ShellNodeConfigItem
{
    private bool _allowAdding;
    private ShellNodeConfigSectionType _sectionType;
    private bool _isTyped;
    private bool _isEditable;
    private bool _allowDuplicates;
    private bool _isAutomatic;
    private bool _hasPredefinedOptions;

    [JsonProperty, InspectorProperty]
    public ShellNodeConfigSectionType SectionType
    {
        get { return _sectionType; }
        set
        {
            _sectionType = value;
            this.Changed("SectionType", _sectionType, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public bool IsTyped
    {
        get { return _isTyped; }
        set
        {
            _isTyped = value;
            this.Changed("IsTyped", _isTyped, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public virtual bool AllowAdding
    {
        get
        {
            if (SectionType == ShellNodeConfigSectionType.ChildItems)
            {
                return true;
            }
            return _allowAdding;
        }
        set
        {
            _allowAdding = value;
            this.Changed("AllowAdding", _allowAdding, value);
        }
    }

    public override string ClassName
    {
        get
        {
            if (SectionType == ShellNodeConfigSectionType.ChildItems)
            {
                return TypeName + "ChildItem";
            }
            return TypeName + "Reference";
        }
    }


    [InspectorProperty, JsonProperty]
    public bool IsEditable
    {
        get { return _isEditable; }
        set
        {
            _isEditable = value;
            this.Changed("IsEditable", _isEditable, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public bool AllowDuplicates
    {
        get { return _allowDuplicates; }
        set
        {
            _allowDuplicates = value;
            this.Changed("AllowDuplicates", _allowDuplicates, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public bool IsAutomatic
    {
        get { return _isAutomatic; }
        set
        {
            _isAutomatic = value;
            this.Changed("IsAutomatic", _isAutomatic, value);
        }
    }

    [InspectorProperty, JsonProperty]
    public bool HasPredefinedOptions
    {
        get { return _hasPredefinedOptions; }
        set
        {
            _hasPredefinedOptions = value;
            this.Changed("HasPredefinedOptions", _hasPredefinedOptions, value);
        }
    }
}
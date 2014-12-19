using Invert.Core.GraphDesigner;
using Invert.Json;

public class ConnectionData : IJsonObject
{

    private string _outputIdentifier;

    private string _inputIdentifier;

    public ConnectionData(string outputIdentifier, string inputIdentifier)
    {
        OutputIdentifier = outputIdentifier;
        InputIdentifier = inputIdentifier;
    }

    public ConnectionData()
    {
    }

    public string OutputIdentifier
    {
        get { return _outputIdentifier; }
        set { _outputIdentifier = value; }
    }

    public string InputIdentifier
    {
        get { return _inputIdentifier; }
        set { _inputIdentifier = value; }
    }

    public IGraphData Graph { get; set; }
    public IGraphItem Output { get; set; }
    public IGraphItem Input { get; set; }

    public void Serialize(JSONClass cls)
    {
        cls.Add("OutputIdentifier", OutputIdentifier ?? string.Empty);
        cls.Add("InputIdentifier", InputIdentifier ?? string.Empty);
    }

    public void Deserialize(JSONClass cls, INodeRepository repository)
    {
        if (cls["InputIdentifier"] != null)
        {
            InputIdentifier = cls["InputIdentifier"].Value;
        }
        if (cls["OutputIdentifier"] != null)
        {
            OutputIdentifier = cls["OutputIdentifier"].Value;
        }
    }
}
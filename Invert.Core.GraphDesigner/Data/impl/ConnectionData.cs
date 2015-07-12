using System.Collections.Generic;
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
    public IConnectable Output { get; set; }
    public IConnectable Input { get; set; }

    public void Remove()
    {
        Graph.RemoveConnection(Output,Input);
    }

    public void Serialize(JSONClass cls)
    {
        cls.Add("OutputIdentifier", OutputIdentifier ?? string.Empty);
        cls.Add("InputIdentifier", InputIdentifier ?? string.Empty);
    }

    public void Deserialize(JSONClass cls)
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

public interface IConnectionDataProvider
{
    IEnumerable<ConnectionData> Connections { get; }
}
using System.Collections.Generic;
using UnityEngine;

public class uFrameProject : ScriptableObject, INodeRepository
{
    [SerializeField]
    private string _name;
    [SerializeField]
    private JsonElementDesignerData[] _nodeRepositories;
    [SerializeField]
    private string _outputDirectory;

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public IEnumerable<IDiagramNode> NodeItems
    {
        get
        {
            foreach (var item in _nodeRepositories)
            {
                foreach (var node in item.NodeItems)
                {
                    yield return node;
                }
            }
        }
    }

    public ElementDiagramSettings Settings { get; private set; }


    public void AddNode(IDiagramNode data)
    {

    }

    public void RemoveNode(IDiagramNode enumData)
    {

    }

    public IDiagramFilter CurrentFilter { get; private set; }

    public JsonElementDesignerData[] NodeRepositories
    {
        get { return _nodeRepositories; }
        set { _nodeRepositories = value; }
    }

    public string OutputDirectory
    {
        get { return _outputDirectory; }
        set { _outputDirectory = value; }
    }
}
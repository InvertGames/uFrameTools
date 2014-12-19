namespace Invert.Core.GraphDesigner.Data.Upgrading
{
    public interface IUpgradeProcessor
    {
        int Version { get; }

        void Upgrade(INodeRepository repository, IGraphData graphData);
    }

  
}
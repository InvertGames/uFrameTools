using System.Collections;
using System.Linq;
using Invert.Core.GraphDesigner;

public class PluginGraphData : GenericGraphData<ShellPluginNode>
{
    public override void Document(IDocumentationBuilder docs)
    {
        //base.Document(docs);
        docs.BeginArea("TOC");
        foreach (var config in NodeItems.OfType<ShellNodeConfig>().Where(p => p.IsGraphType))
        {
            DocumentNodes(docs, config);
        }
        docs.EndArea();
        docs.BeginArea("DOCS");
        foreach (var config in NodeItems.OfType<DiagramNode>())
        {
            docs.BeginArea("NODE");
            config.Document(docs);
            docs.BeginArea("NODERELATEDCONTENT");
            var config1 = config;
            docs.Columns(() => { docs.Title3("Related: "); }, () =>
            {
                foreach (var item in config1.Inputs.Select(p=>p.Output).OfType<IDiagramNodeItem>())
                {
                    var item1 = item;
                    docs.Columns(()=>{docs.LinkToNode(item1.Node);});
                }
            });
            docs.EndArea();
            docs.EndArea();
        }
        docs.EndArea();

        //docs.Section(config.Name + " Graph");
        //config.Document(docs);
        //foreach (var item in config.SubNodes)
        //{
        //    config.Document(item);
        //}

    }

    private static void DocumentNodes(IDocumentationBuilder docs, ShellNodeConfig config)
    {
        docs.LinkToNode(config.Node, config.Name);
        docs.PushIndent();
        foreach (var item in config.SubNodes)
        {
            var item1 = item;
            docs.Rows(() => DocumentNodes(docs, item1));
            docs.PushIndent();
            foreach (var child in item.ChildItems)
            {
                var child1 = child;

                docs.Rows(() => { docs.LinkToNode(child1, child1.Name + " Items"); });
            }
            docs.PopIndent();
        }
        docs.PopIndent();
    }
}

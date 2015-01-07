using System;
using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public abstract class DesignerGeneratorFactory
    {
        public abstract Type DiagramItemType
        {
            get;
        }

        [Inject]
        public IUFrameContainer Container { get; set; }

        [Inject]
        public INodeRepository Repository { get; set; }

        public object ObjectData { get; set; }

        public abstract IEnumerable<OutputGenerator> GetGenerators(GeneratorSettings settings,ICodePathStrategy pathStrategy, INodeRepository diagramData, object node);
    }

    public abstract class DesignerGeneratorFactory<TData> : DesignerGeneratorFactory where TData : class
    {
        public override Type DiagramItemType
        {
            get { return typeof(TData); }
        }

        public sealed override IEnumerable<OutputGenerator> GetGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData, object node)
        {
            return CreateGenerators(settings, pathStrategy, diagramData, node as TData);
        }
        public abstract IEnumerable<OutputGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData, TData item);

    }

    public class GraphGeneratorFactory : DesignerGeneratorFactory<IGraphData> 
    {
        public Func<IGraphData, OutputGenerator> GetGenerator { get; set; }

        public GraphGeneratorFactory(Func<IGraphData, OutputGenerator> getGenerator)
        {
            GetGenerator = getGenerator;
        }

        public override IEnumerable<OutputGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
            IGraphData item)
        {
            yield return GetGenerator(item);
        }

        public bool AlwaysRegenerate { get; set; }
    }
}
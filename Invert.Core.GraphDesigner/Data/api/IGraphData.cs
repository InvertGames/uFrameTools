using System;
using System.Collections.Generic;
using Invert.Json;

namespace Invert.Core.GraphDesigner
{
    public interface IGraphData : IElementFileData,IItem
    {
        List<IChangeData> ChangeData { get; set; }
        string Identifier { get; set; }

        int RefactorCount { get; set; }
    
        string Version { get; set; }

        // Not Persisted
        FilterState FilterState { get; set; }

        // Filters
        IDiagramFilter RootFilter { get; set; }
        ICodePathStrategy CodePathStrategy { get; set; }
        bool Errors { get; set; }
        Exception Error { get; set; }
        string Path { get; set; }
        IProjectRepository Project { get; set; }
        bool Precompiled { get; set; }
        bool DocumentationMode { get; set; }
  
        //IEnumerable<ConnectionData> Connections { get; }
        void AddConnection(IConnectable output, IConnectable input);
        void AddConnection(string output, string input);
        void RemoveConnection(IConnectable output, IConnectable input);
        void ClearOutput(IConnectable output);
        void ClearInput(IConnectable input); 
        void SetProject(IProjectRepository value);
        void DeserializeFromJson(JSONNode graphData);
        IDiagramFilter CreateDefaultFilter();
        JSONNode Serialize();
        void Deserialize(string jsonData);
        void CleanUpDuplicates();
        List<ErrorInfo> Validate();
        void TrackChange(IChangeData data);
    }
}
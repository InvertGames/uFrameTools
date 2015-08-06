using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Invert.Data;
using Invert.Json;

namespace Invert.Core.GraphDesigner
{
    public interface IGraphData : IElementFileData,IItem, IDataRecord
    {
        List<IChangeData> ChangeData { get; set; }
        string Identifier { get; set; }

        int RefactorCount { get; set; }
    
        string Version { get; set; }

        // Filters
        IDiagramFilter RootFilter { get; set; }

        bool Errors { get; set; }
        Exception Error { get; set; }

        bool Precompiled { get; set; }

        string Directory { get;  }
        IDiagramFilter[] FilterStack { get; set; }


        //IEnumerable<ConnectionData> Connections { get; }
        void AddConnection(IConnectable output, IConnectable input);
        void AddConnection(string output, string input);
        void RemoveConnection(IConnectable output, IConnectable input);
        void ClearOutput(IConnectable output);
        void ClearInput(IConnectable input); 
       // void SetProject(IProjectRepository value);
        void DeserializeFromJson(JSONNode graphData);
        IDiagramFilter CreateDefaultFilter();
        JSONNode Serialize();
        void Deserialize(string jsonData);
        void CleanUpDuplicates();
     
        void TrackChange(IChangeData data);
        void PushFilter(IDiagramFilter filter);
        void PopToFilter(IDiagramFilter filter1);
        void PopToFilterById( string filterId);
        void PopFilter();
    }

    public interface IGraphConfiguration : IItem
    {
        string CodeOutputSystemPath { get;  }
        string Namespace { get; set; }
        bool IsCurrent { get; set; }
        string FullPath { get;  }
    }

    public class GraphConfiguration : IGraphConfiguration
    {
        public GraphConfiguration(string codeOutputSystemPath, string ns)
        {
            CodeOutputSystemPath = codeOutputSystemPath;
            Namespace = ns;
            IsCurrent = true;
        }

        public string CodeOutputSystemPath { get; set; }
        public string Namespace { get; set; }
        public bool IsCurrent { get; set; }
        public string FullPath { get; set; }
        public string Title { get; set; }
        public string Group { get;  set; }
        public string SearchTag { get;  set; }
    }
}
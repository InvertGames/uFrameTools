using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Invert.uFrame.VS
{
    public class VisualStudioProjectRepository : DefaultProjectRepository, IVsHierarchyEvents, IDisposable
    {
        public IVsProject Project { get; set; }
        public IVsHierarchy ProjectHierarchy { get; set; }
        private List<IGraphData> _loadedGraphs;
        private IGraphData _currentGraph1;
        private Action _projectEventsDisposer;

        public VisualStudioProjectRepository(IVsProject project)
        {
            Project = project;
            ProjectHierarchy = project as IVsHierarchy;
            uint eventsSubscriptionCookie;
 
            Debug.Assert(ProjectHierarchy != null, "ProjectHierarchy != null");
      
            ProjectHierarchy.AdviseHierarchyEvents(this, out eventsSubscriptionCookie);
            _projectEventsDisposer = delegate { ProjectHierarchy.UnadviseHierarchyEvents(eventsSubscriptionCookie); };
            Refresh();
        }

        public string ProjectName
        {
            get
            {
                object name;
                ProjectHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectName, out name);
                return (string) name;
            }
        }

        public override string Namespace
        {
            get { return ProjectHierarchy.GetProjectNamespace(); }
        }

        public override string Name
        {
            get { return ProjectHierarchy.GetProjectName(); }
        }

        

        protected List<IGraphData> LoadedGraphs
        {
            get { return _loadedGraphs ?? (_loadedGraphs = new List<IGraphData>()); }
            set { _loadedGraphs = value; }
        }

        public override IGraphData CurrentGraph
        {
            get { return _currentGraph1; }
            set { _currentGraph1 = value; }
        }

        public override IEnumerable<IGraphData> Graphs
        {
            get { return LoadedGraphs; }
            set
            {
                
            }
        }

        public override IGraphData CreateNewDiagram(Type diagramType, IDiagramFilter defaultFilter = null)
        {
            return null;
        }

        public override void RecordUndo(INodeRepository data, string title)
        {

        }

        public sealed override void Refresh()
        {
           // ProjectUtilities.GetProjectByFileName("asdf").AddItem(,VSADDITEMOPERATION.VSADDITEMOP_OPENFILE, )
            LoadedGraphs = new List<IGraphData>();
            foreach (var item in ProjectUtilities.GetProjectFiles(Project,Project as IVsHierarchy))
            {
                var graph = LoadGraph(item);
                if (graph != null)
                {
                    LoadedGraphs.Add(graph);
                    CurrentGraph = graph;
                    graph.SetProject(this);
                }
                
                //_currentGraph1 = instance;
                //_lastLoadedDiagram = System.IO.Path.GetFileNameWithoutExtension(filename);
            }
        }

        public static InvertGraph LoadGraph(string item)
        {
            if (!item.EndsWith(".ufgraph")) return null;
            if (!File.Exists(item)) return null;
            try
            {
                var jsonText = File.ReadAllText(item);
                InvertGraph instance;
                if (!LoadGraphFromString(item, jsonText, out instance)) return null;

                return instance;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Skipping {0} beause {1}", item, ex.Message);
            }
            return null;
        }

        public static bool LoadGraphFromString(string item, string jsonText, out InvertGraph instance)
        {
            instance = null;
            var graphJson = JSON.Parse(jsonText);
            if (graphJson["Type"] == null) return false;
            var type = InvertApplication.FindType(graphJson["Type"]);

            instance = Activator.CreateInstance(type) as InvertGraph;
            instance.DeserializeFromJson(graphJson);
            instance.Path = item;
            instance.CodePathStrategy = new DefaultCodePathStrategy()
            {
                Data = instance,
                AssetPath = System.IO.Path.GetDirectoryName(item),

            };
            return true;
        }

        public override void SaveDiagram(INodeRepository data)
        {

        }

        public override string LastLoadedDiagram { get; set; }


        public int OnItemAdded(uint itemidParent, uint itemidSiblingPrev, uint itemidAdded)
        {
            var itemFilename = ProjectHierarchy.GetProperty<string>(__VSHPROPID.VSHPROPID_SaveName, itemidAdded);
            if (itemFilename == null) return VSConstants.S_OK;
            
            if (itemFilename.EndsWith(".ufgraph"))
            Refresh();
            return VSConstants.S_OK;
        }

        public int OnItemsAppended(uint itemidParent)
        {
            return VSConstants.S_OK;
        }

        public int OnItemDeleted(uint itemid)
        {
            var itemFilename = ProjectHierarchy.GetProperty<string>(__VSHPROPID.VSHPROPID_SaveName, itemid);
            if (itemFilename == null) return VSConstants.S_OK;
            if (itemFilename.EndsWith(".ufgraph"))
            Refresh();
            return VSConstants.S_OK;
        }

        public int OnPropertyChanged(uint itemid, int propid, uint flags)
        {
            if (propid != (int)__VSHPROPID.VSHPROPID_SaveName) return VSConstants.S_OK;
            var itemFilename = ProjectHierarchy.GetProperty<string>(__VSHPROPID.VSHPROPID_SaveName, itemid);
            if (itemFilename.EndsWith(".ufgraph"))
            {
                Refresh();
            }
            return VSConstants.S_OK;
        }

        public int OnInvalidateItems(uint itemidParent)
        {
            return VSConstants.S_OK;
        }

        public int OnInvalidateIcon(IntPtr hicon)
        {
            return VSConstants.S_OK;
        }

        public void Dispose()
        {
            _projectEventsDisposer();
        }
    }
}
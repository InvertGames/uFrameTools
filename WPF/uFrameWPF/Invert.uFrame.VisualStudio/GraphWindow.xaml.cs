using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Invert.Core.GraphDesigner;
using Invert.IOC;
using Invert.Platform.Gwen;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using UserControl = System.Windows.Controls.UserControl;

namespace Invert.uFrame.VisualStudio
{
    /// <summary>
    /// Interaction logic for GraphWindow.xaml
    /// </summary>
    public partial class GraphWindow : UserControl
    {
        [Inject]
        public IUFrameContainer Container { get; set; }

        public IVsUIShell Shell { get; set; }
        public ITrackSelection Track { get; set; }
        public GraphWindow()
        {
            InitializeComponent(); 
            ControlHost.Child = GraphControl = new uFrameGraphControl()
            {
                Dock = DockStyle.Fill
            };
            
           // this.Diagram.SelectionChanged = ShowSelection; TODO

        }

        public uFrameGraphControl GraphControl { get; set; }


        private IVsWindowFrame frame = null;
        private SelectionContainer mySelContainer;


        private void ShowSelection()
        {
            GraphItemViewModel selected = DiagramViewModel.SelectedNodeItem;
            if (selected == null)
            {
                selected = DiagramViewModel.SelectedNode;
            }
            if (selected != null)
            {
                //Shell = Pane.GetSiteService(typeof (SVsUIShell)) as IVsUIShell;
                //if (Shell != null)
                //{
                //    var guidPropertyBrowser = new
                //        Guid(ToolWindowGuids.PropertyBrowser);
                //    Shell.FindToolWindow((uint) __VSFINDTOOLWIN.FTW_fForceCreate,
                //        ref guidPropertyBrowser, out frame);
                //}
                ////if (frame != null)
                ////{
                ////    frame.Show();
                ////}
                //if (mySelContainer == null)
                //{
                //    mySelContainer = new SelectionContainer();
                //}

                //mySelContainer.SelectedObjects = new List<object> {selected.DataObject};

                //Track = Pane.GetSiteService(typeof (STrackSelection)) as ITrackSelection;
                //if (Track != null)
                //{
                    
                //    Track.OnSelectChange(mySelContainer);
                //}
            }
        }

        //public EditorPane Pane { get; set; }
        public bool IsDirty { get; set; }
        public string Filename { get; set; }
        public void OpenGraph(string filename)
        {
            GraphControl.Filename = filename;
            Filename = filename;
            IsDirty = false;
            //InvertGraphEditor.CurrentProject = new SingleFileProjectRepository(filename);
           // LoadFile();
        }
        public IProjectRepository Project { get; set; }
        public IGraphData Graph { get; set; }

        //private void LoadFile()
        //{
        //    var service = InvertGraphEditor.Container.Resolve<ProjectService>();
        //    Project = service.Projects.First(x => x.Graphs.Any(p => p.Path == Filename));

        //    Graph = Project.Graphs.FirstOrDefault(p => p.Path == Filename);
        //    if (Graph != null)
        //    {
        //        Graph.DeserializeFromJson(JSON.Parse(File.ReadAllText(Filename)));
        //        DataContext = new DiagramViewModel(Graph, Graph);
        //    }
        //   // GraphControl.Filename
        //    Diagram.DataContext = DataContext;
        //}

        public IEnumerable<object> ContextObjects
        {
            get { return DiagramViewModel.ContextObjects; }
        }

        public void CommandExecuted(IEditorCommand command)
        {
            //var vm = DiagramViewModel;
            //DataContext = null;
            //DataContext = vm;
            //this.Diagram.DataContext = null;
            //this.Diagram.DataContext = DiagramViewModel;
            //DiagramViewModel.CommandExecuted(command);
            //Diagram.InvalidateArrange();
            //Diagram.InvalidateMeasure();
            //Diagram.InvalidateVisual();
            IsDirty = true;
            Project.MarkDirty(Graph);
        }

        public void CommandExecuting(IEditorCommand command)
        {
            Project.RecordUndo(Graph,command.Title);
        }

        public DiagramViewModel DiagramViewModel
        {
            get { return DataContext as DiagramViewModel; }
        }

        public float Scale
        {
            get { return 1f; }
            set
            {
                
            }
        }

        //public DiagramDrawer DiagramDrawer
        //{
        //    get { return  Diagram.Drawer; }
        //    set { Diagram.Drawer = value; }
        //}

        public void RefreshContent()
        {
            
        }

        public void ProjectChanged(IProjectRepository project)
        {
            
        }

        public void Save()
        {
            GraphControl.Save();

            //Project.Save();
            IsDirty = false;
        }
    }

    
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Microsoft.Samples.VisualStudio.IDE.EditorWithToolbox;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Invert.uFrame.VS
{
    /// <summary>
    /// Interaction logic for GraphWindow.xaml
    /// </summary>
    public partial class GraphWindow : UserControl, IGraphWindow
    {
        [Inject]
        public IUFrameContainer Container { get; set; }

        public IVsUIShell Shell { get; set; }
        public ITrackSelection Track { get; set; }
        public GraphWindow()
        {
            InitializeComponent();
            InvertGraphEditor.DesignerWindow = this;
            this.Diagram.SelectionChanged = ShowSelection;

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            VSWindows.Dispatcher = this.Dispatcher;
            base.OnRender(drawingContext);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
         
            //if (!string.IsNullOrEmpty(Filename))
            //{
            //    LoadFile();
            //}
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            InvertGraphEditor.DesignerWindow = this;
        }
        private IVsWindowFrame frame = null;
        private SelectionContainer mySelContainer;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            InvertGraphEditor.DesignerWindow = this;
            ShowSelection();
        }

        private void ShowSelection()
        {
            var selected = DiagramViewModel.SelectedGraphItem;
            if (selected != null)
            {
                Shell = Pane.GetSiteService(typeof (SVsUIShell)) as IVsUIShell;
                if (Shell != null)
                {
                    var guidPropertyBrowser = new
                        Guid(ToolWindowGuids.PropertyBrowser);
                    Shell.FindToolWindow((uint) __VSFINDTOOLWIN.FTW_fForceCreate,
                        ref guidPropertyBrowser, out frame);
                }
                if (frame != null)
                {
                    frame.Show();
                }
                if (mySelContainer == null)
                {
                    mySelContainer = new SelectionContainer();
                }

                mySelContainer.SelectedObjects = new List<object>() {selected.DataObject};

                Track = Pane.GetSiteService(typeof (STrackSelection)) as ITrackSelection;
                if (Track != null)
                {
                    
                    Track.OnSelectChange(mySelContainer);
                }
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            InvertGraphEditor.DesignerWindow = this;
        }

        public EditorPane Pane { get; set; }
        public bool IsDirty { get; set; }
        public string Filename { get; set; }
        public void OpenGraph(string filename)
        {
            Filename = filename;
            IsDirty = false;
            //InvertGraphEditor.CurrentProject = new SingleFileProjectRepository(filename);
            LoadFile();
        }
        public IProjectRepository Project { get; set; }
        public IGraphData Graph { get; set; }

        private void LoadFile()
        {
            Project = InvertGraphEditor.Projects.First(x => x.Graphs.Any(p => p.Path == Filename));
            Graph = Project.Graphs.FirstOrDefault(p => p.Path == Filename);
            DataContext = new DiagramViewModel(Graph, Graph);
            Diagram.DataContext = DataContext;
        }

        public IEnumerable<object> ContextObjects
        {
            get { return DiagramViewModel.ContextObjects; }
        }

        public void CommandExecuted(IEditorCommand command)
        {
            var vm = DiagramViewModel;
            DataContext = null;
            DataContext = vm;
            this.Diagram.DataContext = null;
            this.Diagram.DataContext = DiagramViewModel;
            DiagramViewModel.CommandExecuted(command);
            Diagram.InvalidateArrange();
            Diagram.InvalidateMeasure();
            Diagram.InvalidateVisual();
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

        public void RefreshContent()
        {
            
        }

        public void Save()
        {
            Project.Save();
            IsDirty = false;
        }
    }

    
}

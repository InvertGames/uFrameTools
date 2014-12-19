using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DiagramDesigner.Platform;
using Invert.Core.GraphDesigner;

namespace DiagramDesigner
{
    public partial class Window1 : Window, IGraphWindow
    {

        public MainWindowUIViewModel ViewModel
        {
            get { return DataContext as MainWindowUIViewModel; }
        }

        public Window1()
        {
            
            InitializeComponent();
            InvertGraphEditor.DesignerWindow = this;
            DataContext = new MainWindowUIViewModel();
            ViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                DataContext = null;
                DataContext = sender;     
                this.MyDesigner.DataContext = null;
                this.MyDesigner.DataContext = ViewModel.CurrentDiagram;
                
            };
            
       
        }

  
        public IEnumerable<object> ContextObjects
        {
            get
            {
                return ViewModel.ContextObjects;
            }
        }
        
        public void CommandExecuted(IEditorCommand command)
        {
            var vm = ViewModel;
            DataContext = null;
            DataContext = vm;
            this.MyDesigner.DataContext = null;
            this.MyDesigner.DataContext = ViewModel.CurrentDiagram;
            ViewModel.CommandExecuted(command);
            MyDesigner.InvalidateArrange();
            MyDesigner.InvalidateMeasure();
            MyDesigner.InvalidateVisual();
            ProjectGraphsList.ItemsSource = null;
            ProjectGraphsList.ItemsSource = ViewModel.ProjectGraphs;

        }

        public void CommandExecuting(IEditorCommand command)
        {
            
        }

        public DiagramViewModel DiagramViewModel
        {
            get { return ViewModel.CurrentDiagram; }
        }

        public float Scale
        {
            get { return 1.0f; }
            set
            {
                
            }
        }

        public void RefreshContent()
        {
            ProjectGraphsList.ItemsSource = ViewModel.ProjectGraphs;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void CreateProject_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateProject();
        }

        private void CreateGraph_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateGraph();
            CommandExecuted(null);
        }

        private void SaveAll_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveProject();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           // var sender = itemGraph = ProjectGraphViewModel
            if (e.AddedItems.Count > 0)
            {
                var graphItem = e.AddedItems[0] as ProjectGraphViewModel;
                graphItem.ShowCommand.Execute(DiagramViewModel);
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
   
        }
    }
}

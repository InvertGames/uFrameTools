using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DiagramDesigner.Platform;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.GraphDesigner.Documentation;
using UnityEngine;

namespace DiagramDesigner
{
    public partial class Window1 : Window, IGraphWindow, IDebugLogger
    {

        public MainWindowUIViewModel ViewModel
        {
            get { return DataContext as MainWindowUIViewModel; }
        }

        public Window1()
        {
            InvertApplication.Logger = this;
            InvertApplication.CachedAssemblies.Add(typeof(ICollection<>).Assembly);
            InvertApplication.CachedAssemblies.Add(typeof(DocumentationPlugin).Assembly);
            //foreach (var assembly in Assembly.GetEntryAssembly().GetReferencedAssemblies())
            //{
            //    Debug.WriteLine("------ LOADED: " + assembly.FullName);
            //    AppDomain.CurrentDomain.Load(assembly);
            //}
            InitializeComponent();
            InvertGraphEditor.DesignerWindow = this;
            String[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index < arguments.Length; index++)
            {
                var argument = arguments[index];
                InvertApplication.Log(argument);
            }
            DataContext = new MainWindowUIViewModel();
            ViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                DataContext = null;
                DataContext = sender;
                this.MyDesigner.DataContext = null;
                this.MyDesigner.DataContext = ViewModel.CurrentDiagram;

            };
            foreach (var graphType in InvertGraphEditor.Container.Mappings.Where(p => p.From == typeof(IGraphData)))
            {
                TypeMapping type = graphType;
                var menuItem = new MenuItem()
                {
                    Header = graphType.Name,
                    Command = new SimpleEditorCommand<MainWindowUIViewModel>((d) =>
                    {
                        var diagram = d.CurrentProject.CreateNewDiagram(type.To, null);
                        d.CurrentProject.CurrentGraph = diagram;
                        d.LoadDiagram(diagram);
                        CommandExecuted(null);
                    })

                };
                CreateMenuItem.Items.Add(menuItem);
            }

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

        private void Button_Click_1(object s, RoutedEventArgs e)
        {

            var docs = DiagramViewModel.CurrentRepository;
            var sb = new StringBuilder();
            foreach (var generator in InvertGraphEditor.GetAllCodeGenerators(null, docs, false))
            {
                if (generator is RazorOutputGenerator)
                {
                    sb.Append(generator.ToString());
                }

            }
            DocsBrowser.NavigateToString(sb.ToString());
            //DiagramViewModel.ViewModelToImage(DiagramViewModel.DiagramBounds.size).ToFile(DiagramViewModel.Title + ".png");

        }

        public void Log(string message)
        {
            DebugText.AppendText(message);
            DebugText.AppendText(Environment.NewLine);
        }

        public void LogException(Exception ex)
        {
            DebugText.AppendText(ex.Message);
            DebugText.AppendText(ex.StackTrace);
            DebugText.AppendText(Environment.NewLine);
        }
    }
}

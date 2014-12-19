using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Invert.Core.GraphDesigner;

namespace Invert.GraphDesigner.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IGraphWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Scale = 1f;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        public IEnumerable<object> ContextObjects { get; private set; }
        public void CommandExecuted(IEditorCommand command)
        {
            
        }

        public void CommandExecuting(IEditorCommand command)
        {
            
        }

        public DiagramViewModel DiagramViewModel { get; private set; }
        
        public float Scale { get; set; }

        public void RefreshContent()
        {
            
        }
    }
}

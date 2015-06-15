using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gwen;
using Gwen.Control;
using Gwen.Control.Layout;
using Invert.Core.GraphDesigner;
using Invert.Json;


namespace Invert.Platform.Gwen
{
    public class GraphApplication : DockBase
    {   
       // private Control.Base m_LastControl;
        private readonly global::Gwen.Control.StatusBar m_StatusBar;
        private readonly global::Gwen.Control.ListBox m_TextOutput;
       // private Control.TabButton m_Button;
        private readonly global::Gwen.Control.CollapsibleList m_List;
        private readonly Center m_Center;
      //  private readonly Control.LabeledCheckBox m_DebugCheck;

        public double Fps; // set this in your rendering loop
        public string Note; // additional text to display in status bar
        private GraphWindowControl _graphWindowControl;
        private InspectorControl inspectorControl;
        private ScrollControl scrollControl;

        public GraphApplication(Base parent)
            : base(parent)
        {
            Dock = Pos.Fill;
            SetSize(1024, 768);
           // m_List = new Gwen.Control.CollapsibleList(this);
            inspectorControl = new InspectorControl(this);
            inspectorControl.Dock = Pos.Right;
            RightDock.TabControl.AddPage("Inspector", inspectorControl);
            //RightDock.TabControl.AddPage("Inspector");
            RightDock.Width = 150;

           
            scrollControl = new ScrollControl(this);
            scrollControl.Dock = Pos.Fill;
            GraphWindowControl = new GraphWindowControl(scrollControl.InnerPanel);
            scrollControl.SetInnerSize(5000,5000);
            GraphWindowControl.ScrollContainer = scrollControl;
            // m_TextOutput = new Control.ListBox(BottomDock);
            //m_Button = BottomDock.TabControl.AddPage("Output", m_TextOutput);
            //BottomDock.Height = 200;

            //m_DebugCheck = new Control.LabeledCheckBox(m_List);
            //m_DebugCheck.Text = "Debug outlines";
            //m_DebugCheck.CheckChanged += DebugCheckChanged;

            //m_StatusBar = new Gwen.Control.StatusBar(this);
            // m_StatusBar.Dock = Pos.Bottom;
            // m_StatusBar.AddControl(m_DebugCheck, true);

            //m_Center = new Center(this);
            //m_Center.Dock = Pos.Fill;
            //GUnit test;

            //{
            //    CollapsibleCategory cat = m_List.Add("Non-Interactive");
            //    {
            //        test = new Label(m_Center);
            //        RegisterUnitTest("Label", cat, test);
            //        test = new RichLabel(m_Center);
            //        RegisterUnitTest("RichLabel", cat, test);
            //        test = new GroupBox(m_Center);
            //        RegisterUnitTest("GroupBox", cat, test);
            //        test = new ProgressBar(m_Center);
            //        RegisterUnitTest("ProgressBar", cat, test);
            //        test = new ImagePanel(m_Center);
            //        RegisterUnitTest("ImagePanel", cat, test);
            //        test = new StatusBar(m_Center);
            //        RegisterUnitTest("StatusBar", cat, test);
            //    }
            //}

            //{
            //    CollapsibleCategory cat = m_List.Add("Standard");
            //    {
            //        test = new Button(m_Center);
            //        RegisterUnitTest("Button", cat, test);
            //        test = new TextBox(m_Center);
            //        RegisterUnitTest("TextBox", cat, test);
            //        test = new CheckBox(m_Center);
            //        RegisterUnitTest("CheckBox", cat, test);
            //        test = new RadioButton(m_Center);
            //        RegisterUnitTest("RadioButton", cat, test);
            //        test = new ComboBox(m_Center);
            //        RegisterUnitTest("ComboBox", cat, test);
            //        test = new ListBox(m_Center);
            //        RegisterUnitTest("ListBox", cat, test);
            //        test = new NumericUpDown(m_Center);
            //        RegisterUnitTest("NumericUpDown", cat, test);
            //        test = new Slider(m_Center);
            //        RegisterUnitTest("Slider", cat, test);
            //        test = new Menu(m_Center);
            //        RegisterUnitTest("Menu", cat, test);
            //        test = new CrossSplitter(m_Center);
            //        RegisterUnitTest("CrossSplitter", cat, test);
            //    }
            //}

            //{
            //    CollapsibleCategory cat = m_List.Add("Containers");
            //    {
            //        test = new Window(m_Center);
            //        RegisterUnitTest("Window", cat, test);
            //        test = new TreeControl(m_Center);
            //        RegisterUnitTest("TreeControl", cat, test);
            //        test = new Properties(m_Center);
            //        RegisterUnitTest("Properties", cat, test);
            //        test = new TabControl(m_Center);
            //        RegisterUnitTest("TabControl", cat, test);
            //        test = new ScrollControl(m_Center);
            //        RegisterUnitTest("ScrollControl", cat, test);
            //        test = new Docking(m_Center);
            //        RegisterUnitTest("Docking", cat, test);
            //    }
            //}

            //{
            //    CollapsibleCategory cat = m_List.Add("Non-standard");
            //    {
            //        test = new CollapsibleList(m_Center);
            //        RegisterUnitTest("CollapsibleList", cat, test);
            //        test = new ColorPickers(m_Center);
            //        RegisterUnitTest("Color pickers", cat, test);
            //    }
            //}

            // m_StatusBar.SendToBack();

        }

        public GraphWindowControl GraphWindowControl
        {
            get { return _graphWindowControl; }
            set { _graphWindowControl = value; }
        }
        public string Filename { get; set; }
        public void OpenGraph(string filename)
        {
            Filename = filename;
            //IsDirty = false;
            //InvertGraphEditor.CurrentProject = new SingleFileProjectRepository(filename);
            LoadFile();
        }
        public IProjectRepository Project { get; set; }
        public IGraphData Graph { get; set; }

        public void LoadFile()
        {
            var service = InvertGraphEditor.Container.Resolve<ProjectService>();
            Project = service.Projects.First(x => x.Graphs.Any(p => p.Path == Filename));

            Graph = Project.Graphs.FirstOrDefault(p => p.Path == Filename);
            if (Graph != null)
            {
                Graph.DeserializeFromJson(JSON.Parse(File.ReadAllText(Filename)));
                GraphWindowControl.DesignerWindow.LoadDiagram(Graph);
            }
      
        }

        public DiagramViewModel GraphViewModel { get; set; }
    }
}

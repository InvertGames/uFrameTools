using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Invert.GraphDesigner.Standalone
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.Controls.Add(TheGraphControl = new uFrameGraphControl()
            {
                Dock = DockStyle.Fill
            });
            this.Closed += (sender, args) =>
            {
                TheGraphControl.Destroy();
            };
        }

        public uFrameGraphControl TheGraphControl { get; set; }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}

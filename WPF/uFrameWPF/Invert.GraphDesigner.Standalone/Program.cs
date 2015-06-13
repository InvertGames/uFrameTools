using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Invert.GraphDesigner.Standalone
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DViewer
{
    public partial class Form1 : Form
    {
        FormRobot_multiObj frmRobot = new FormRobot_multiObj();
        
        public Form1()
        {
            InitializeComponent();

            frmRobot.MdiParent = this;

            frmRobot.Show();
        }

        private void test01ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //frmRobot.MdiParent = this;
            frmRobot.Show();
            frmRobot.StartRendering();
        }
    }
}

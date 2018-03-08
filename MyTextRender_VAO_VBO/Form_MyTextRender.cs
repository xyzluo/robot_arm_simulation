using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyTextRender_VAO_VBO
{
    public partial class Form_MyTextRender : Form
    {
        GeneralRender myRender;
        public Form_MyTextRender()
        {
            InitializeComponent();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            myRender = GeneralRender.Instance(glControl1);

        }
    }
}

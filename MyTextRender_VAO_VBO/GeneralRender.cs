using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace MyTextRender_VAO_VBO
{
    class GeneralRender
    {


        public static GeneralRender Instance(object host)
        {
            if (null == instance)
            {
                instance = new GeneralRender(host);
            }
            return instance;
        }

        public void StartRend()
        {

        }

        public void StopRend()
        {

        }

        protected void GlControl_Paint(object sender, PaintEventArgs e)
        {
            glCrl.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //GL.UseProgram(shaderProgramHandle);
            textRenderer.Clear(Color.Transparent);//move to draw() ?
            PointF position = PointF.Empty;
            textRenderer.DrawString(DateTime.Now.Minute.ToString() + ":" +
                            DateTime.Now.Second.ToString() +
                            ":" +
                            DateTime.Now.Millisecond.ToString(), mono, Brushes.Yellow, position);
            textRenderer.Draw();
            //GL.UseProgram(0);
            glCrl.SwapBuffers();
        }
        protected void GLControl_Resize(object sender, EventArgs e)
        {
            OpenTK.GLControl c = sender as OpenTK.GLControl;
            if (c.ClientSize.Height == 0)
                c.ClientSize = new System.Drawing.Size(c.ClientSize.Width, 1);
            GL.Viewport(0, 0, c.ClientSize.Width, c.ClientSize.Height);
        }

        #region private members
        private static GeneralRender instance;
        private GLControl glCrl;
        private TextRenderer textRenderer;

        private GeneralRender(object host)
        {
            glCrl = host as GLControl;
            glCrl.VSync = true;
            GL.ClearColor(Color.DarkBlue);
            GL.Enable(EnableCap.DepthTest);
            Application.Idle += Application_Idle;// or += new EventHandler(Application_Idle);
            glCrl.Paint += new PaintEventHandler(GlControl_Paint);
            glCrl.Resize += new EventHandler(GLControl_Resize);

            //init()
            this.textRenderer = new TextRenderer(this.glCrl.Width, this.glCrl.Height);

            GLControl_Resize(glCrl, EventArgs.Empty);
        }



        private void Application_Idle(object sender, EventArgs e)
        {
            while (glCrl.IsIdle)
            {
                glCrl.Invalidate();//this will cause glControl1_Paint be called then Render();
            }
        }

        Font mono = new Font(FontFamily.GenericSansSerif, 16);
        #endregion
    }
}

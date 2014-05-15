#region --- Using directives ---

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK;


#endregion

namespace _3DViewer
{
    public partial class FormRobot_multiObj : Form
    {
        #region shader
        string vertexShaderSource = @"
#version 330 core

precision highp float;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;

in vec3 in_position[7];
in vec3 in_normal;
in vec2 texpos;

out vec3 normal;
out vec2 uv;

void main(void)
{
  //works only for orthogonal modelview
  normal = (modelview_matrix * vec4(in_normal, 0)).xyz;
  
  gl_Position = projection_matrix * modelview_matrix * vec4(in_position[0], 1);
  uv = texpos;
}";

        string fragmentShaderSource = @"
#version 330 core

precision highp float;

const vec3 ambient = vec3(0.1, 0.1, 0.1);
const vec3 lightVecNormalized = normalize(vec3(0.5, 0.5, 2.0));
const vec3 lightColor = vec3(0.8, 0.8, 0.8);
//out vec3 outColor;
in vec3 normal;
in vec2 uv;
out vec4 out_frag_color;

uniform sampler2D myTextureSampler;
uniform vec3 triangleColor;
uniform int selector;

void main(void)
{
  float diffuse = clamp(dot(lightVecNormalized, normalize(normal)), 0.0, 1.0);

  if(selector ==0)
    out_frag_color = texture2D(myTextureSampler,uv);
  if(selector ==1)
    out_frag_color = vec4(ambient + diffuse * lightColor * triangleColor, 1.0);

}";
        #endregion
        public const float toDegree = (float)(180 / Math.PI);
        public const float toRad = (float)(Math.PI/180);
        public const int BODYCOUNT = 7;
        public const int KINECOUNT = 7;
        public const bool FIXED_ASPECT_RATIO = false;
        private int idxStart = 0;
        private const int prtLookAt = 3;
        //static float angle = 0.0f;
        int uniform_texture, uniform_objSelector;
        int vertexShaderHandle,
            fragmentShaderHandle,
            shaderProgramHandle,
            modelviewMatrixLocation,
            projectionMatrixLocation,
            colorVectorLocation;

        RobotArm mRobot = new RobotArm();
        float[] jointVecter = new float[7];

        int[] positionVboHandle = new int[BODYCOUNT];
        int[] normalVboHandle = new int[BODYCOUNT];
        int txtPositionVboHandle, txtUVVboHandle;
        int vaoHandle_text;
        int[] vaoHandle = new int[BODYCOUNT];

        Vector3[] positionVboData;
        public Vector3[] pData
        {
            get { return positionVboData; }
            set { positionVboData = value; }
        }

        Vector3[] normalVboData;
        public Vector3[] nData
        {
            get { return normalVboData; }
            set { normalVboData = value; }
        }

        stlLoader myLoader = new stlLoader();
        MVPControls mvpCtrl = new MVPControls();

        dataObject[] kChain = tools.InitializeArray<dataObject>(KINECOUNT);

        Matrix4 projectionMatrix, modelMatrix, modelviewMatrix;

        //GUI controls
        TrackBar[] _trackBars;

        //2D text
        TextRenderer txtRenderer;
        PointF position = PointF.Empty;
        const int FONTSIZE = 16;
        Font mono = new Font(FontFamily.GenericSansSerif, FONTSIZE);

        void CreateShaders()
        {
            vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);

            GL.CompileShader(vertexShaderHandle);
            GL.CompileShader(fragmentShaderHandle);

            Debug.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle));
            Debug.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle));

            // Create program
            shaderProgramHandle = GL.CreateProgram();

            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);

            //bind attribute index with a named attribute variable, index will be used by VAO
            //so the index can be any user specified.
            //can avoid calling BindAttribLocation if use layout = <index> in shader
            GL.BindAttribLocation(shaderProgramHandle, 0, "in_position[0]");
            GL.BindAttribLocation(shaderProgramHandle, 1, "in_normal");
            GL.BindAttribLocation(shaderProgramHandle, 2, "texpos");
            //GL.BindAttribLocation(shaderProgramHandle, 3, "uv");

            GL.LinkProgram(shaderProgramHandle);
            Debug.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));
            //once using this program , text rendering not work 
            GL.UseProgram(shaderProgramHandle);

            // get uniforms location
            projectionMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "projection_matrix");
            modelviewMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "modelview_matrix");
            colorVectorLocation = GL.GetUniformLocation(shaderProgramHandle, "triangleColor");
            uniform_texture = GL.GetUniformLocation(shaderProgramHandle, "myTextureSampler");//1
            uniform_objSelector = GL.GetUniformLocation(shaderProgramHandle, "selector");//3
            
            
            Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2, aspectRatio(), 1, 4000, out projectionMatrix);

            //
            //modelviewMatrix = Matrix4.LookAt(new Vector3(250, -250, -600), new Vector3(193, -228, -565), new Vector3(0, 1, 0));
            //modelviewMatrix = Matrix4.LookAt(new Vector3(00.0f, 00.0f, 1200.0f), kChain[prtLookAt].Center, new Vector3(0, 0, 1));
            //Vector3 eye = kChain[prtLookAt].Center;
            //eye.Y += 2000;
            //modelviewMatrix = Matrix4.LookAt(eye, kChain[prtLookAt].Center, new Vector3(0, 0, 1));
            mvpCtrl.TargetPosition = kChain[prtLookAt].Center;
            mvpCtrl.CameraDistance = 2000;
            mvpCtrl.HorizontalAngle = 0;
            mvpCtrl.VerticalAngle = 90;
            mvpCtrl.computeMatricesFromInputs();

            //modelviewMatrix = mvpCtrl.ViewMatrix;
            //GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);
            //GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);
        }

        void CreateVBOs()
        {
            // text buffer
            txtPositionVboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, txtPositionVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(txtRenderer.txtData.Length * Vector3.SizeInBytes),
                txtRenderer.txtData, BufferUsageHint.StaticDraw);

            txtUVVboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, txtUVVboHandle);
            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer,
                new IntPtr(txtRenderer.uvData.Length * Vector2.SizeInBytes),
                txtRenderer.uvData, BufferUsageHint.StaticDraw);

            //part 0 - 6
            for (int i = idxStart; i < BODYCOUNT; i++)
            {
                positionVboData = kChain[i].Vertex;
                normalVboData = kChain[i].Normal;

                positionVboHandle[i] = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, positionVboHandle[i]);
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                    new IntPtr(positionVboData.Length * Vector3.SizeInBytes),
                    positionVboData, BufferUsageHint.StaticDraw);

                normalVboHandle[i] = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, normalVboHandle[i]);
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                    new IntPtr(normalVboData.Length * Vector3.SizeInBytes),
                    normalVboData, BufferUsageHint.StaticDraw);

            }
            //clean
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        void CreateVAOs()
        {
            // GL3 allows us to store the vertex layout in a "vertex array object" (VAO).
            // This means we do not have to re-issue VertexAttribPointer calls
            // every time we try to use a different vertex layout - these calls are
            // stored in the VAO so we simply need to bind the correct VAO.

            //text
            vaoHandle_text = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle_text);

            GL.EnableVertexAttribArray(0);//see BindAttribLocation for index
            GL.BindBuffer(BufferTarget.ArrayBuffer, txtPositionVboHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);

            GL.EnableVertexAttribArray(2);//see BindAttribLocation for index
            GL.BindBuffer(BufferTarget.ArrayBuffer, txtUVVboHandle);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, true, Vector2.SizeInBytes, 0);

            for (int i = idxStart; i < BODYCOUNT; i++)
            {
                vaoHandle[i] = GL.GenVertexArray();
                GL.BindVertexArray(vaoHandle[i]);

                GL.EnableVertexAttribArray(0);//see BindAttribLocation for index
                GL.BindBuffer(BufferTarget.ArrayBuffer, positionVboHandle[i]);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);

                GL.EnableVertexAttribArray(1);//see BindAttribLocation for index
                GL.BindBuffer(BufferTarget.ArrayBuffer, normalVboHandle[i]);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
            }
            GL.BindVertexArray(0);
        }

        public FormRobot_multiObj()
        {
            InitializeComponent();
            _trackBars = new TrackBar[]{trackBar1,
                                        trackBar2,
                                        trackBar3,
                                        trackBar4,
                                        trackBar5,
                                        trackBar6};
            foreach (var bar in _trackBars)
            {
                //For track bar,both valueChanged and scroll event works like I needed
                // --- update immediately
                bar.ValueChanged += new EventHandler(scrollbar_ValueChanged);
                //bar.Scroll += new EventHandler(bar_Scroll);  
            }
        }

        void scrollbar_ValueChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            update(sender);
        }

        void update(object sender)
        {
            var bar = (TrackBar)sender;
            //System.Console.WriteLine(bar.Name);
            int i = int.Parse(bar.Name.Substring(bar.Name.Length - 1, 1));
            JointMove(i, bar.Value);
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            txtRenderer = new TextRenderer(glControl1.Width, glControl1.Height);
            
            System.Console.WriteLine(glControl1.GraphicsMode.ToString());
            glControl1.VSync = true;
            glControl1.KeyDown += glControl1_KeyDown;
            //glControl1.KeyPress += glControl1_KeyPress;
            glControl1.Resize += new EventHandler(glControl1_Resize);
            glControl1.Paint += new PaintEventHandler(glControl1_Paint);

            //loading data
            kChain[0].Name = "Base";
            kChain[1].Name = "Joint1";
            kChain[2].Name = "Joint2";
            kChain[3].Name = "Joint3";
            kChain[4].Name = "Joint4";
            kChain[5].Name = "Joint5";
            kChain[6].Name = "Joint6";

            myLoader.loadData(ref kChain);



            CreateShaders();
            CreateVBOs();
            CreateVAOs();

            //background color
            GL.ClearColor(Color.MidnightBlue);
            GL.Enable(EnableCap.DepthTest);

            Application.Idle += Application_Idle;// or += new EventHandler(Application_Idle);
            glControl1_Resize(glControl1, EventArgs.Empty);
        }

        #region OnClosing

        protected override void OnClosing(CancelEventArgs e)
        {
            Application.Idle -= Application_Idle;
            //textRenderer.Dispose();
            base.OnClosing(e);
        }

        #endregion

        #region Application_Idle event

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                //Render();//or glControl1.Invalidate
                glControl1.Invalidate();
            }
        }

        #endregion

        protected void glControl1_Resize(object sender, EventArgs e)
        {
            OpenTK.GLControl c = sender as OpenTK.GLControl;
            if (c.ClientSize.Height == 0)
                c.ClientSize = new System.Drawing.Size(c.ClientSize.Width, 1);
            //this can make sure view port center aligned with the object u r looking at
            GL.Viewport(0, 0, c.ClientSize.Width, c.ClientSize.Height);

            //update perspective matrix according to new aspect ratio
            
            Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2, aspectRatio(), 1, 4000, out projectionMatrix);
            //projectionMatrix = Matrix4.Identity;

            txtRenderer = new TextRenderer(c.ClientSize.Width, c.ClientSize.Height);
            Debug.Print("resize width {0}, height {1}", c.ClientSize.Width, c.ClientSize.Height);
        }

        protected void glControl1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.Q:
                    mvpCtrl.RotatePush(MVPControls.RotateDirection.up);
                    break;
                case Keys.Z:
                    mvpCtrl.RotatePush(MVPControls.RotateDirection.down);
                    break;
                case Keys.Delete:
                    mvpCtrl.RotatePush(MVPControls.RotateDirection.left);
                    break;
                case Keys.PageDown:
                    mvpCtrl.RotatePush(MVPControls.RotateDirection.right);
                    break;
                case Keys.Home:
                    mvpCtrl.TranslatePush(MVPControls.TranslateDirection.backward);
                    break;
                case Keys.End:
                    mvpCtrl.TranslatePush(MVPControls.TranslateDirection.forward);
                    break;
            }

        }

        protected void glControl1_Paint(object sender, PaintEventArgs e)
        {
            //GL.Clear(ClearBufferMask.ColorBufferBit);
            //glControl1.SwapBuffers();
            Render();
        }

        #region private void Render()

        private void Render()
        {

            //if multi-threading
            glControl1.MakeCurrent();



            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(shaderProgramHandle);

            //update text
            txtRenderer.Clear(Color.Transparent);
            position.Y = 0;
            
            txtRenderer.DrawString(DateTime.Now.Minute.ToString() + ":" +
                            DateTime.Now.Second.ToString() +
                            ":" +
                            DateTime.Now.Millisecond.ToString(), mono, Brushes.Yellow, position);
            position.Y += 20;
            Vector3 res = new Vector3((int)(mRobot.EOFPose.Row3.X),
                                    (int)(mRobot.EOFPose.Row3.Y),
                                    (int)(mRobot.EOFPose.Row3.Z));
            
            string str = res.ToString();
            txtRenderer.DrawString(str,mono,Brushes.Yellow,position);
            for (int i = 1; i < 7; i++)
            {
                position.Y += 20;
                txtRenderer.DrawString("J" + i + ": " + mRobot.GetJointsDisplay()[i].ToString(), mono, Brushes.Yellow, position);
            }

            //bind texture
            int texture = txtRenderer.Texture;
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Uniform1(uniform_texture, 0);

            //text
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BindVertexArray(vaoHandle_text);
            GL.Uniform1(uniform_objSelector, 0);
            Matrix4 identity = Matrix4.Identity;
            GL.UniformMatrix4(projectionMatrixLocation, false, ref identity);
            GL.UniformMatrix4(modelviewMatrixLocation, false, ref identity);
            GL.DrawArrays(PrimitiveType.Triangles, 0, txtRenderer.txtData.Length);
            GL.Disable(EnableCap.Blend);

            //user update modelviewMatrix
            GL.Uniform1(uniform_objSelector, 1);
            GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);
            mvpCtrl.computeMatricesFromInputs();
            
            idxStart = 0;
            for (int i = idxStart; i < BODYCOUNT; i++)
            {
                
                //if (!((i == 5)||(i==6))) continue;
                modelMatrix = mRobot.ModelTransf[i];
                //if (i == 1)
                //{
                //    modelMatrix = Matrix4.CreateRotationX((float)Math.PI / 2);
                //    modelMatrix = modelMatrix * Matrix4.CreateTranslation(-150, 0, -570);
                //}
                modelviewMatrix = modelMatrix * mvpCtrl.ViewMatrix;
                //modelviewMatrix.Transpose();
                GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);
                GL.Uniform3(colorVectorLocation, kChain[i].Color);
                GL.BindVertexArray(vaoHandle[i]);
                //DrawElements with indices
                GL.DrawArrays(PrimitiveType.Triangles, 0, kChain[i].Vertex.Length);
            }
            GL.UseProgram(0);
            //GL.MatrixMode(MatrixMode.Modelview);
            //GL.LoadIdentity();
            //GL.Enable(EnableCap.Texture2D);
            //GL.BindTexture(TextureTarget.Texture2D, textRenderer.Texture);
            
            glControl1.SwapBuffers();
        }

        #endregion

        private float aspectRatio()
        {
            float ratio = 1.0f;
            if (!FIXED_ASPECT_RATIO)
                ratio = (glControl1.ClientRectangle.Width / (float)glControl1.ClientRectangle.Height);
            return ratio;
        }

        private void JointMove(int index, float value)
        {
            System.Console.WriteLine("Joint {0} value {1}", index, value);

            jointVecter[index] = value * mRobot.Links[index].Sign;
            mRobot.JointMove(ref jointVecter);
        }

        private void IKTest()
        {
            //T = fkine(q2);
            //q2 =  0.5236    1.5708    0.5236    0.5236    1.5708         0
            //q2 =  pi/6    pi/2    pi/6    pi/6    pi/2         0
            float[] offset= {0, 0, -90, 0, 0, 0, 180};
            //float[] j = { 0, 0, -45, 0, 0, 0, 180 };
            //j0 = j - offset
            //float[] j = { 0, 0, 45, 0, 0, 0, 0 };
            //float[] j = { 0, 30, 90, 30, 30, 90, 0 };
            float[] j = { 0, 30, 180, 30, 30, 90, 180 };
            //T.Row0 = new Vector4(-0.75f, -0.2165f, 0.625f, -688.4f);
            //T.Row1 = new Vector4(-0.433f, 0.875f, -0.2165f, -397.4f);
            //T.Row2 = new Vector4(-0.5f, -0.433f, -0.75f, -1525.2f);
            //T.Row3 = new Vector4(0.0f, 0.0f, 0.0f, 1f);

            mRobot.JointMove(ref j);
            Matrix4 T = mRobot.EOFPose;
            T = mRobot.Tool.Inverted() * T;
            T.Transpose();

            float[] theta = new float[7];
            int arguments = 0;
            var l = mRobot.Links;
            if (tools.ikine6s(ref l, ref T, ref theta, arguments))
            {
                for (int i = 0; i < theta.Length; i++)
                {
                    Debug.Print("theta[{0}]", theta[i]);
                }
                Debug.Print("");

                mRobot.JointMove(ref theta);
                Matrix4 T0 = mRobot.Tool.Inverted() * mRobot.EOFPose;
                T.Transpose();
                var tt = T - T0;
            }
        }

        private void createJogRotation(ref Matrix4 T, int coord, int axis, float angle, out Matrix4 mat)
        {
            //coord 0 tool 1 world
            Vector3 vec = new Vector3(0f,0f,0f);
            Matrix4 tt = T;
            tt.Transpose();
            if (coord == 1)
            {
                vec = new Vector3(tt[axis, 0], tt[axis, 1], tt[axis, 2]);
            }
            else if (coord == 0)
            {
                vec[axis] = 1f;
            }
            else
            {//something wrong
                vec.X = 1f;
                angle = 0f;
            }
            Matrix4.CreateFromAxisAngle(vec, angle, out mat);
        }
        //tool cartesion jog
        private void CartesianMove(int sign,int coord)
        {
            Debug.Assert((sign == -1) || (sign == 1));
            float dT = sign * 50;
            float dR = sign * (float)(Math.PI*10 / 180f);
            Matrix4 dRMatrix = Matrix4.Identity;
            Matrix4 dTMatrix = Matrix4.Identity;
            Matrix4 T = mRobot.EOFPose;
            bool found = false;
            foreach (Control ctl in this.panel_ik.Controls)
            {
                if (ctl is RadioButton)
                {
                    RadioButton rdButton = ctl as RadioButton;
                    if (rdButton.Checked)
                    {
                        switch (rdButton.Name)
                        {
                            case "x":
                                dTMatrix = Matrix4.CreateTranslation(dT, 0f, 0f);
                                break;
                            case "y":
                                dTMatrix = Matrix4.CreateTranslation(0f, dT, 0f);
                                break;
                            case "z":
                                dTMatrix = Matrix4.CreateTranslation(0f, 0f, dT);
                                break;
                            case "roll":
                                createJogRotation(ref T, coord, 0, dR, out dRMatrix);
                                //dRMatrix = Matrix4.CreateRotationX(dR);
                                break;
                            case "pitch":
                                createJogRotation(ref T, coord, 1, dR, out dRMatrix);
                                break;
                            case "yaw":
                                createJogRotation(ref T, coord, 2, dR, out dRMatrix);
                                break;
                        }
                    }
                }
            }

            if (coord == 1)
            {
                T = dRMatrix * T * dTMatrix;//world
            }
            else if (coord == 0)
            {
                T = dTMatrix * dRMatrix * T;//tool
            }

            T = mRobot.Tool.Inverted() * T;
            T.Transpose();
            float[] theta = new float[7];
            int arguments = 0;
            var l = mRobot.Links;
            if (tools.ikine6s(ref l, ref T, ref theta, arguments))
            {
                mRobot.JointMove(ref theta);
            }
        }
        private void btnIncrease_Click(object sender, EventArgs e)
        {
            //0 tool, 1 world
            if (rdBtn1.Checked)
                CartesianMove(1, 1);
            else
                CartesianMove(1, 0);
        }
        private void btnDecrease_Click(object sender, EventArgs e)
        {
            if (rdBtn1.Checked)
                CartesianMove(-1, 1);
            else
                CartesianMove(-1, 0);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            IKTest();
        }
    }
}

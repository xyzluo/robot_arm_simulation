using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace _3DViewer
{
    class MyTextWriter
    {
        private readonly Font TextFont = new Font(FontFamily.GenericSansSerif, 8);
        private readonly Bitmap TextBitmap;
        private List<PointF> _positions;
        private List<string> _lines;
        private List<Brush> _colours;
        private int _textureId;
        private Size _clientSize;

        public void Update(int ind, string newText)
        {
            if (ind < _lines.Count)
            {
                _lines[ind] = newText;
                UpdateText();
            }
        }


        public MyTextWriter(Size ClientSize, Size areaSize)
        {
            _positions = new List<PointF>();
            _lines = new List<string>();
            _colours = new List<Brush>();

            TextBitmap = new Bitmap(areaSize.Width, areaSize.Height);
            this._clientSize = ClientSize;
            _textureId = CreateTexture();
        }

        private int CreateTexture()
        {
            int textureId;
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Replace);//Important, or wrong color on some computers
            Bitmap bitmap = TextBitmap;
            GL.GenTextures(1, out textureId);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
            GL.Finish();
            bitmap.UnlockBits(data);
            return textureId;
        }

        public void Dispose()
        {
            if (_textureId > 0)
                GL.DeleteTexture(_textureId);
        }

        public void Clear()
        {
            _lines.Clear();
            _positions.Clear();
            _colours.Clear();
        }

        public void AddLine(string s, PointF pos, Brush col)
        {
            _lines.Add(s);
            _positions.Add(pos);
            _colours.Add(col);
            UpdateText();
        }

        public void UpdateText()
        {
            if (_lines.Count > 0)
            {
                using (Graphics gfx = Graphics.FromImage(TextBitmap))
                {
                    gfx.Clear(Color.Black);
                    gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    for (int i = 0; i < _lines.Count; i++)
                        gfx.DrawString(_lines[i], TextFont, _colours[i], _positions[i]);
                }

                System.Drawing.Imaging.BitmapData data = TextBitmap.LockBits(new Rectangle(0, 0, TextBitmap.Width, TextBitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, TextBitmap.Width, TextBitmap.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                TextBitmap.UnlockBits(data);
            }
        }

        public void Draw()
        {
            GL.PushMatrix();
            GL.LoadIdentity();

            Matrix4 ortho_projection = Matrix4.CreateOrthographicOffCenter(0, _clientSize.Width, _clientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Projection);

            GL.PushMatrix();//
            GL.LoadMatrix(ref ortho_projection);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.DstColor);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, _textureId);


            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 0); GL.Vertex2(0, 0);
            GL.TexCoord2(1, 0); GL.Vertex2(TextBitmap.Width, 0);
            GL.TexCoord2(1, 1); GL.Vertex2(TextBitmap.Width, TextBitmap.Height);
            GL.TexCoord2(0, 1); GL.Vertex2(0, TextBitmap.Height);
            GL.End();
            GL.PopMatrix();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
        }
    }
    /// <summary>
    /// Uses System.Drawing for 2d text rendering.
    /// </summary>
    /// to render bitmap font, refer to tutorial11_2d_fonts
    /// to load a bitmap file and get handle, refer to tutorial_5 which show
    /// both loadBMP_custom and loadDDS. They return same thing: GLuint Texture
    public class TextRenderer : IDisposable
    {
        Bitmap bmp;
        Graphics gfx;
        int texture;
        Rectangle dirty_region;
        bool disposed;

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="width">The width of the backing store in pixels.</param>
        /// <param name="height">The height of the backing store in pixels.</param>
        public TextRenderer(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height ");
            if (GraphicsContext.CurrentContext == null)
                throw new InvalidOperationException("No GraphicsContext is current on the calling thread.");

            bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfx = Graphics.FromImage(bmp);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Clears the backing store to the specified color.
        /// </summary>
        /// <param name="color">A <see cref="System.Drawing.Color"/>.</param>
        public void Clear(Color color)
        {
            gfx.Clear(color);
            dirty_region = new Rectangle(0, 0, bmp.Width, bmp.Height);
        }

        /// <summary>
        /// Draws the specified string to the backing store.
        /// </summary>
        /// <param name="text">The <see cref="System.String"/> to draw.</param>
        /// <param name="font">The <see cref="System.Drawing.Font"/> that will be used.</param>
        /// <param name="brush">The <see cref="System.Drawing.Brush"/> that will be used.</param>
        /// <param name="point">The location of the text on the backing store, in 2d pixel coordinates.
        /// The origin (0, 0) lies at the top-left corner of the backing store.</param>
        public void DrawString(string text, Font font, Brush brush, PointF point)
        {
            gfx.DrawString(text, font, brush, point);

            SizeF size = gfx.MeasureString(text, font);
            dirty_region = Rectangle.Round(RectangleF.Union(dirty_region, new RectangleF(point, size)));
            dirty_region = Rectangle.Intersect(dirty_region, new Rectangle(0, 0, bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Gets a <see cref="System.Int32"/> that represents an OpenGL 2d texture handle.
        /// The texture contains a copy of the backing store. Bind this texture to TextureTarget.Texture2d
        /// in order to render the drawn text on screen.
        /// </summary>
        public int Texture
        {
            get
            {
                UploadBitmap();
                return texture;
            }
        }

        //public void Draw()
        //{
        //    //bind texture
        //    int texture = this.Texture;
        //    GL.ActiveTexture(TextureUnit.Texture0);
        //    GL.BindTexture(TextureTarget.Texture2D, texture);
        //    GL.Uniform1(uniform_texture, 0);

        //    //text
        //    GL.Enable(EnableCap.Blend);
        //    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        //    GL.BindVertexArray(vaoHandle_text);
        //    GL.Uniform1(uniform_objSelector, 0);
        //    Matrix4 identity = Matrix4.Identity;
        //    GL.UniformMatrix4(projectionMatrixLocation, false, ref identity);
        //    GL.UniformMatrix4(modelviewMatrixLocation, false, ref identity);
        //    GL.DrawArrays(PrimitiveType.Triangles, 0, txtRenderer.txtData.Length);
        //    GL.Disable(EnableCap.Blend);
        //}

        public void resize(int width, int height)
        {
            throw new Exception("Textrenderer resize() is not implemented");
            //Clear(Color.Black);
            //bmp.Dispose();
            //gfx.Dispose();
            //bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //gfx = Graphics.FromImage(bmp);
            //gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;        
        }
        #endregion

        #region Private Members

        // Uploads the dirty regions of the backing store to the OpenGL texture.
        void UploadBitmap()
        {
            if (dirty_region != RectangleF.Empty)
            {
                System.Drawing.Imaging.BitmapData data = bmp.LockBits(dirty_region,
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                    dirty_region.X, dirty_region.Y, dirty_region.Width, dirty_region.Height,
                    PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bmp.UnlockBits(data);

                dirty_region = Rectangle.Empty;
            }
        }

        #endregion

        #region IDisposable Members

        void Dispose(bool manual)
        {
            if (!disposed)
            {
                if (manual)
                {
                    bmp.Dispose();
                    gfx.Dispose();
                    if (GraphicsContext.CurrentContext != null)
                        GL.DeleteTexture(texture);
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TextRenderer()
        {
            Console.WriteLine("[Warning] Resource leaked: {0}.", typeof(TextRenderer));
        }

        #endregion

        //================================
        //additional stuff 
        //================================
        
        //text vertex
        //manipulate text size & position
        const float pScaleX = 1f;// pScaleY
        const float pScaleY = 1f;//0.1f;
        //text works in (-1 ~ 0.9999847114086), robot -0.82 ~ -inf
        // so we display text at deepest to not to block robot arm.
        const float pScaleZ = 1f;
        const float depthZ = 0.9999847114086f;
        Vector3 txtPosition = new Vector3(0f, 0f, 0f);
        Vector3[] txtPositionVboData = new Vector3[]{
            new Vector3( -1.0f*pScaleX,  -1.0f*pScaleY,  depthZ*pScaleZ),
            new Vector3( 1.0f*pScaleX,   -1.0f*pScaleY,  depthZ*pScaleZ),
            new Vector3( 1.0f*pScaleX,    1.0f*pScaleY,  depthZ*pScaleZ),
            new Vector3( -1.0f*pScaleX,   1.0f*pScaleY,  depthZ*pScaleZ),
            new Vector3( -1.0f*pScaleX,  -1.0f*pScaleY,  depthZ*pScaleZ),
            new Vector3(  1.0f*pScaleX,   1.0f*pScaleY,  depthZ*pScaleZ)};

        //UV
        const float uvScale = 1.0f;
        Vector2[] txtUVBuffer = new Vector2[]{
            new Vector2( 0.0f, 1.0f)*uvScale,
            new Vector2( 1.0f, 1.0f)*uvScale,
            new Vector2( 1.0f, 0.0f)*uvScale,
            new Vector2( 0.0f, 0.0f)*uvScale,
            new Vector2( 0.0f, 1.0f)*uvScale,
            new Vector2( 1.0f, 0.0f)*uvScale};

        public Vector3[] txtData
        {
            get { return txtPositionVboData; }
        }
        public Vector2[] uvData
        {
            get { return txtUVBuffer; }
        }
        public int CreatVBOHandle()
        { 
            int vbo=0;

            return vbo;
        }


    }

    public class dataObject
    {

        int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public dataObject() { }
        public dataObject(string nameStr)
        {
            name = nameStr;
        }

        Vector3 color;
        public Vector3 Color
        {
            get { return color; }
            set { color = value; }
        }
        Vector3[] vertex;
        Vector3[] normal;
        Vector3[] boundary = new Vector3[2] { 
            new Vector3(
                Single.PositiveInfinity, 
                Single.PositiveInfinity, 
                Single.PositiveInfinity), 
            new Vector3(
                Single.NegativeInfinity, 
                Single.NegativeInfinity, 
                Single.NegativeInfinity) 
        };
        Vector3 center = new Vector3();
        public Vector3 Center
        {
            get { return center; }
            set { center = value; }
        }

        public Vector3[] Vertex
        {
            get { return vertex; }
            set { 
                vertex = value;
                for (long i = 0; i < vertex.Length; ++i)
                {
                    boundary[0].X = Math.Min(boundary[0].X, vertex[i].X);
                    boundary[1].X = Math.Max(boundary[1].X, vertex[i].X);
                    boundary[0].Y = Math.Min(boundary[0].Y, vertex[i].Y);
                    boundary[1].Y = Math.Max(boundary[1].Y, vertex[i].Y);
                    boundary[0].Z = Math.Min(boundary[0].Z, vertex[i].Z);
                    boundary[1].Z = Math.Max(boundary[1].Z, vertex[i].Z);
                }
                center.X = 0.5f * (boundary[0].X + boundary[1].X);
                center.Y = 0.5f * (boundary[0].Y + boundary[1].Y);
                center.Z = 0.5f * (boundary[0].Z + boundary[1].Z);
            }
        }

        public Vector3[] Normal
        {
            get { return normal; }
            set { normal = value; }
        }    
    }

    public class RobotArm
    {
        //hmm, don't forget this
        Matrix4 CADZERO = Matrix4.CreateTranslation(0f, 0f, 0f);
        Matrix4 mbase = Matrix4.CreateTranslation(0, 0f, 570f);
        Matrix4 mtool = Matrix4.CreateTranslation(0f, 0f, 175f);
        Matrix4 mtoolBase = Matrix4.CreateTranslation(0f, 0f, 0f);
        Matrix4 mEOF = Matrix4.Identity;
        float[] jointVector = new float[7];
        float[] jointsDisplay = new float[7];
        public struct link
        {
            private readonly float theta;
            private readonly float d;
            private readonly float a;
            private readonly float alpha;
            private float offset;
            private readonly int sign;
            public link(float theta,float d, float a, float alpha,float offset = 0, int sign = 1)
            {
                this.theta = theta;
                this.d = d;
                this.a = a;
                this.alpha = alpha;
                this.offset = offset;
                this.sign = sign;
            }

            public float Theta
            { get { return theta; } }
            public float D
            { get { return d; } }
            public float A
            { get { return a; } }
            public float Alpha
            { get { return alpha; } }
            public float Offset
            { 
                get { return offset; }
                set { offset = value; }
            }
            
            public float Sign
            { get { return sign; } }
        }
        link[] links = new link[7];//CAD link
        link[] IKLinks = new link[7];
        //link physical charactors, with standard DH.
        //or rename to T1,..T6 for typical fkine ?
        Matrix4[] linkTransf = new Matrix4[7];

        //link's (model) pose, constant
        Matrix4[] linkOrigin = new Matrix4[7];
        Matrix4[] fkineTransf = new Matrix4[7];
        Matrix4[] modelTransf = new Matrix4[7];
        public Matrix4[] ModelTransf
        {
            get { return modelTransf; }
        }

        public float[] GetJointsDisplay()
        {
            for (int i = 0; i < 7; i++)
            {
                jointsDisplay[i] = jointVector[i] * links[i].Sign;
            }
            return jointsDisplay;
        }
        //public float[] Joints
        //{ get { return jointVector; } }

        public link[] Links
        { get { return links; } }

        public Matrix4 EOFPose
        {
            // Z measured from bottum of base part
            get { return mEOF*CADZERO.Inverted(); }
        }

        public Matrix4 Base
        { get { return mbase; } }

        public Matrix4 Tool
        { 
            get { return mtool; }
            set { mtool = value; }
        }

        public Matrix4[] fkTransf
        { 
            get { return fkineTransf; } 
        }

        public RobotArm()
        {
            //to calculate inverse kinematic in Matlab toolbox,
            //end plate (link6) d = 175 should be set to zero.
            // ( theta, d (offset), a(link length), alpha (axis twist))
            //links[0] = new link(0, 0, 0, 0);
            //links[1] = new link(0, 570, 150, -90);
            ////links[1] = new link(0, 570, 150, 90);
            //links[2] = new link(-90, 0, 870, 0);
            ////links[2] = new link(-90, 0, 870, 180);
            //links[3] = new link(0, 0, 170, -90);
            //links[4] = new link(0, 1016, 0, 90);
            ////links[4] = new link(0, -1016, 0, 90);
            //links[5] = new link(0, 0, 0, -90);
            ////links[6] = new link(180, 175, 0, 0);
            //links[6] = new link(180, -175, 0, 0);

            //CAD links, read from file
            links[0] = new link(0, 0, 0, 0);
            links[1] = new link(0, 0, 150, 90);
            links[2] = new link(0, 0, -870, 0, -90, -1);//-90 offset
            links[3] = new link(0, 0, -170, -90);
            links[4] = new link(0, 1016, 0, 90,0,-1);
            links[5] = new link(0, 0, 0, -90);
            links[6] = new link(0, 0, 0, 0, 180,-1);//180 offset

            //model links
            // for test only
            //IKLinks[0] = new link(0, 0, 0, 0);
            //IKLinks[1] = new link(0, 0, 150, 90);
            //IKLinks[2] = new link(0, 0, -870, 0);
            //IKLinks[3] = new link(0, 0, -170, -90);
            //IKLinks[4] = new link(0, 1016, 0, 90);
            //IKLinks[5] = new link(0, 0, 0, -90);
            //IKLinks[6] = new link(0, 0, 0, 0);
            for (int i = 0; i < 7; i++)
            {
                IKLinks[i] = links[i];
                IKLinks[i].Offset = 0f;
            }



            //=======================================
            //robot construct
            //=======================================
            //build up links with jointVector == 0
            updateLink(ref links,ref jointVector);
            //transform by which transform each part to world ZERO. 
            //one time task 
            for (int i = 0; i < 7; i++)
            {
                linkOrigin[i] = fkineTransf[i].Inverted();
            }

            fKine2Model();
        }

        // from links physical parameter plus current joint value, get each link's 
        // transformation
        private void updateLink(ref link[] L,ref float[] jvector )//to: dependancy inverse
        {
            //reconstruct link
            for (int i = 0; i < 7; i++)
            {
                Matrix4 thisMatrix = Matrix4.Identity;
                getLinkTransf(ref L[i], jvector[i], out thisMatrix);
                //getLinkTransf(ref links[i], jointVector[i], out linkTransf[i]);
                linkTransf[i] = thisMatrix;
            }
            //base offset
            fkineTransf[0] = CADZERO*linkTransf[0];
            //relative to world
            for (int i = 1; i < 7; i++)//1 to 6
            {
                fkineTransf[i] = linkTransf[i]*fkineTransf[i - 1];
                //fkineTransf[i] = fkineTransf[i - 1] * linkTransf[i] * linkOrigin[i];
            }

            updateEOF();
        }

        private void updateEOF()
        {
            mEOF =  mtool* mtoolBase*fkineTransf[6];
        }
        private void getLinkTransf(ref link lnk, float Theta, out Matrix4 transf)
        {
            float theta = lnk.Offset + Theta, d = lnk.D, a = lnk.A, alpha = lnk.Alpha;
            transf = new Matrix4(
                tools.CosD(theta), -tools.SinD(theta) * tools.CosD(alpha), tools.SinD(theta) * tools.SinD(alpha), a * tools.CosD(theta),
                tools.SinD(theta), tools.CosD(theta) * tools.CosD(alpha), -tools.CosD(theta) * tools.SinD(alpha), a * tools.SinD(theta),
                0, tools.SinD(alpha), tools.CosD(alpha), d,
                0, 0, 0, 1);
            transf.Transpose();
        }

        private void fKine2Model()
        {
            for (int i = 0; i < 7; i++)//1 to 6, 0 never move
            {
                modelTransf[i] = linkOrigin[i] *fkineTransf[i];
            }
        }

        //
        public void JointMove(ref float[] joints)
        {
            jointVector = joints;
            updateLink(ref links, ref jointVector);
            fKine2Model();
        }

        public Matrix4 fkine(ref float[] jv)
        {
            Matrix4 T = Matrix4.Identity;
            updateLink(ref IKLinks, ref jv);
            T =  fkineTransf[6];
            return T;
        }

    }
    
    class tools
    {
        static public T[] InitializeArray<T>(int length) where T : new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; ++i)
            {
                array[i] = new T();
            }
            return array;
        }
        
        static public float CosD(float angle)
        {
            return (float) System.Math.Cos(angle / 180 * System.Math.PI);
        }

        static public float SinD(float angle)
        {
            return (float)System.Math.Sin(angle / 180 * System.Math.PI);
        }

        static public float ceiling(float number, float Limit)
        {
            if (number > 0)
            {
                return Math.Min(number, Math.Abs(Limit));
            }
            else
            {
                return Math.Max(number, -1 * Math.Abs(Limit));
            }
        }

        static public bool ikine6s(ref RobotArm.link[] links, ref Matrix4 T, ref float[] Theta,int variagin=7)
        {
            bool result = false;
            //arm links 0~6 use 1~6
            //assert Theta.Length == 6
            for (int i = 0; i < Theta.Length; i++)
            {
                Theta[i] = 0;
            }

            double[] theta = new double[7];
            var L = links;
            var a1 = L[1].A;
            var a2 = L[2].A;
            var a3 = L[3].A;
            
            //assert :  be spherical wrist

            var d3 = L[3].D;
            var d4 = L[4].D;

            //assert : T be homo xform

            //undo base and tool transform

            var Ox = T[0, 1];
            var Oy = T[1, 1];
            var Oz = T[2, 1];

            var Ax = T[0, 2];
            var Ay = T[1, 2];
            var Az = T[2, 2];

            var Px = T[0, 3];
            var Py = T[1, 3];
            var Pz = T[2, 3];

            int n1,n2,n4; //
            n1 = -1;
            n2 = -1;
            n4 = -1;
            //set configuration parameters
            if (1 == (variagin & 1))
                n1 = -1;                //'l'
            else
                n1 = 1;                 //'r'

            if (2 == (variagin & 2))
            {                           //'u'
                n2 = n1;
            }
            else                        //'d'
            {
                n2 = -n1;
            }

            if (4 == (variagin & 4))    //'n'
                n4 = 1;
            else
                n4 = -1;                //'f'

            // solve
            // theta 1
            var r = Math.Sqrt(Px * Px + Py * Py);
            if (n1 == 1)
            {
                theta[1] = Math.Atan2(Py, Px) + Math.Asin(d3 / r);
            }
            else{
                theta[1] = Math.Atan2(Py,Px) + Math.PI - Math.Asin(d3/r);
            }
            if(double.IsNaN(theta[1])) return result;

            //theta 2
            var V114 = Px*Math.Cos(theta[1]) + Py* Math.Sin(theta[1]) - a1;
            r = Math.Sqrt(V114 * V114 + Pz * Pz);
            var t1 = (a2 * a2 - d4 * d4 - a3 * a3 + V114 * V114 + Pz * Pz);
            var t2 = 2.0d * a2 * r;
            var tmp = (a2 * a2 - d4 * d4 - a3 * a3 + V114 * V114 + Pz * Pz) / (2.0d * a2 * r);
            var Psi = Math.Acos(tmp);
            //check Psi is a real
            if (double.IsNaN(Psi)) return result;
            theta[2] = Math.Atan2(Pz, V114) + n2 * Psi;
            if (double.IsNaN(theta[2])) return result;

            //theta[3]
            var num = Math.Cos(theta[2]) * V114 + Math.Sin(theta[2]) * Pz - a2;
            var den = Math.Cos(theta[2]) * Pz - Math.Sin(theta[2]) * V114;
            theta[3] = Math.Atan2(a3, d4) - Math.Atan2(num, den);
            if (double.IsNaN(theta[3])) return result;
            //theta[4]
            var V113 = Math.Cos(theta[1]) * Ax + Math.Sin(theta[1]) * Ay;
            var V323 = Math.Cos(theta[1]) * Ay - Math.Sin(theta[1]) * Ax;
            var V313 = Math.Cos(theta[2] + theta[3]) * V113 + Math.Sin(theta[2] + theta[3]) * Az;
            theta[4] = Math.Atan2((n4 * V323), (n4 * V313));
            if (double.IsNaN(theta[4])) return result;
            //theta[5]
            num = -Math.Cos(theta[4]) * V313 - V323 * Math.Sin(theta[4]);
            den = -V113 * Math.Sin(theta[2] + theta[3]) + Az * Math.Cos(theta[2] + theta[3]);
            theta[5] = Math.Atan2(num, den);
            if (double.IsNaN(theta[5])) return result;

            var V112 = Math.Cos(theta[1]) * Ox + Math.Sin(theta[1]) * Oy;
            var V132 = Math.Sin(theta[1]) * Ox - Math.Cos(theta[1]) * Oy;
            var V312 = V112 * Math.Cos(theta[2] + theta[3]) + Oz * Math.Sin(theta[2] + theta[3]);
            var V332 = -V112 * Math.Sin(theta[2] + theta[3]) + Oz * Math.Cos(theta[2] + theta[3]);
            var V412 = V312 * Math.Cos(theta[4]) - V132 * Math.Sin(theta[4]);
            var V432 = V312 * Math.Sin(theta[4]) + V132 * Math.Cos(theta[4]);
            num = -V412 * Math.Cos(theta[5]) - V332 * Math.Sin(theta[5]);
            den = -V432;
            theta[6] = Math.Atan2(num, den);
            if (double.IsNaN(theta[6])) return result;
            //remove the link offset angle
            //result in degree
            for (int i = 1; i < 7; i++)
            { Theta[i] = (float)(theta[i]) * (float)(180d/ Math.PI) - L[i].Offset; }
            return true;
        }
    }

    class stlLoader
    {
        dataObject mDataObj = new dataObject();

        private string fileName(string dataName)
        {
            string fName = "";
            switch (dataName)
            {
                case "Base":
                    fName = "FANUC_M710iC_50-0";
                    break;
                case "Joint1":
                    fName = "FANUC_M710iC_50-1";
                    break;
                case "Joint2":
                    fName = "FANUC_M710iC_50-2";
                    break;
                case "Joint3":
                    fName = "FANUC_M710iC_50-3";
                    break;
                case "Joint4":
                    fName = "FANUC_M710iC_50-4";
                    break;
                case "Joint5":
                    fName = "FANUC_M710iC_50-5";
                    break;
                case "Joint6":
                    fName = "FANUC_M710iC_50-6";
                    break;
            }
            return fName;
        }

        public void loadData(ref dataObject[] dataObjArray)
        {
            for (int i = 0; i < dataObjArray.Length; i++)
            {
                loadData(ref dataObjArray[i]);
            }
        }

        public void loadData(ref dataObject dataObj)
        {
            int counter = 0;
            string fName = "";
            string line;

            fName = fileName(dataObj.Name);
            if (fName == "") return;

            fName = ".\\data\\" + fName + ".stl";
            System.IO.StreamReader file =
                new System.IO.StreamReader(fName);

            while ((line = file.ReadLine()) != null)
            {
                //Console.WriteLine(line);
                counter++;
            }
            file.Close();
            //Console.ReadLine();

            if ((counter - 3) % 7 != 0)
                System.Console.WriteLine("wrong file format ?");

            int pointNumber = (counter - 3) / 7 * 3;
            mDataObj.Vertex = new Vector3[pointNumber];
            mDataObj.Normal = new Vector3[pointNumber];
            file = new System.IO.StreamReader(fName);
            long nCounter = 0;
            long pCounter = 0;
            while ((line = file.ReadLine()) != null)
            {
                //Console.WriteLine(line);
                line = line.Trim();
                string[] tokens = line.Split(' ');
                if (tokens[0] == "facet")
                {
                    mDataObj.Normal[nCounter] = new Vector3(float.Parse(tokens[2]), float.Parse(tokens[3]), float.Parse(tokens[4]));
                    nCounter++;
                    mDataObj.Normal[nCounter] = mDataObj.Normal[nCounter - 1];
                    nCounter++;
                    mDataObj.Normal[nCounter] = mDataObj.Normal[nCounter - 1];
                    nCounter++;
                }
                else if (tokens[0] == "vertex")
                {
                    mDataObj.Vertex[pCounter] = new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
                    pCounter++;
                }
                else if (tokens[0] == "color")
                { 
                    mDataObj.Color = new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
                }
            }
            file.Close();
            //dataObj = mDataObj;
            dataObj.Vertex = mDataObj.Vertex;
            dataObj.Normal = mDataObj.Normal;
            dataObj.Color = mDataObj.Color;
        }
        //public Vector3[] getPData()
        //{
        //    return mDataObj.Vertex;
        //}
        //public Vector3[] getNData()
        //{
        //    return mDataObj.Normal;
        //}

        //static void Main(string[] args)
        //{
        //    stlLoader loader = new stlLoader();
        //    loader.getData();
        //}
    }
    
    //MVP user control, actually camera only ;-)
    class MVPControls
    {
        public enum RotateDirection
        { 
            left = 0,
            right = 1,
            up = 2,
            down = 3
        }

        public enum TranslateDirection
        { 
            forward = 0,
            backward = 1
        }

        static DateTime lastTime = DateTime.Now;
        float deltaTime = 0;
        // Initial Field of View

        #region camera motion control
        //=====================================
        //camera motion 
        //=====================================
        //speed const
        const float initialFoV = 45.0f;
        const float rAcceleration = 0.5f;
        const float tAcceleration = 1f;
        const float rSpeedMax = 20.0f; // 3 units / second
        const float tSpeedMax = 200.0f;
        const float mouseSpeed = 0.005f;
        float rSpeed_v = 0;
        float rSpeed_h = 0;
        float tSpeed = 0;
        
        //rotate
        bool bRotateSpeedUp = false;
        public void RotatePush(RotateDirection direction)
        {
            bRotateSpeedUp = true;
            float accel_v = 0;
            float accel_h = 0;
            if (direction == RotateDirection.left)
                accel_h = -1 * rAcceleration;
            else if (direction == RotateDirection.right)
                accel_h = rAcceleration;
            else if (direction == RotateDirection.up)
                accel_v = -1 * rAcceleration;
            else if (direction == RotateDirection.down)
                accel_v = rAcceleration;

            rSpeed_v += accel_v;
            rSpeed_v = 2* tools.ceiling(rSpeed_v, rSpeedMax);
            rSpeed_h += accel_h;
            rSpeed_h = 2* tools.ceiling(rSpeed_h, rSpeedMax);
        }

        //translate
        bool bTranslateSpeedUp = false;
        public void TranslatePush(TranslateDirection direction)
        { 
            bTranslateSpeedUp = true;
            float accel = 0;
            if (direction == TranslateDirection.forward)
                accel = tAcceleration;
            if (direction == TranslateDirection.backward)
                accel = -1 * tAcceleration;
            tSpeed += accel;
            tSpeed = 2* tools.ceiling(tSpeed, tSpeedMax);
            //System.Console.WriteLine("TranslatePush tSpeed {0}", tSpeed);
        }

        private void Deccel()
        {
            if (!bRotateSpeedUp)
            {
                if (rSpeed_h != 0)
                {
                    if (Math.Abs(rSpeed_h)<0.01)
                        rSpeed_h = 0.0f;
                    else
                        rSpeed_h /= 2;
                }
                if (rSpeed_v != 0)
                {
                    if (Math.Abs(rSpeed_v) < 0.01)
                        rSpeed_v = 0.0f;
                    else
                        rSpeed_v /= 2;
                }
            }
            else
                bRotateSpeedUp = false;

            if (!bTranslateSpeedUp)
            {
                if (tSpeed != 0)
                {
                    if (Math.Abs(tSpeed) < 0.01)
                        tSpeed = 0.0f;
                    else
                    {
                        tSpeed /= 2;
                        //System.Console.WriteLine("Deccel tSpeed {0}", tSpeed);
                    }
                }
            }
            else
                bTranslateSpeedUp = false;
        }
         
        // spherial coord R
        float distance = 2000;
        public float CameraDistance
        {
            get { return distance; }
            set 
            {
                if (value >0 )
                    distance = value; 
            }
        }
        Vector3 position = new Vector3();
        public Vector3 CameraPosition
        {
            get { return position; }
            set { position = value; }//need to update distance together
        }

        Vector3 target = new Vector3();
        public Vector3 TargetPosition
        {
            set { target = value; }
        }
        // Initial horizontal angle : toward -Z
        float horizontalAngle = 0f;//3.14f;
        public float HorizontalAngle
        {
            get { return horizontalAngle; }
            set 
            {
                if (value > 360)
                    horizontalAngle = value - 360;
                else if (value < 0)
                    horizontalAngle = value + 360;
                else
                    horizontalAngle = value;
            }
        }

        // Initial vertical angle : none
        float verticalAngle = 0.0f;
        public float VerticalAngle
        {
            get { return verticalAngle; }
            set 
            {
                if ((value<175.0) && (value>-175.0))
                    verticalAngle = value; 
            }
        }

        Matrix4[] modelMatrix = new Matrix4[7];
        public Matrix4[] ModelMatrix
        {
            get { return modelMatrix; }
            set { modelMatrix = value; }
        }

        Matrix4 viewMatrix;
        public Matrix4 ViewMatrix
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }
        #endregion

        //
        //projection no update yet
        Matrix4 projectionMatrix;
        public Matrix4 ProjectionMatrix
        {
            get { return projectionMatrix; }
            set { projectionMatrix = value; }
        }

        //update view 
        public void computeMatricesFromInputs()
        {
            DateTime currentTime = DateTime.Now;
            deltaTime = (currentTime - lastTime).Seconds + (currentTime - lastTime).Milliseconds * 0.001f;
            //System.Console.WriteLine("deltaTime {0}", deltaTime);
            lastTime = currentTime;
            float step = rSpeed_v * deltaTime;
            //System.Console.WriteLine("angle step {0}", step);
            VerticalAngle += step;
            //System.Console.WriteLine("VerticalAngle {0}", VerticalAngle);
            step = rSpeed_h * deltaTime;
            HorizontalAngle += step;
            //System.Console.WriteLine("HorizontalAngle {0}", horizontalAngle);
            step = tSpeed * deltaTime;
            //System.Console.WriteLine("distance step {0}", step);
            CameraDistance += step;
            //System.Console.WriteLine("CameraDistance {0}", distance);
            Deccel();
            //position direction
            position = new Vector3( (float)tools.SinD(verticalAngle) * tools.CosD(horizontalAngle),
                                    (float)tools.SinD(verticalAngle) * tools.SinD(horizontalAngle),
                                    (float)tools.CosD(verticalAngle));
            position *= distance;//this is relative position !!! so you need add to target, see below.

            viewMatrix = Matrix4.LookAt(position+target,target,new Vector3(0,0,1));
            
        }
    }
}

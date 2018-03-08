using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTextRender_VAO_VBO
{

    /// <summary>
    /// Uses System.Drawing for 2d text rendering.
    /// </summary>
    /// to render bitmap font, refer to tutorial11_2d_fonts
    /// to load a bitmap file and get handle, refer to tutorial_5 which show
    /// both loadBMP_custom and loadDDS. They return same thing: GLuint Texture
    public class TextRenderer : IDisposable
    {
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
            //shader
            shaderProgramHandle = CreateShaders();
            uniform_texture = GL.GetUniformLocation(shaderProgramHandle,"myTextureSampler");//1
            uniform_objSelector = GL.GetUniformLocation(shaderProgramHandle, "selector");//3
            projectionMatrixLocation = GL.GetUniformLocation(shaderProgramHandle,"projection_matrix");
            modelviewMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "modelview_matrix");

            //vbo
            CreatVBOHandle();
            //vao
            CreatVAOHandle();

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
        /// Draw to screen
        /// </summary>
        public void Draw()
        {
            GL.UseProgram(this.shaderProgramHandle);
            //bind texture
            int texture = this.Texture;
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Uniform1(uniform_texture, 0);
            //textureUniform.Set(shaderProgram);

            //text
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BindVertexArray(vaoHandle_text);
            GL.Uniform1(uniform_objSelector, 0);
            //selectorUniform.Set(shaderProgram);
            Matrix4 identity = Matrix4.Identity;
            GL.UniformMatrix4(projectionMatrixLocation, false, ref identity);
            GL.UniformMatrix4(modelviewMatrixLocation, false, ref identity);
            GL.DrawArrays(PrimitiveType.Triangles, 0, this.TxtData.Length);

            //finish up
            GL.Disable(EnableCap.Blend);
            GL.BindVertexArray(0);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);
        }

        public void Resize(int width, int height)
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

        #region private Members
        Bitmap bmp;
        Graphics gfx;
        int texture;
        Rectangle dirty_region;
        bool disposed;


        #region MyShaderString
        string vertexShaderSource = @"
            #version 330 core

            precision highp float;

            uniform mat4 projection_matrix;
            uniform mat4 modelview_matrix;

            in vec3 in_position;
            in vec3 in_normal;
            in vec2 texpos;

            out vec3 normal;
            out vec2 uv;

            void main(void)
            {
              //works only for orthogonal modelview
              normal = (modelview_matrix * vec4(in_normal, 0)).xyz;
  
              gl_Position = projection_matrix * modelview_matrix * vec4(in_position, 1);
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

        int shaderProgramHandle;
        //private VertexBuffer<Vector3> vertexBuffer;
        //private VertexBuffer<Vector2> uvBuffer;
        //private VertexArray<Vector3> vertexArray;
        //private VertexArray<Vector2> uvArray;
        //private Matrix4Uniform projectionMatrixUniform;
        //private Matrix4Uniform modelViewMatrixUniform;

        int uniform_texture, uniform_objSelector;
        //private SingleUniform textureUniform = new SingleUniform("myTextureSampler")
        //{ Value = 0 };
        //private SingleUniform selectorUniform = new SingleUniform("selector")
        //{
        //    Value = 0
        //};

        int projectionMatrixLocation;
        int modelviewMatrixLocation;
        int vaoHandle_text = 0;

        //================================
        //additional stuff 
        //================================

        //text vertex
        //manipulate text size & position
        const float pScaleX = 1f;// pScaleY
        const float pScaleY = 1f;//0.1f;
        // text works in (-1 ~ 0.9999847114086), robot -0.82 ~ -inf
        // so we display text at deepest to not to block robot arm.
        const float pScaleZ = 1f;
        //const float depthZ = 0.9999847114086f;
        const float depthZ = 0.9999f;
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

        private Vector3[] TxtData
        {
            get { return txtPositionVboData; }
        }
        private Vector2[] UvData
        {
            get { return txtUVBuffer; }
        }

        int txtPositionVboHandle = 0;
        int txtUVVboHandle = 0;

        private int CreateShaders()
        {
            var vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            var fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);

            GL.CompileShader(vertexShaderHandle);
            GL.CompileShader(fragmentShaderHandle);

            Debug.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle));
            Debug.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle));

            // Create program
            var handle = GL.CreateProgram();

            GL.AttachShader(handle, vertexShaderHandle);
            GL.AttachShader(handle, fragmentShaderHandle);

            //bind attribute index with a named attribute variable, index will be used by VAO
            //so the index can be any user specified.
            //can avoid calling BindAttribLocation if use layout = <index> in shader
            //GL.BindAttribLocation(handle, 0, "in_position");
            //GL.BindAttribLocation(handle, 1, "in_normal");
            //GL.BindAttribLocation(handle, 2, "texpos");
            GL.LinkProgram(handle);
            Debug.WriteLine(GL.GetProgramInfoLog(handle));

            return handle;
        }
        private void CreatVBOHandle()
        {
            // text buffer
            txtPositionVboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, txtPositionVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(this.TxtData.Length * Vector3.SizeInBytes),
                this.TxtData, BufferUsageHint.StaticDraw);

            txtUVVboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, txtUVVboHandle);
            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer,
                new IntPtr(this.UvData.Length * Vector2.SizeInBytes),
                this.UvData, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        private void CreatVAOHandle()
        {
            vaoHandle_text = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle_text);

            int index = GL.GetAttribLocation(shaderProgramHandle,"in_position");
            GL.EnableVertexAttribArray(index);//"in_position" see BindAttribLocation for index
            GL.BindBuffer(BufferTarget.ArrayBuffer, txtPositionVboHandle);
            GL.VertexAttribPointer(index, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);

            index = GL.GetAttribLocation(shaderProgramHandle, "texpos");
            GL.EnableVertexAttribArray(index);//"texpos" see BindAttribLocation for index
            GL.BindBuffer(BufferTarget.ArrayBuffer, txtUVVboHandle);
            GL.VertexAttribPointer(index, 2, VertexAttribPointerType.Float, true, Vector2.SizeInBytes, 0);

            GL.BindVertexArray(0);
        }


        // Uploads the dirty regions of the backing store to the OpenGL texture.
        private void UploadBitmap()
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


        /// <summary>
        /// Gets a <see cref="System.Int32"/> that represents an OpenGL 2d texture handle.
        /// The texture contains a copy of the backing store. Bind this texture to TextureTarget.Texture2d
        /// in order to render the drawn text on screen.
        /// </summary>
        private int Texture
        {
            get
            {
                UploadBitmap();
                return texture;
            }
        }
        #endregion
    }
}

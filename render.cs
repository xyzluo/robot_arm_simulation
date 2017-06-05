using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace _3DViewer
{
    public class Renderer
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
        public const float toRad = (float)(Math.PI / 180);
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

        Matrix4 projectionMatrix, modelMatrix, modelviewMatrix;
        dataObject[] kChain = tools.InitializeArray<dataObject>(KINECOUNT);

        public Renderer()
        {
            CreateShaders();
            CreateVBOs();
            CreateVAOs();
        }

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


            Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2, 1, 1, 4000, out projectionMatrix);

            //
            //modelviewMatrix = Matrix4.LookAt(new Vector3(250, -250, -600), new Vector3(193, -228, -565), new Vector3(0, 1, 0));
            //modelviewMatrix = Matrix4.LookAt(new Vector3(00.0f, 00.0f, 1200.0f), kChain[prtLookAt].Center, new Vector3(0, 0, 1));
            //Vector3 eye = kChain[prtLookAt].Center;
            //eye.Y += 2000;
            //modelviewMatrix = Matrix4.LookAt(eye, kChain[prtLookAt].Center, new Vector3(0, 0, 1));


            //modelviewMatrix = mvpCtrl.ViewMatrix;
            //GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);
            //GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);
        }

        void CreateVBOs()
        {


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

    }
}

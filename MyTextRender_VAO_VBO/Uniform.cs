using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace MyTextRender_VAO_VBO
{
    sealed class Matrix4Uniform
    {
        private readonly string name;
        private Matrix4 matrix;

        public Matrix4 Matrix { get { return this.matrix; } set { this.matrix = value; } }

        public Matrix4Uniform(string name)
        {
            this.name = name;
        }

        public void Set(ShaderProgram program)
        {
            // get uniform location
            var i = program.GetUniformLocation(this.name);

            // set uniform value
            GL.UniformMatrix4(i, false, ref this.matrix);
        }
    }

    sealed class Vector2Uniform
    {
        private readonly string name;
        private Vector2 vector;

        public Vector2 Vector { get { return this.vector; } set { this.vector = value; } }

        public Vector2Uniform(string name)
        {
            this.name = name;
        }

        public void Set(ShaderProgram program)
        {
            // get uniform location
            var i = program.GetUniformLocation(this.name);

            // set uniform value
            GL.Uniform2(i,vector.X,vector.Y);
        }
    }

    sealed class SingleUniform
    {
        private readonly string name;
        private float value;
        public float Value { get { return value; } set { this.value = value; } }

        public SingleUniform(string name)
        {
            this.name = name;
        }

        public void Set(ShaderProgram program)
        {
            var i = program.GetUniformLocation(this.name);
            GL.Uniform1(i, this.value);
        }
    }
}

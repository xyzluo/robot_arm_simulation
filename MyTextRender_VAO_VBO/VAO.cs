using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace MyTextRender_VAO_VBO
{
    sealed class VertexAttribute
    {
        private readonly string name;
        private readonly int size;
        private readonly VertexAttribPointerType type;
        private readonly bool normalize;
        private readonly int stride;
        private readonly int offset;

        public VertexAttribute(string name, int size, VertexAttribPointerType type,
            int stride, int offset, bool normalize = false)
        {
            this.name = name;
            this.size = size;
            this.type = type;
            this.stride = stride;
            this.offset = offset;
            this.normalize = normalize;
        }

        public void Set(ShaderProgram program)
        {
            // get location of attribute from shader program
            int index = program.GetAttributeLocation(this.name);

            // enable and set attribute
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, this.size, this.type,
                this.normalize, this.stride, this.offset);
        }
    }

    sealed class VertexArray<TVertex>
        where TVertex : struct
    {
        private readonly int handle;

        public VertexArray(VertexBuffer<TVertex> vertexBuffer, ShaderProgram program,
            params VertexAttribute[] attributes)
        {
            // create new vertex array object
            GL.GenVertexArrays(1, out this.handle);

            // bind the object so we can modify it
            this.Bind();

            // bind the vertex buffer object
            vertexBuffer.Bind();

            // set all attributes
            foreach (var attribute in attributes)
                attribute.Set(program);

            // unbind objects to reset state
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Bind()
        {
            // bind for usage (modification or rendering)
            GL.BindVertexArray(this.handle);
        }
    }
}

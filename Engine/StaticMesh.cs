using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Engine
{

    public class StaticMesh : Entity
    {
        public struct Vertex
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 texCoord;
            public Vector3 tangent;
            public Vector3 bitangent;

            public static int SizeInBytes => Vector3.SizeInBytes * 4 + Vector2.SizeInBytes;
        }

        public void GenerateVAO( List<Vertex> vertices, List<uint> indices)
        {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            int VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vertex.SizeInBytes, vertices.ToArray(), BufferUsageHint.StaticDraw);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

            // Positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);
            // Normals
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, sizeof(float) * 3);
            // TexCoords
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, sizeof(float) * 6);
            // Tangents
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, sizeof(float) * 8);
            // Bitangents
            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, sizeof(float) * 11);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void Draw()
        {
            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles , indicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
            GL.ActiveTexture(TextureUnit.Texture0);
        }

    }

}

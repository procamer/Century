using OpenTK.Graphics.OpenGL;

namespace Engine
{
    public class Terrain : Entity
    {
        public Shader shader;
        public Texture texture;

        public Terrain(): base()
        {
            float unit = 500f;
            float[] vertices = {
                -unit, 0f, -unit,
                 unit, 0f, -unit,
                -unit, 0f,  unit,
                 unit, 0f,  unit
            };

            float[] textureCoords =
            {
                0f,0f,
                1f,0f,
                0f,1f,
                1f,1f
            };

            uint[] indices =
            {
                0,2,1,1,2,3
            };

            VAO = GenerateVAO(vertices, textureCoords, indices);
            indicesCount = indices.Length;
            shader = new Shader("TerrainVertex.glsl", "TerrainFragment.glsl");
            texture = new Texture( "grass.png", Util.ModelsPath);
        }

        private int GenerateVAO(float[] vertices, float[] textureCoords, uint[] indices)
        {
            int VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            int VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, textureCoords.Length * sizeof(float), textureCoords, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);

            return VAO;
        }

        public void Draw()
        {
            GL.BindVertexArray(VAO);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
            GL.DrawElements(PrimitiveType.Triangles, indicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
            GL.ActiveTexture(TextureUnit.Texture0);
        }

    }

}

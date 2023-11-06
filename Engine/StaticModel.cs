using Assimp;
using OpenTK;
using System.Collections.Generic;
using System.IO;

namespace Engine
{
    public sealed class StaticModel :  StaticMesh
    {
        public Scene Raw { get; set; }
        public TextureSet TextureSet { get; set; }
        
        public Shader shader;
        public List<StaticMesh> meshes = new List<StaticMesh>();

        public StaticModel(string file)
        {
            shader = new Shader("StaticVertex.glsl", "StaticFragment.glsl");
            Raw = Loader.LoadRaw(file);
            RecursiveNode(Raw.RootNode);
            TextureSet = new TextureSet(Path.GetDirectoryName(file));
            Loader.LoadMaterials(Raw, TextureSet);
        }

        private void RecursiveNode(Node node)
        {
            for (int i = 0; i < node.MeshCount; i++)
            {
                StaticMesh mesh = ProcessMesh(Raw.Meshes[node.MeshIndices[i]]);
                mesh.Transform = node.Transform != null
                    ? Util.ConvertMatrix(node.Transform)
                    :  Matrix4.Identity;
                meshes.Add(mesh);
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                RecursiveNode(node.Children[i]);
            }
        }

        private StaticMesh ProcessMesh(Mesh mesh)
        {
            StaticMesh result = new StaticMesh();
            List<Vertex> vertices = new List<Vertex>();
            List<uint> indices = new List<uint>();

            // vertices
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex = new Vertex()
                {
                    position = Util.ConvertVector3Dto3(mesh.Vertices[i]),
                    normal = Util.ConvertVector3Dto3(mesh.Normals[i]),
                };

                if (mesh.HasTangentBasis && mesh.Tangents.Count == mesh.VertexCount)
                {
                    vertex.tangent = Util.ConvertVector3Dto3(mesh.Tangents[i]);
                    vertex.bitangent = Util.ConvertVector3Dto3(mesh.BiTangents[i]);
                }

                vertex.texCoord = mesh.HasTextureCoords(0)
                    ? Util.ConvertVector3Dto2(mesh.TextureCoordinateChannels[0][i])
                    :  Vector2.Zero;

                vertices.Add(vertex);
            }

            // indices
            for (int i = 0; i < mesh.FaceCount; i++)
            {
                Face face = mesh.Faces[i];
                for (int j = 0; j < face.IndexCount; j++)
                {
                    indices.Add((uint)face.Indices[j]);
                }
            }
            result.indicesCount = indices.Count;
            
            //material index
            if (Raw.HasMaterials)
                result.MaterialIndex = mesh.MaterialIndex;            

            result.GenerateVAO(vertices, indices);
            return result;
        }

    }

}
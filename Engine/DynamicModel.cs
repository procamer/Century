﻿using Assimp;
using OpenTK;
using System.Collections.Generic;
using System.IO;

namespace Engine
{
    public sealed class DynamicModel :  DynamicMesh
    {
        public Scene Raw { get; set; }
        public TextureSet TextureSet { get; set; }
        
        public Animator animator;
        public Shader shader;
        public List<DynamicMesh> meshes = new List<DynamicMesh>();
        
        public DynamicModel(string file)
        {
            shader = new Shader("DynamicVertex.glsl", "DynamicFragment.glsl");
            Raw = Loader.LoadRaw(file);
            RecursiveNode(Raw.RootNode);
            TextureSet = new TextureSet(Path.GetDirectoryName(file));
            Loader.LoadMaterials(Raw, TextureSet);
            animator = new Animator(Raw);
        }

        private void RecursiveNode(Node node)
        {
            for (int i = 0; i < node.MeshCount; i++)
            {
                DynamicMesh mesh = ProcessMesh(Raw.Meshes[node.MeshIndices[i]]);
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

        private DynamicMesh ProcessMesh(Mesh mesh)
        {
            DynamicMesh result = new DynamicMesh();
            List<Vertex> vertices = new List<Vertex>();
            List<uint> indices = new List<uint>();
            List<BoneTransform> boneTransforms = new List<BoneTransform>();

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
            
            // bones
            if (Raw.HasAnimations)
            {
                for (int b = 0; b < mesh.BoneCount; b++)
                {
                    // BoneTransform
                    Bone bone = mesh.Bones[b];
                    boneTransforms.Add(new BoneTransform()
                    {
                        Name = bone.Name,
                        Offset = Util.ConvertMatrix(bone.OffsetMatrix)
                    });

                    // VertexWeight
                    for (int w = 0; w < bone.VertexWeightCount; w++)
                    {
                        VertexWeight vw = bone.VertexWeights[w];
                        int access = vw.VertexID;
                        Vertex vertex = vertices[access];

                        if (vertices[access].boneID.X == 0 && vertices[access].boneWeight.X == 0)
                        {
                            vertex.boneID.X = b;
                            vertex.boneWeight.X = vw.Weight;
                            vertices[access] = vertex;
                        }
                        else if (vertices[access].boneID.Y == 0 && vertices[access].boneWeight.Y == 0)
                        {
                            vertex.boneID.Y = b;
                            vertex.boneWeight.Y = vw.Weight;
                            vertices[access] = vertex;
                        }
                        else if (vertices[access].boneID.Z == 0 && vertices[access].boneWeight.Z == 0)
                        {
                            vertex.boneID.Z = b;
                            vertex.boneWeight.Z = vw.Weight;
                            vertices[access] = vertex;
                        }
                        else
                        {
                            vertex.boneID.W = b;
                            vertex.boneWeight.W = vw.Weight;
                            vertices[access] = vertex;
                        }
                    }
                }
                result.boneTransforms = boneTransforms;
            }

            //material index
            if (Raw.HasMaterials)
                result.MaterialIndex = mesh.MaterialIndex;
            
            result.GenerateVAO(vertices, indices);
            return result;
        }

    }
}
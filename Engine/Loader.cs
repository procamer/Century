using Assimp;
using OpenTK.Graphics.OpenGL;
using System;

namespace Engine
{
    public class Loader
    {       
        public static Scene LoadRaw(string file)
        {
            PostProcessSteps postProcessSteps = Util.GetPostProcessStepsFlags();
            AssimpContext context = new AssimpContext();
            Scene scene = context.ImportFile(file, postProcessSteps);
            if (scene == null )
            {
                Console.WriteLine("Loader::LoadRaw -> File could not be loaded");
                return null;
            }            
            return scene;
        }

        public static void LoadMaterials(Scene raw, TextureSet textureSet)
        {
            if (raw.Materials == null) return;

            foreach (Material material in raw.Materials)
            {
                TextureSlot[] textures = material.GetAllMaterialTextures();

                foreach (TextureSlot tex in textures)
                {
                    string path = tex.FilePath;
                    EmbeddedTexture embeddedSource = null;

                    if (path != null)
                    {
                        if (path.StartsWith("*"))
                        {
                            if (raw.HasTextures && uint.TryParse(path.Substring(1), out uint index) && index < raw.TextureCount)
                            {
                                embeddedSource = raw.Textures[(int)index];
                            }
                        }
                        textureSet.Add(tex.FilePath, embeddedSource);
                    }
                }
            }
        }
                
    }

}

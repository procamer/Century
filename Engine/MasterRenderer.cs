using Assimp;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Engine
{
    public class MasterRenderer
    {
        internal Skybox skybox;
        internal Terrain terrain;

        public MasterRenderer()
        {
            skybox = new Skybox();
            terrain = new Terrain();
            
            GL.ClearColor(0.6f, 0.7f, 0.8f, 1.0f);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        public void Render(List<StaticModel> staticModels, List<DynamicModel> dynamicModels, Camera camera)
        {            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            skybox.shader.SetStart();
            skybox.shader.SetUniform("projectionMatrix", camera.ProjectionMatrix());
            skybox.shader.SetUniform("viewMatrix", new Matrix4(new Matrix3(camera.ViewMatrix())));
            skybox.Draw();
            skybox.shader.SetStop();

            terrain.shader.SetStart();
            terrain.shader.SetUniform("projectionMatrix", camera.ProjectionMatrix());
            terrain.shader.SetUniform("viewMatrix", camera.ViewMatrix());
            terrain.Draw();
            terrain.shader.SetStop();

            foreach (var item in staticModels)
            {
                item.shader.SetStart();
                item.shader.SetUniform("projectionMatrix", camera.ProjectionMatrix());
                item.shader.SetUniform("viewMatrix", camera.ViewMatrix());
                item.shader.SetUniform("transformationMatrix", item.TransformationMatrix());
                for (int i = 0; i < item.meshes.Count; i++)
                {
                    ApplyMaterial(item.shader, item.Raw.Materials[item.meshes[i].MaterialIndex], item.TextureSet);
                    item.meshes[i].Draw();
                }
                item.shader.SetStop();
            }

            foreach (var item in dynamicModels)
            {
                item.shader.SetStart();
                item.shader.SetUniform("projectionMatrix", camera.ProjectionMatrix());
                item.shader.SetUniform("viewMatrix", camera.ViewMatrix());
                item.shader.SetUniform("transformationMatrix", item.TransformationMatrix());
                for (int i = 0; i < item.meshes.Count; i++)
                {
                    item.animator.boneTransform = item.meshes[i].boneTransforms;
                    item.animator.UpdateAnimation();
                    for (int j = 0; j < item.animator.boneTransform.Count; j++)
                        item.shader.SetUniform("boneTransform[" + j + "]", item.animator.boneTransform[j].Transformation);
                    ApplyMaterial(item.shader, item.Raw.Materials[item.meshes[i].MaterialIndex], item.TextureSet);
                    item.meshes[i].Draw();
                }
                item.shader.SetStop();
            }

        }

        private void ApplyMaterial(Shader shader, Material material, TextureSet textureSet)
        {            
            // colorDiffuse
            Vector3 colorDiffuse = new Vector3(1.0f, 0.5f, 0.31f);
            if (material.HasColorDiffuse)
                colorDiffuse = Util.ConvertColor4DtoVector3(material.ColorDiffuse);
            shader.SetUniform("colorDiffuse", colorDiffuse);

            // textureDiffuse (Texture0)
            if (material.HasTextureDiffuse)
            {
                material.GetMaterialTexture(TextureType.Diffuse, 0, out TextureSlot tex);
                Texture texture = textureSet.GetTexture(tex.FilePath);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
                shader.SetUniform("textureDiffuse", 0);
                shader.SetUniform("hasTextureDiffuse", 1); // true
            }
            else
            {
                shader.SetUniform("hasTextureDiffuse", 0); // false
            }

            // colorSpecular
            Vector3 colorSpecular = new Vector3(0.7f, 0.7f, 0.7f);
            if (material.HasColorSpecular)
                colorSpecular = Util.ConvertColor4DtoVector3(material.ColorSpecular);
            shader.SetUniform("colorSpecular", colorSpecular);

            // textureSpecular (Texture1)
            if (material.HasTextureSpecular)
            {
                material.GetMaterialTexture(TextureType.Specular, 0, out TextureSlot tex);
                Texture texture = textureSet.GetTexture(tex.FilePath);
                if (texture != null)
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
                    shader.SetUniform("textureSpecular", 1);
                    shader.SetUniform("hasTextureSpecular", 1);
                }
            }
            else
            {
                shader.SetUniform("hasTextureSpecular", 0);
            }

            // textureHeight (Texture2)
            if (material.HasTextureHeight)
            {
                material.GetMaterialTexture(TextureType.Height, 0, out TextureSlot tex);
                Texture texture = textureSet.GetTexture(tex.FilePath);
                if (texture != null)
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
                    shader.SetUniform("textureNormal", 2);
                    shader.SetUniform("hasTextureNormal", 1);
                }
            }
            else
            {
                shader.SetUniform("hasTextureNormal", 0);
            }

            // Clear textures
            GL.Disable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0);
        }

    }
}

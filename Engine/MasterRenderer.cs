using Assimp;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    public class MasterRenderer
    {
        internal Skybox skybox;
        internal Terrain terrain;

        // Points toward the sun in the skybox (back face, ~37 degrees above the horizon)
        public Vector3 LightDirection { get; set; } = new Vector3(-0.41f, 0.55f, -0.73f).Normalized();
        public Vector3 LightColor { get; set; } = new Vector3(0.78f, 0.74f, 0.66f);
        // Hemisphere ambient: cool sky light from above, darker light bounced off the ground from below
        public Vector3 AmbientSky { get; set; } = new Vector3(0.36f, 0.40f, 0.48f);
        public Vector3 AmbientGround { get; set; } = new Vector3(0.20f, 0.18f, 0.16f);

        // Blob shadows on the terrain; MaxShadowCasters must match MAX_SHADOW_CASTERS in TerrainFragment.glsl
        const int MaxShadowCasters = 16;
        public float ShadowDarkness { get; set; } = 0.55f;
        public float ShadowFadeHeight { get; set; } = 20f;

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
            ApplyLighting(terrain.shader);
            ApplyShadowCasters(terrain.shader, staticModels, dynamicModels);
            terrain.Draw();
            terrain.shader.SetStop();

            foreach (var item in staticModels)
            {
                item.shader.SetStart();
                ApplyLighting(item.shader);
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
                ApplyLighting(item.shader);
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

        private void ApplyLighting(Shader shader)
        {
            shader.SetUniform("lightDirection", LightDirection);
            shader.SetUniform("lightColor", LightColor);
            shader.SetUniform("ambientSky", AmbientSky);
            shader.SetUniform("ambientGround", AmbientGround);
        }

        private void ApplyShadowCasters(Shader shader, List<StaticModel> staticModels, List<DynamicModel> dynamicModels)
        {
            int count = 0;
            foreach (Entity entity in staticModels.Concat<Entity>(dynamicModels))
            {
                if (entity.ShadowRadius <= 0 || count == MaxShadowCasters) continue;

                // Higher up (e.g. mid-jump) the shadow gets smaller and fainter, which shows the height
                float lift = MathHelper.Clamp(entity.Position.Y / ShadowFadeHeight, 0f, 1f);
                float radius = entity.ShadowRadius * (1f - 0.4f * lift);
                float darkness = ShadowDarkness * (1f - 0.7f * lift);

                shader.SetUniform("shadowCasters[" + count + "]",
                    new Vector4(entity.Position.X, entity.Position.Z, radius, darkness));
                count++;
            }
            shader.SetUniform("shadowCasterCount", count);
        }

        private void ApplyMaterial(Shader shader, Material material, TextureSet textureSet)
        {            
            // colorDiffuse
            Vector3 colorDiffuse = new Vector3(1.0f, 0.5f, 0.31f);
            if (material.HasColorDiffuse)
                colorDiffuse = Util.ConvertColor4DtoVector3(material.ColorDiffuse);
            shader.SetUniform("colorDiffuse", colorDiffuse);

            // textureDiffuse (Texture0)
            Texture diffuse = null;
            if (material.HasTextureDiffuse)
            {
                material.GetMaterialTexture(TextureType.Diffuse, 0, out TextureSlot tex);
                diffuse = textureSet.GetTexture(tex.FilePath);
            }
            if (diffuse != null)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, diffuse.TextureID);
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
            bool specularBound = false;
            if (material.HasTextureSpecular)
            {
                material.GetMaterialTexture(TextureType.Specular, 0, out TextureSlot tex);
                Texture texture = textureSet.GetTexture(tex.FilePath);
                if (texture != null)
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
                    shader.SetUniform("textureSpecular", 1);
                    specularBound = true;
                }
            }
            shader.SetUniform("hasTextureSpecular", specularBound ? 1 : 0);

            // textureHeight (Texture2)
            bool normalBound = false;
            if (material.HasTextureHeight)
            {
                material.GetMaterialTexture(TextureType.Height, 0, out TextureSlot tex);
                Texture texture = textureSet.GetTexture(tex.FilePath);
                if (texture != null)
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
                    shader.SetUniform("textureNormal", 2);
                    normalBound = true;
                }
            }
            shader.SetUniform("hasTextureNormal", normalBound ? 1 : 0);

            // Clear textures
            GL.Disable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0);
        }

    }
}

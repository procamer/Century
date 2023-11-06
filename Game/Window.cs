using Assimp;
using Engine;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Camera = Engine.Camera;

namespace Game
{
    public class Window : GameWindow
    {
        FPSTracker fpsTracker = new FPSTracker();
        Camera camera;
        MasterRenderer renderer;
        Player player;
        List<DynamicModel> dynamicModels = new List<DynamicModel>();
        List<StaticModel> staticModels = new List<StaticModel>();

        public Window(int width = 800, int height = 600, string title = "Game Engine")
            : base(width, height, GraphicsMode.Default, title, GameWindowFlags.Default, DisplayDevice.Default, 0, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            WindowState = WindowState.Minimized;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // tree - (origin)
            StaticModel tree = new StaticModel(Util.ModelsPath + "Tree\\tree.obj")
            {
                Scale = 10f
            };
            staticModels.Add(tree);

            // lara model
            DynamicModel lara = new DynamicModel(Util.ModelsPath + "Lara\\T-Pose.fbx")
            {
                Position = new Vector3(-50f, 0f, -10f),
                Scale = 0.05f,
            };
            // lara animations
            Scene anim = Loader.LoadRaw(Util.ModelsPath + "Lara\\idle.fbx");
            lara.animator.AddAnimation("idle", anim.Animations[0]);

            anim = Loader.LoadRaw(Util.ModelsPath + "Lara\\walking.fbx");
            lara.animator.AddAnimation("walk", anim.Animations[0]);

            anim = Loader.LoadRaw(Util.ModelsPath + "Lara\\Jump.fbx");
            lara.animator.AddAnimation("jump", anim.Animations[0]);

            anim = Loader.LoadRaw(Util.ModelsPath + "Lara\\Running.fbx");
            lara.animator.AddAnimation("run", anim.Animations[0]);

            anim = Loader.LoadRaw(Util.ModelsPath + "Lara\\Arms Hip Hop Dance.fbx");
            lara.animator.AddAnimation("dance", anim.Animations[0]);

            lara.animator.ActiveAnimation = lara.animator.animationDict["idle"];
            dynamicModels.Add(lara);

            // knight model
            DynamicModel knight = new DynamicModel(Util.ModelsPath + "Solus the knight\\solus_the_knight.fbx")
            {
                Position = new Vector3(50f, 0f, -10f),
                Scale = 0.2f
            };
            //knight animations
            knight.animator.AddAnimation("idle", knight.Raw.Animations[14]);
            knight.animator.AddAnimation("walk", knight.Raw.Animations[40]);
            knight.animator.AddAnimation("jump", knight.Raw.Animations[22]);
            anim = Loader.LoadRaw(Util.ModelsPath + "Solus the knight\\Jogging.fbx");
            knight.animator.AddAnimation("run", anim.Animations[0]);
            anim = Loader.LoadRaw(Util.ModelsPath + "Solus the knight\\Salute.fbx");
            knight.animator.AddAnimation("dance", anim.Animations[0]);

            knight.animator.ActiveAnimation = 0;
            dynamicModels.Add(knight);

            player = new Player(lara);
            camera = new Camera(player);
            renderer = new MasterRenderer();

            WindowState = WindowState.Fullscreen;
            CursorVisible = false;
            OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            fpsTracker.Update();
            foreach (var model in dynamicModels) model.animator.Update(fpsTracker.LastFrameDelta);
            player.Update(fpsTracker.LastFrameDelta);
            camera.Update();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            renderer.Render(staticModels, dynamicModels, camera);
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
            camera.AspectRatio = Width / (float)Height;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape) { Exit(); }

            if (e.Key == Key.Number1)
            {
                player = new Player(dynamicModels[0]);
                camera = new Camera(player);
            }

            if (e.Key == Key.Number2)
            {
                player = new Player(dynamicModels[1]);
                camera = new Camera(player);
            }

        }

    }

}

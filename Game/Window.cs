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
        List<Player> players = new List<Player>();
        int activePlayer = 0;
        List<DynamicModel> dynamicModels = new List<DynamicModel>();
        List<StaticModel> staticModels = new List<StaticModel>();
        List<Collider> colliders = new List<Collider>();

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
                Scale = 10f,
                ShadowRadius = 30f // the canopy's footprint
            };
            staticModels.Add(tree);
            // trunk measured from tree.obj in model space (base center -0.07,0.37 / radius 0.84 / height 6.2);
            // the leaves start above the characters' reach, so only the trunk collides
            colliders.Add(new CylinderCollider(
                tree.Position + new Vector3(-0.07f, 0f, 0.37f) * tree.Scale,
                0.84f * tree.Scale,
                6.2f * tree.Scale));

            // lara model
            DynamicModel lara = new DynamicModel(Util.ModelsPath + "Lara\\T-Pose.fbx")
            {
                Position = new Vector3(-50f, 0f, -10f),
                Scale = 0.05f,
                ShadowRadius = 9f
            };
            // lara animations
            Scene anim = Loader.LoadRaw(Util.ModelsPath + "Lara\\idle.fbx");
            lara.animator.AddAnimation("idle", anim.Animations[0]);

            anim = Loader.LoadRaw(Util.ModelsPath + "Lara\\walking.fbx");
            lara.animator.AddAnimation("walk", anim.Animations[0]);

            anim = Loader.LoadRaw(Util.ModelsPath + "Lara\\Jump.fbx");
            // the clip lifts the hips itself; strip that so the Player's jump physics does the lifting
            Animator.MakeInPlace(anim.Animations[0], "mixamorig:Hips_$AssimpFbx$_Translation");
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
                Scale = 0.2f,
                ShadowRadius = 8f
            };
            //knight animations
            knight.animator.AddAnimation("idle", knight.Raw.Animations[14]);
            knight.animator.AddAnimation("walk", knight.Raw.Animations[40]);
            knight.animator.AddAnimation("jump", knight.Raw.Animations[20]); // anim_jump_in_place
            anim = Loader.LoadRaw(Util.ModelsPath + "Solus the knight\\Jogging.fbx");
            knight.animator.AddAnimation("run", anim.Animations[0]);
            anim = Loader.LoadRaw(Util.ModelsPath + "Solus the knight\\Salute.fbx");
            knight.animator.AddAnimation("dance", anim.Animations[0]);

            knight.animator.ActiveAnimation = 0;
            dynamicModels.Add(knight);

            // jump timings measured from each character's jump clip
            players.Add(new Player(lara, new JumpSettings { TakeoffTime = 0.75f, AirTime = 0.55f, Height = 7f })
            {
                CollisionRadius = 5f,
                CollisionHeight = 46f
            });
            players.Add(new Player(knight, new JumpSettings { TakeoffTime = 0.4f, AirTime = 0.5f, Height = 5f })
            {
                CollisionRadius = 4.5f,
                CollisionHeight = 36f
            });
            foreach (Player p in players) colliders.Add(p.Body); // characters block each other
            camera = new Camera(players[activePlayer]);
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
            for (int i = 0; i < players.Count; i++)
                players[i].Update(fpsTracker.LastFrameDelta, i == activePlayer, colliders);
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

            if (e.Key == Key.Number1) activePlayer = 0;
            if (e.Key == Key.Number2) activePlayer = 1;
            camera.Player = players[activePlayer];

        }

    }

}

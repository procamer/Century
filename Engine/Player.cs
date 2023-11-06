using OpenTK;
using OpenTK.Input;
using System;

namespace Engine
{
    public class Player
    {
        public DynamicModel model;

        const int WALK_SPEED = 35;
        const int RUN_SPEED = 70;
        const float TURN_SPEED = 160;
        const float JUMP_POWER = 10;
        const float GRAVITY = -50;

        float currentRunSpeed = 0;
        float currentTurnSpeed = 0;
        float upwardsSpeed = 0;

        bool isInAir = false;

        public Player(DynamicModel model)
        {
            this.model = model;
        }

        public void Update( double delta)
        {
            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsAnyKeyDown)
            {
                // Rotate
                if (keyboard.IsKeyDown(Key.D))
                {
                    model.animator.ActiveAnimation = model.animator.animationDict["walk"];
                    currentTurnSpeed = -TURN_SPEED;
                    currentRunSpeed = WALK_SPEED;
                }

                if (keyboard.IsKeyDown(Key.A))
                {
                    model.animator.ActiveAnimation = model.animator.animationDict["walk"];
                    currentTurnSpeed = TURN_SPEED;
                    currentRunSpeed = WALK_SPEED;
                }

                if (keyboard.IsKeyUp(Key.D)&& keyboard.IsKeyUp(Key.A))
                {
                    model.animator.ActiveAnimation = model.animator.animationDict["idle"];
                    currentTurnSpeed = 0;
                    currentRunSpeed = 0;
                }

                // Walk
                if (keyboard.IsKeyDown(Key.W))
                {
                    model.animator.ActiveAnimation = model.animator.animationDict["walk"];
                    currentRunSpeed = WALK_SPEED;
                }

                // Run
                if (keyboard.IsKeyDown(Key.W) && keyboard.IsKeyDown(Key.ControlRight))
                {
                model.animator.ActiveAnimation = model.animator.animationDict["run"];
                currentRunSpeed = RUN_SPEED;
                }

                // Dance :)
                if (keyboard.IsKeyDown(Key.S))
                {
                    model.animator.ActiveAnimation = model.animator.animationDict["dance"];
                    currentRunSpeed = 0;
                }

                // Jump
                if (keyboard.IsKeyDown(Key.Space))
                {
                    if (!isInAir)
                    {
                        model.animator.ActiveAnimation = model.animator.animationDict["jump"]; 
                        upwardsSpeed = JUMP_POWER;
                        currentTurnSpeed = 0;
                        currentRunSpeed = 0; 
                        isInAir = true;
                    }
                }

                if (model.Position.Y < 0)
                {
                    model.Position = new Vector3(model.Position.X, 0.0f, model.Position.Z);
                    upwardsSpeed = 0;
                    isInAir = false;
                }

                float distance = currentRunSpeed * (float)delta;
                float dx = (float)(distance * Math.Sin(MathHelper.DegreesToRadians(model.Rotation.Y)));
                float dz = (float)(distance * Math.Cos(MathHelper.DegreesToRadians(model.Rotation.Y)));
                upwardsSpeed += GRAVITY * (float)delta;
                model.Move(new Vector3(dx  , upwardsSpeed * (float)delta, dz));
                model.Rotate(new Vector3(0, currentTurnSpeed * (float)delta, 0));
            }
            else
            {
                model.animator.ActiveAnimation = model.animator.animationDict["idle"]; 
                currentTurnSpeed = 0f;
                if (!isInAir)
                {                    
                    currentRunSpeed -= 0.9f;
                    if (currentRunSpeed < 0) currentRunSpeed = 0;                                        
                }
            }
        }
    }
}

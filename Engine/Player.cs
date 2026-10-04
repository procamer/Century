using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;

namespace Engine
{
    // Jump timing taken from the character's jump clip, so the physics matches what the animation shows
    public class JumpSettings
    {
        // Clip time (seconds) at which the feet leave the ground
        public float TakeoffTime { get; set; }
        // Seconds between takeoff and landing
        public float AirTime { get; set; }
        // Peak height in world units
        public float Height { get; set; }
    }

    public class Player
    {
        public DynamicModel model;
        public JumpSettings jump;

        // Upright cylinder that follows the model, so other characters collide with this one
        public CylinderCollider Body { get; }

        // Size of Body in world units
        public float CollisionRadius
        {
            get { return Body.Radius; }
            set { Body.Radius = value; }
        }
        public float CollisionHeight
        {
            get { return Body.Height; }
            set { Body.Height = value; }
        }

        const int WALK_SPEED = 35;
        const int RUN_SPEED = 70;
        const float TURN_SPEED = 160;
        // Running jumps skip most of the crouch so the character doesn't slide forward while winding up
        const float RUNNING_JUMP_WINDUP = 0.15f;

        float currentRunSpeed = 0;
        float currentTurnSpeed = 0;
        float upwardsSpeed = 0;

        bool isJumping = false; // jump clip is playing
        bool isInAir = false;   // feet are off the ground
        bool hasLanded = false;
        bool spaceWasDown = false;

        public Player(DynamicModel model, JumpSettings jump)
        {
            this.model = model;
            this.jump = jump;
            Body = new CylinderCollider(model.Position, 4f, 40f);
        }

        // Inactive players still need updating so a jump in progress can finish
        public void Update(double delta, bool acceptInput = true, IList<Collider> colliders = null)
        {
            float dt = (float)delta;
            KeyboardState keyboard = acceptInput ? Keyboard.GetState() : default(KeyboardState);

            // Jump on the press, not while held, so holding Space doesn't bounce on every landing
            bool spaceDown = keyboard.IsKeyDown(Key.Space);
            bool spacePressed = spaceDown && !spaceWasDown;
            spaceWasDown = spaceDown;

            if (isJumping)
                UpdateJump(keyboard);
            else if (spacePressed)
                StartJump();
            else
                HandleGroundInput(keyboard);

            float distance = currentRunSpeed * dt;
            float dx = (float)(distance * Math.Sin(MathHelper.DegreesToRadians(model.Rotation.Y)));
            float dz = (float)(distance * Math.Cos(MathHelper.DegreesToRadians(model.Rotation.Y)));
            float dy = 0f;
            if (isInAir)
            {
                // Gravity that lands the character exactly AirTime seconds after takeoff
                upwardsSpeed += -8f * jump.Height / (jump.AirTime * jump.AirTime) * dt;
                dy = upwardsSpeed * dt;
            }
            model.Move(new Vector3(dx, dy, dz));
            model.Rotate(new Vector3(0, currentTurnSpeed * dt, 0));

            if (colliders != null)
            {
                Vector3 position = model.Position;
                foreach (Collider collider in colliders)
                {
                    if (collider == Body) continue;
                    collider.Resolve(ref position, CollisionRadius, CollisionHeight);
                }
                model.Position = position;
            }

            if (isInAir && model.Position.Y < 0)
            {
                model.Position = new Vector3(model.Position.X, 0.0f, model.Position.Z);
                upwardsSpeed = 0;
                currentRunSpeed = 0;
                isInAir = false;
                hasLanded = true;
            }

            Body.Base = model.Position;
        }

        private void StartJump()
        {
            // Horizontal speed carries over from the ground; steering is locked until landing
            float startTime = currentRunSpeed > 0 ? Math.Max(0f, jump.TakeoffTime - RUNNING_JUMP_WINDUP) : 0f;
            model.animator.PlayOnce("jump", startTime);
            currentTurnSpeed = 0;
            isJumping = true;
            hasLanded = false;
        }

        private void UpdateJump(KeyboardState keyboard)
        {
            // The clip cursor is the jump clock, so takeoff stays in sync with the animation
            double clipTime = model.animator.Cursor;

            if (!isInAir && !hasLanded && clipTime >= jump.TakeoffTime)
            {
                // Initial speed that reaches Height at the midpoint of AirTime
                upwardsSpeed = 4f * jump.Height / jump.AirTime;
                isInAir = true;
            }

            // After landing, moving cuts the recovery short; otherwise the clip plays out
            if (hasLanded && (HasMoveInput(keyboard) || clipTime >= model.animator.AnimationDuration))
                isJumping = false;
        }

        private void HandleGroundInput(KeyboardState keyboard)
        {
            string animation = "idle";
            currentTurnSpeed = 0;
            currentRunSpeed = 0;

            // Rotate
            if (keyboard.IsKeyDown(Key.D))
            {
                animation = "walk";
                currentTurnSpeed = -TURN_SPEED;
                currentRunSpeed = WALK_SPEED;
            }

            if (keyboard.IsKeyDown(Key.A))
            {
                animation = "walk";
                currentTurnSpeed = TURN_SPEED;
                currentRunSpeed = WALK_SPEED;
            }

            // Walk
            if (keyboard.IsKeyDown(Key.W))
            {
                animation = "walk";
                currentRunSpeed = WALK_SPEED;
            }

            // Run
            if (keyboard.IsKeyDown(Key.W) && keyboard.IsKeyDown(Key.ControlRight))
            {
                animation = "run";
                currentRunSpeed = RUN_SPEED;
            }

            // Dance :)
            if (keyboard.IsKeyDown(Key.S))
            {
                animation = "dance";
                currentRunSpeed = 0;
            }

            model.animator.Play(animation);
        }

        private static bool HasMoveInput(KeyboardState keyboard)
        {
            return keyboard.IsKeyDown(Key.W) || keyboard.IsKeyDown(Key.A)
                || keyboard.IsKeyDown(Key.S) || keyboard.IsKeyDown(Key.D);
        }
    }
}

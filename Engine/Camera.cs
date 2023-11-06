using OpenTK;
using OpenTK.Input;
using System;

namespace Engine
{
    public class Camera
    {
        public Player Player { get; private set; }
        public float AspectRatio { get; set; }
        public static float Fov { get; set; }
        public static float Near { get; set; }
        public static float Far { get; set; }
        public float DistanceFromPlayer { get; set; }

        public Vector3 Position { get; set; }
        public float Pitch = 30;
        public float Yaw;
        
        float angleAroundPlayer;
        private MouseState mousePrevious;
        private MouseState mousePreviousAngle;

        public Camera(Player player)
        {
            Position = new Vector3(0.0f, 20.0f, 10.0f);
            Player = player;
            DistanceFromPlayer = 100f;
            AspectRatio = 4f / 3f;
            Fov = 70.0f;
            Near = 0.1f;
            Far = 1000.0f;
        }

        public void Update()
        {
            CalculateZoom();
            CalculateAngleAroundPlayer();
            float horizontalDistance = CalculateHorizontalDistance();
            float verticalDistance = CalculateVerticalDistance();
            CalculatePitch();
            CalculateCameraPosition(horizontalDistance, verticalDistance);
            Yaw = Player.model.Rotation.Y + angleAroundPlayer - 180;
        }

        private void CalculateAngleAroundPlayer()
        {
            MouseState mouse = Mouse.GetState();
            int delta = mouse.X - mousePreviousAngle.X;
            if (mouse.IsButtonDown(MouseButton.Right))
            {
                angleAroundPlayer -= delta * 0.1f;
            }
            mousePreviousAngle = mouse;
        }

        private float CalculateHorizontalDistance()
        {
            return (float)(DistanceFromPlayer * Math.Cos(MathHelper.DegreesToRadians(Pitch)));
        }

        private float CalculateVerticalDistance()
        {
            return (float)(DistanceFromPlayer * Math.Sin(MathHelper.DegreesToRadians(Pitch)));
        }

        private void CalculateZoom()
        {
            MouseState mouse = Mouse.GetState();
            var delta = mouse.WheelPrecise - mousePrevious.WheelPrecise;
            if (delta != 0)
            {
                float zoomLevel = delta * 10f;
                DistanceFromPlayer -= zoomLevel;
                mousePrevious = mouse;
            }
        }

        private void CalculatePitch()
        {
            var mouse = Mouse.GetState();
            var delta = mouse.Y - mousePrevious.Y;
            if (mouse.IsButtonDown(MouseButton.Right))
            {
                Pitch -= delta * 0.1f;
            }
            Pitch = MathHelper.Clamp(Pitch, 2, 89);
            mousePrevious = mouse;
        }

        private void CalculateCameraPosition(float horizontalDistance, float verticalDistance)
        {
            //float theta = Player.model.rY + angleAroundPlayer;    // Track Camera
            float theta = angleAroundPlayer;                                     // Side Camera
            float offsetX = (float)(horizontalDistance * Math.Sin(MathHelper.DegreesToRadians(theta)));
            float offsetZ = (float)(horizontalDistance * Math.Cos(MathHelper.DegreesToRadians(theta)));
            Position = new Vector3(Player.model.Position.X - offsetX, verticalDistance, Player.model.Position.Z - offsetZ);
            //Position.Y = Player.model.Position.Y + verticalDistance;
        }

        public Matrix4 ViewMatrix()
        {
            Vector3 target = new Vector3(Player.model.Position.X, Player.model.Position.Y + 10F, Player.model.Position.Z);
            return Matrix4.LookAt(Position, target, Vector3.UnitY);
        }

        public Matrix4 ProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), AspectRatio, Near, Far);
        }











        //public float Distance { get; set; }

        //public CameraTPS(Vector3 position)
        //{
        //    Position = position;
        //}

        //public Matrix4 GetViewMatrix()
        //{
        //    Matrix4 viewMatrix;
        //    viewMatrix = Matrix4.LookAt(Position, Position + Front, Up);
        //    return viewMatrix;
        //}

        //public void MouseMove(int xDelta, int yDelta)
        //{
        //    if (xDelta == 0 && yDelta == 0) return;
        //    const float sensitivity = 0.2f;
        //    Yaw += xDelta * sensitivity;
        //    Pitch -= yDelta * sensitivity;
        //}

        //public void Scroll(float z)
        //{
        //    const float zoomSpeed = 0.02f;
        //    Fov -= z * zoomSpeed;
        //}

        public void SetTarget(Vector3 pivot)
        {
        }

        public void Pan(float x, float y)
        {
        }

        public void MouseMove(int x, int y)
        {
        }

        public void Scroll(float z)
        {
        }

    }
}

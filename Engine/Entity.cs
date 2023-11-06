using OpenTK;

namespace Engine
{
    public class Entity
    {
        public int MaterialIndex { get; set; }
        public int VAO;
        public int indicesCount;
        public int textureDiffuseID;
        public int textureSpecularID;
        public int textureNormalID;

        public Matrix4 Transform
        {
            get
            {
                transform =   TransformationMatrix();
                return transform;
            }
            set
            {
                transform = value; 
            } 
        }

        private Matrix4 transform = Matrix4.Identity; 

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation {  get; set; } = Vector3.Zero;
        public float Scale { get; set; } = 1;

        public void Move( Vector3 position)
        {
            Position += position;
        }

        public void Rotate( Vector3 rotation)
        {
            Rotation += rotation;
        }

        public Matrix4 TransformationMatrix()
        {
            return _ = Matrix4.CreateScale(Scale)
                * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z))
                * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y))
                * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X))
                * Matrix4.CreateTranslation(Position);
        }

    }

}

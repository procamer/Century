using Assimp;
using OpenTK;
using Quaternion = OpenTK.Quaternion;

namespace Engine
{
    public static class Util
    {
        public static string AssetsPath = "..\\..\\..\\Assets\\";
        public static string SkyboxPath = AssetsPath + "Skybox\\";
        public static string ModelsPath = AssetsPath + "Models\\";

        public static PostProcessSteps GetPostProcessStepsFlags()
        {
            return PostProcessSteps.Triangulate |
                PostProcessSteps.FlipUVs |
                PostProcessSteps.CalculateTangentSpace |
                PostProcessSteps.GenerateSmoothNormals;
        }

        public static Matrix4 ConvertMatrix(Matrix4x4 matrix)
        {
            return new Matrix4(
                matrix.A1, matrix.B1, matrix.C1, matrix.D1,
                matrix.A2, matrix.B2, matrix.C2, matrix.D2,
                matrix.A3, matrix.B3, matrix.C3, matrix.D3,
                matrix.A4, matrix.B4, matrix.C4, matrix.D4
            );
        }

        public static Vector3 ConvertVector3Dto3(Vector3D vector3D) => new Vector3
        {
            X = vector3D.X,
            Y = vector3D.Y,
            Z = vector3D.Z
        };

        public static Vector2 ConvertVector3Dto2(Vector3D vector3D) => new Vector2
        {
            X = vector3D.X,
            Y = vector3D.Y
        };

        public static Vector3 ConvertColor4DtoVector3(Color4D color4D) => new Vector3
        {
            X = color4D.R,
            Y = color4D.G,
            Z = color4D.B,
        };

        public static Quaternion ConvertQuaternion(Assimp.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

    }

}

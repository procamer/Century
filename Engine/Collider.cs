using OpenTK;
using System;

namespace Engine
{
    // Simple shapes that keep characters out of the scenery. A character is treated as an upright cylinder.
    // Collisions are pushed apart on the ground plane (XZ); height only decides whether two shapes overlap at all.
    public abstract class Collider
    {
        // Moves a character cylinder (bottom-center position) out of this shape. Returns true if it was touching.
        public abstract bool Resolve(ref Vector3 position, float radius, float height);

        protected static bool OverlapsHeight(float bottomA, float heightA, float bottomB, float heightB)
        {
            return bottomA < bottomB + heightB && bottomB < bottomA + heightA;
        }
    }

    // Upright cylinder, e.g. a tree trunk or a pillar
    public class CylinderCollider : Collider
    {
        public Vector3 Base { get; set; } // bottom-center
        public float Radius { get; set; }
        public float Height { get; set; }

        public CylinderCollider(Vector3 basePosition, float radius, float height)
        {
            Base = basePosition;
            Radius = radius;
            Height = height;
        }

        public override bool Resolve(ref Vector3 position, float radius, float height)
        {
            if (!OverlapsHeight(position.Y, height, Base.Y, Height)) return false;

            Vector2 offset = new Vector2(position.X - Base.X, position.Z - Base.Z);
            float minDistance = Radius + radius;
            float distanceSquared = offset.LengthSquared;
            if (distanceSquared >= minDistance * minDistance) return false;

            // Push straight out from the axis; this lets the character slide around the shape
            float distance = (float)Math.Sqrt(distanceSquared);
            Vector2 normal = distance > 1e-5f ? offset / distance : Vector2.UnitX;
            position.X = Base.X + normal.X * minDistance;
            position.Z = Base.Z + normal.Y * minDistance;
            return true;
        }
    }

    // Axis-aligned box, e.g. a wall or a crate
    public class BoxCollider : Collider
    {
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }

        public BoxCollider(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public override bool Resolve(ref Vector3 position, float radius, float height)
        {
            if (!OverlapsHeight(position.Y, height, Min.Y, Max.Y - Min.Y)) return false;

            // Closest point of the box footprint to the character's axis
            float closestX = MathHelper.Clamp(position.X, Min.X, Max.X);
            float closestZ = MathHelper.Clamp(position.Z, Min.Z, Max.Z);
            Vector2 offset = new Vector2(position.X - closestX, position.Z - closestZ);
            float distanceSquared = offset.LengthSquared;

            if (distanceSquared > 1e-10f)
            {
                // Axis is outside the footprint: push away from the nearest edge or corner
                if (distanceSquared >= radius * radius) return false;
                float distance = (float)Math.Sqrt(distanceSquared);
                position.X = closestX + offset.X / distance * radius;
                position.Z = closestZ + offset.Y / distance * radius;
                return true;
            }

            // Axis is inside the footprint: leave through the nearest side
            float left = position.X - Min.X, right = Max.X - position.X;
            float back = position.Z - Min.Z, front = Max.Z - position.Z;
            float nearest = Math.Min(Math.Min(left, right), Math.Min(back, front));
            if (nearest == left) position.X = Min.X - radius;
            else if (nearest == right) position.X = Max.X + radius;
            else if (nearest == back) position.Z = Min.Z - radius;
            else position.Z = Max.Z + radius;
            return true;
        }
    }
}

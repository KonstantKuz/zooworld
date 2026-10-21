using Core.Navigation.Abstract;
using UnityEngine;

namespace Game.Navigation
{
    public sealed class CameraPlayfieldService : IPlayfieldService
    {
        private readonly Camera camera;

        public CameraPlayfieldService(Camera camera)
        {
            this.camera = camera;
        }

        public bool Contains(Vector3 position)
        {
            (Vector2 center, Vector2 half) = GetArea();
            return Mathf.Abs(position.x - center.x) <= half.x && Mathf.Abs(position.z - center.y) <= half.y;
        }

        public Vector3 RandomPoint(float inset)
        {
            (Vector2 center, Vector2 half) = GetArea();
            float x = Random.Range(-half.x + inset, half.x - inset);
            float z = Random.Range(-half.y + inset, half.y - inset);
            return new Vector3(center.x + x, 0f, center.y + z);
        }

        private (Vector2 center, Vector2 half) GetArea()
        {
            Vector3 bottomLeft = GroundPoint(0f, 0f);
            Vector3 bottomRight = GroundPoint(1f, 0f);
            Vector3 topLeft = GroundPoint(0f, 1f);
            Vector3 topRight = GroundPoint(1f, 1f);

            float minX = Mathf.Max(bottomLeft.x, topLeft.x);
            float maxX = Mathf.Min(bottomRight.x, topRight.x);
            float minZ = Mathf.Max(bottomLeft.z, bottomRight.z);
            float maxZ = Mathf.Min(topLeft.z, topRight.z);
            return (new Vector2((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f),
                new Vector2((maxX - minX) * 0.5f, (maxZ - minZ) * 0.5f));
        }

        private Vector3 GroundPoint(float viewportX, float viewportY)
        {
            Ray ray = camera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0f));
            new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float distance);
            return ray.GetPoint(distance);
        }
    }
}

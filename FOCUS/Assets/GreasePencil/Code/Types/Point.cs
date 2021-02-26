using UnityEngine;

namespace GreasePencil
{
    [System.Serializable]
    public struct Point
    {
        public Point(GreasePencilHit point)
        {
            position = point.position;
            normal = point.normal;
            markedForRemoval = false;
        }

        public Point(Point point)
        {
            position = point.position;
            normal = point.normal;
            markedForRemoval = false;
        }

        public Point(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = normal;
            markedForRemoval = false;
        }

        public static Point LerpUnclamped(Point a, Point b, float alpha)
        {
            return new Point(
                Vector3.LerpUnclamped(a.position, b.position, alpha), 
                Vector3.SlerpUnclamped(a.normal, b.normal, alpha));
        }

        public Vector3 position;
        public Vector3 normal;

        public bool markedForRemoval;

        public void SetPosition(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = position;
        }
    }
}
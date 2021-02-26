using UnityEngine;

namespace GreasePencil
{
    public struct GreasePencilHit
    {
        public GreasePencilHit(Vector3 position, Vector3 normal, Vector3 origin)
        {
            this.position = position;
            this.normal = normal;
            distance = Vector3.Distance(position, origin);
        }

        public Vector3 position;
        public Vector3 normal;
        public float distance;
    }
}
using UnityEngine;

namespace GreasePencil
{
    public class GreasePencilRuntimeColliderSurface : GreasePencilRuntimeSurface
    {
        Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        public override bool Raycast(Camera sourceCamera, Ray ray, out GreasePencilHit hit)
        {
            RaycastHit rayHit;

            if (_collider.Raycast(ray, out rayHit, float.PositiveInfinity))
            {
                hit = new GreasePencilHit(rayHit.point, rayHit.normal, ray.origin);
                return true;
            }

            hit = default(GreasePencilHit);
            return false;
        }
    }
}
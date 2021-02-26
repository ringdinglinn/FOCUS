using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreasePencil
{
    public class GreasePencilRuntimePlanarSurface : GreasePencilRuntimeSurface
    {
        [SerializeField, Tooltip("When disabled, casts against the camera's plane")]
        EnabledVector3 _planeNormal = new EnabledVector3(Vector3.forward, true);

        [SerializeField]
        Space _space;
        
        public override bool Raycast(Camera sourceCamera, Ray ray, out GreasePencilHit hit)
        {
            Plane plane;

            if (_planeNormal.enabled)
            {
                if (_space == Space.Self)
                    plane = new Plane(transform.TransformDirection(_planeNormal.value), transform.position);
                else
                    plane = new Plane(_planeNormal.value, transform.position);
            }
            else
                plane = new Plane(sourceCamera.transform.forward, transform.position);

            float distance;

            if (plane.Raycast(ray, out distance))
            {
                hit = new GreasePencilHit(ray.GetPoint(distance), plane.normal, ray.origin);
                return true;
            }

            hit = default(GreasePencilHit);
            return false;
        }
    }
}
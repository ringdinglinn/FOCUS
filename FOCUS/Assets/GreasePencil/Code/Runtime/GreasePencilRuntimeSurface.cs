using System;
using System.Collections.Generic;
using UnityEngine;

namespace GreasePencil
{
    public abstract class GreasePencilRuntimeSurface : MonoBehaviour
    {
        static List<GreasePencilRuntimeSurface> _allSurfaces = new List<GreasePencilRuntimeSurface>();
        public static List<GreasePencilRuntimeSurface> AllSurfaces { get { return _allSurfaces; } }

        protected virtual void OnEnable()
        {
            if (!_allSurfaces.Contains(this))
                _allSurfaces.Add(this);
        }

        protected virtual void OnDisable()
        {
            _allSurfaces.Remove(this);
        }
        
        public abstract bool Raycast(Camera sourceCamera, Ray ray, out GreasePencilHit hit);
    }
}

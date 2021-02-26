using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GreasePencil
{
    public class GreasePencilRuntime : MonoBehaviour, IDragHandler, IPointerDownHandler, IEndDragHandler
    {
        [SerializeField]
        GreasePencilCanvas _activeGreasePencil;

        [SerializeField]
        Camera _originatingCamera;

        [SerializeField]
        Color _lineColor = Color.black;

        [SerializeField]
        Color _fillColor = Color.clear;

        List<GreasePencilHit> _hits = new List<GreasePencilHit>();

        void OnEnable()
        {
            if (_originatingCamera == null)
            {
                Debug.Log("Needs Originating Camera", this);
                enabled = false;
                return;
            }

            Deserialize();
        }

        void OnDisable()
        {
            Serialize();
        }

        private void Deserialize()
        {
            if (_activeGreasePencil != null)
                _activeGreasePencil.Deserialize(PlayerPrefs.GetString("SAVE", string.Empty));
        }

        private void Serialize()
        {
            if (_activeGreasePencil != null)
                PlayerPrefs.SetString("SAVE", _activeGreasePencil.Serialize());
        }

        public void ClearCanvas()
        {
            _activeGreasePencil.ClearAllLayers();
            Serialize();
        }

        public void OnDrag(PointerEventData eventData)
        {
            Ray ray = _originatingCamera.ScreenPointToRay(eventData.position);
            GetHits(ray, _hits, _originatingCamera);
            if (_hits.Count > 0)
                _activeGreasePencil.UpdateStroke(new Point(_hits[0]));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Ray ray = _originatingCamera.ScreenPointToRay(eventData.position);
            _activeGreasePencil.EndStroke();

            Serialize();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Ray ray = _originatingCamera.ScreenPointToRay(eventData.position);
            GetHits(ray, _hits, _originatingCamera);
            if (_hits.Count > 0)
                _activeGreasePencil.StartStroke(new Point(_hits[0]), 1f, _originatingCamera.transform, _lineColor, _fillColor);
        }

        private void GetHits(Ray ray, List<GreasePencilHit> hits, Camera camera)
        {
            hits.Clear();

            var surfaces = GreasePencilRuntimeSurface.AllSurfaces;

            GreasePencilHit hit;

            foreach (var surface in surfaces)
            {
                if (surface.Raycast(camera, ray, out hit))
                    hits.Add(hit);
            }
        }
    }
}

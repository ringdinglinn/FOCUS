using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class CarConfig : MonoBehaviourReferenced {
    public List<WheelCollider> wheels;
    public MeshRenderer meshRenderer;
    public BoxCollider boxCollider;
    public GameObject camRotTarget;
    public GameObject camTranslateTarget;
    public GameObject spotlights;
    public HDAdditionalLightData headlight1;
    public HDAdditionalLightData headlight2;
    public GameObject volumetrics;
    public MeshRenderer volumetricRenderer0;
    public MeshRenderer volumetricRenderer1;
    public List<MeshRenderer> headlightCubes;
    public GameObject armaturenbrett;
}
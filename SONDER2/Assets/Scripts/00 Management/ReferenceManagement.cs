using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourReferenced : MonoBehaviour {
    public ReferenceManagement referenceManagement;

    protected virtual void Awake() {
        referenceManagement = GameObject.Find("Reference Management").GetComponent<ReferenceManagement>();
    }
}

public class ReferenceManagement : MonoBehaviour
{
    public InputManagement inputManagement;
    public SwitchingManagement switchingManagement;
    public Camera1stPerson cam;
    public PathManagement pathManagement;
    [SerializeField] private float BPM;

    public AudioProcessor audioProcessor;

    public float GetBPM() {
        return BPM;
    }
}

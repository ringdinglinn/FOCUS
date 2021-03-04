using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewCone : MonoBehaviourReferenced {
    private SwitchingBehaviour switchingBehaviour;

    private void Start() {
        switchingBehaviour = GetComponentInParent<SwitchingBehaviour>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Car") && other.gameObject != gameObject && other.gameObject != switchingBehaviour.meshRenderer.gameObject) {
            switchingBehaviour.CarBecomesVisible(other.gameObject.GetComponentInParent<SwitchingBehaviour>());
            Debug.Log(other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Car") && other.gameObject != gameObject && other.gameObject != switchingBehaviour.meshRenderer.gameObject) {
            switchingBehaviour.CarBecomesInvisible(other.gameObject.GetComponentInParent<SwitchingBehaviour>());
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelTriggerBehaviour : MonoBehaviourReferenced {

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            other.GetComponentInParent<CarAI>().InTunnel = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            other.GetComponentInParent<CarAI>().InTunnel = false;
        }
    }
}
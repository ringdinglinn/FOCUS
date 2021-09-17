using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInTunnel : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        Debug.Log("JHAIHFOIUAH");
        if (other.CompareTag("Car")) {
            Debug.Log("Enter");
            CarAI carAI = other.GetComponentInParent<CarAI>();
            carAI.InTunnel = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Car")) {
            CarAI carAI = other.GetComponentInParent<CarAI>();
            carAI.InTunnel = false;
        }
    }
}

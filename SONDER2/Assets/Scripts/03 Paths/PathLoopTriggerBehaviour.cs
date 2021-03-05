using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathLoopTriggerBehaviour : MonoBehaviourReferenced {
	private CarAI carAI;

    private void Start() {
        // fix this!
        carAI = GetComponentInParent<CarAI>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            carAI = other.gameObject.GetComponentInParent<CarAI>();
            carAI.EndReached();
        }
    }
}
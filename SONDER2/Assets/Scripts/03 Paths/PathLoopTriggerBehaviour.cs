using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathLoopTriggerBehaviour : MonoBehaviourReferenced {
	private CarAI carAI;
    private bool isColliding;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            Debug.Log("car entered portal");
            if (isColliding) return;
            isColliding = true;
            carAI = other.gameObject.GetComponentInParent<CarAI>();
            carAI.PortalReached();
            StartCoroutine(SetIsCollidingFalse());
        }
    }

    private IEnumerator SetIsCollidingFalse() {
        yield return new WaitForFixedUpdate();
        isColliding = false;
    }
}
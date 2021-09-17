using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableMountains : MonoBehaviour
{
    public GameObject mountains;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Car")) {
            mountains.SetActive(true);
        }
    }
}

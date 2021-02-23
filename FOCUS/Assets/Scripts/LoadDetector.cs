using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadDetector : MonoBehaviour
{

    [SerializeField]
    private string loadTag = "Load";

    [SerializeField]
    private Text loadUiText;

    private List<Rigidbody> loads = new List<Rigidbody>();
   
    private void UpdateCount() {
        loadUiText.text = "Current load: " + loads.Count;
    }

   private void OnTriggerEnter(Collider collider) {
       if(collider.attachedRigidbody == null) return;
       if(collider.attachedRigidbody.CompareTag(loadTag) == false) return;
       if(loads.Contains(collider.attachedRigidbody)) return;

       loads.Add(collider.attachedRigidbody);
       UpdateCount();
   }

   private void OnTriggerExit(Collider collider) {
       if(collider.attachedRigidbody == null) return;
       if(collider.attachedRigidbody.CompareTag(loadTag) == false) return;
       if(loads.Contains(collider.attachedRigidbody) == false) return;

       loads.Remove(collider.attachedRigidbody);
       UpdateCount();
   }
}

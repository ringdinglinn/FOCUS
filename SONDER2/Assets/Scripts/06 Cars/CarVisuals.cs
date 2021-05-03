using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class CarVisuals : MonoBehaviour {
	public List<GameObject> carModels = new List<GameObject>();
	public List<GameObject> wheels = new List<GameObject>();

	public enum car { car1, car2 };
	public car myCar;

	public void SetCarModel(int i) {
		foreach (GameObject obj in carModels) {
			obj.SetActive(false);
        }
		foreach (GameObject obj in wheels) {
			obj.SetActive(false);
		}
		carModels[i].SetActive(true);
		wheels[i].SetActive(true);
	}

    private void Update() {
		SetCarModel((int)myCar);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarVisuals : MonoBehaviourReferenced {
	public List<GameObject> carModels = new List<GameObject>();
	public List<GameObject> wheels = new List<GameObject>();

	VehicleBehaviour.WheelVehicle wv;
	SwitchingBehaviour switchingBehaviour;

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

	private void OnEnable() {
		switchingBehaviour = GetComponent<SwitchingBehaviour>();
		wv = GetComponent<VehicleBehaviour.WheelVehicle>();
        SetCarModel((int)myCar);
        switchingBehaviour.meshRenderer = carModels[(int)myCar].GetComponent<MeshRenderer>();
		wv.TurnWheel = new WheelCollider[] { wheels[(int)myCar].transform.GetChild(0).GetComponentInChildren<WheelCollider>(), wheels[(int)myCar].transform.GetChild(1).GetComponentInChildren<WheelCollider>() };
		wv.DriveWheel = new WheelCollider[] { wheels[(int)myCar].transform.GetChild(2).GetComponentInChildren<WheelCollider>(), wheels[(int)myCar].transform.GetChild(3).GetComponentInChildren<WheelCollider>() };
	}
}
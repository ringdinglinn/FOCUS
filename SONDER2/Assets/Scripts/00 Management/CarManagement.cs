using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.Events;

public class CarManagement : MonoBehaviourReferenced {

    PathManagement pathManagement;
    SwitchingManagement switchingManagement;
    List<PathBehaviour> allPathBehaviours;
    List<SwitchingBehaviour> allSwitchingBehaviours = new List<SwitchingBehaviour>();
    List<CarAI> allCarAIs = new List<CarAI>();

    public UnityEvent cameraChanged;
    public UnityEvent carsCreated;
    public UnityEvent allSBChanged;

    private GameObject carPrefab;

    private List<CarAI> carAIsInTunnel;

    private int idCounter = 0;
    private string carName = "Car";

    private GameObject parentObj;

    private SwitchingBehaviour initialCar = null;
    private bool manualInitialCar = true;
    public bool carsGenerated = false;


    private void OnEnable() {
        pathManagement = referenceManagement.pathManagement;
        allPathBehaviours = pathManagement.GetAllPaths();
        carPrefab = referenceManagement.carPrefab;
        switchingManagement = referenceManagement.switchingManagement;
        parentObj = GameObject.Find("Cars");
        InstantiateCars();
        SetUpInitialCar();
    }

    private void Start() {

    }

    private void InstantiateCars() {
        bool setInitial = false;
        for (int i = 0; i < allPathBehaviours.Count; i++) {

            List<Vector3> startPosList = allPathBehaviours[i].GetStartPosBehaviour().GetStartPosList();
            int initPosIndex = allPathBehaviours[i].GetStartPosBehaviour().GetInitialPosIndex();
            Debug.Log($"initPosIndex = {initPosIndex} ");
            PathCreator path = allPathBehaviours[i].GetPath();

            for (int j = 0; j < startPosList.Count; j++) {
                // adjust start position
                startPosList[j] = path.path.GetClosestPointOnPath(startPosList[j]);

                // find start position
                Vector3 pos = startPosList[j];
                Vector3 dir = path.path.GetDirectionAtDistance(path.path.GetClosestDistanceAlongPath(pos));

                // instantiate and references
                GameObject car = Instantiate(carPrefab, startPosList[j], Quaternion.LookRotation(dir));
                car.transform.SetParent(parentObj.transform);
                CarAI carAI = car.GetComponent<CarAI>();
                SwitchingBehaviour sb = car.GetComponent<SwitchingBehaviour>();
                sb.id = idCounter;
                ++idCounter;
                car.name = carName + (idCounter - 1);

                // set path id
                carAI.PathID = allPathBehaviours[i].id;

                // set and save start position
                carAI.CreateStartConfig(pos, dir);
                carAI.SetToStartConfig();

                // look for initial car
                if (!setInitial && j == initPosIndex) {
                    setInitial = true;
                    initialCar = sb;
                    Debug.Log($"initCar Found, j = {j}");
                }
            }
        }
        if (initialCar != null) {
            manualInitialCar = false;
            Debug.Log("manual initial car set false");
        }
        carsCreated.Invoke();
        carsGenerated = true;
    }

    private void SetUpInitialCar() {
        for (int i = 0; i < allSwitchingBehaviours.Count; i++) {
            if (allSwitchingBehaviours[i].isInitialCar) {
                initialCar = allSwitchingBehaviours[i];
            }
        }
        CarAI carAI = initialCar.GetCarAI();
        carAI.cam = referenceManagement.cam;
        carAI.SetUpInititalCar();
        referenceManagement.switchingManagement.SetUpInitialCar(initialCar);
    }

    public CarAI CreateCarClone(int pathID) {
        GameObject cloneObj = Instantiate(carPrefab, Vector3.zero, Quaternion.identity);
        CarAI carAI = cloneObj.GetComponent<CarAI>();
        carAI.PathID = pathID;
        return carAI;
    }

    public List<SwitchingBehaviour> GetAllSwitchingBehaviours() {
        return allSwitchingBehaviours;
    }

    public List<CarAI> GetAllCarAIs() {
        return allCarAIs;
    }

    public void ActiveCarHasReachedPortal(CarAI activeCar) {
        carAIsInTunnel = activeCar.endTunnel.GetCarAIsInTunnel();
        Debug.Log("active car has reached tunnel");
        foreach (CarAI car in carAIsInTunnel) {
            Debug.Log("foreach");
            if (car.HasClone) {
                Debug.Log("has clone");
                car.ActiveCarHasReachedPortal();
            }
        }
    }

    public void ChangeToClone(bool isActiveCar, CarAI oldCar, CarAI newCar) {
        Debug.Log("car management, change to clone");
        newCar.MakeMainCar(isActiveCar);
        allSwitchingBehaviours.Remove(oldCar.GetComponent<SwitchingBehaviour>());
        allCarAIs.Remove(oldCar);
        allCarAIs.Add(newCar);
        SwitchingBehaviour newSB = newCar.GetComponent<SwitchingBehaviour>();
        newSB.id = idCounter;                   
        ++idCounter;
        newCar.gameObject.name = carName + (idCounter - 1);
        oldCar.endTunnel.CarIsDestroyed(oldCar);
        if (isActiveCar) {
            referenceManagement.cam = newCar.cam;
            cameraChanged.Invoke();
            switchingManagement.SetActiveCar(newSB);
            Destroy(oldCar.cam.gameObject);
        }
        Destroy(oldCar.gameObject);
        allSBChanged.Invoke();
    }

    public void ActiveCarGearChanged(int gear) {
        for (int i = 0; i < allCarAIs.Count; i++) {
            if (allCarAIs[i].CurrentGear == gear && switchingManagement.MarkedCar != allCarAIs[i].GetSwitchingBehaviour()) {
                int newGear = Random.Range(1, 6);
                if (newGear >= gear) newGear++;
                allCarAIs[i].SetGear(newGear);
            }
        }
    }

    public void AddCarManagement(CarAI carAI) {
        allCarAIs.Add(carAI);
    }

    public void AddSwitchingBehaviour(SwitchingBehaviour sb) {
        allSwitchingBehaviours.Add(sb);
    }

    public bool GetManualInitialCar() {
        return manualInitialCar;
    }
}
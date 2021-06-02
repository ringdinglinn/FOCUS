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
    LevelManagement levelManagement;

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
        levelManagement = referenceManagement.levelManagement;
        InstantiateCars();
        SetUpInitialCar();
        SetUpSecondCar();
    }

    private void InstantiateCars() {
        bool setInitial = false;
        for (int i = 0; i < allPathBehaviours.Count; i++) {

            List<Vector3> startPosList = allPathBehaviours[i].GetStartPosBehaviour().GetStartPosList();
            int initPosIndex = allPathBehaviours[i].GetStartPosBehaviour().GetInitialPosIndex();
            PathCreator path = allPathBehaviours[i].GetPath();

            for (int j = 0; j < startPosList.Count; j++) {
                // adjust start position
                startPosList[j] = path.path.GetClosestPointOnPath(startPosList[j]);

                // find start position
                Vector3 pos = startPosList[j];
                Vector3 dir = path.path.GetDirectionAtDistance(path.path.GetClosestDistanceAlongPath(pos));

                // instantiate and references
                GameObject car = Instantiate(carPrefab, startPosList[j], Quaternion.LookRotation(dir));
                car.transform.SetParent(allPathBehaviours[i].transform);
                CarAI carAI = car.GetComponent<CarAI>();
                SwitchingBehaviour sb = car.GetComponent<SwitchingBehaviour>();
                CarVisuals carVisuals = car.GetComponent<CarVisuals>();
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
                }

                // randomize visuals
                carVisuals.SetCarVisuals(Random.Range(0, carVisuals.allCarConfigs.Count), Random.Range(0,2));
                carVisuals.UpdateVisuals(false);
            }
        }
        if (initialCar != null) {
            manualInitialCar = false;
        }
        carsCreated.Invoke();
        carsGenerated = true;
    }

    private void SetUpInitialCar() {
        for (int i = 0; i < allSwitchingBehaviours.Count; i++) {
            if (allSwitchingBehaviours[i].isInitialCar) {
                initialCar = allSwitchingBehaviours[i];
                manualInitialCar = true;
            }
        }
        CarAI carAI = initialCar.GetCarAI();
        CarVisuals carVisuals = initialCar.GetComponent<CarVisuals>();
        carAI.cam = referenceManagement.cam;
        carAI.SetUpInititalCar();
        carVisuals.SetCarVisuals(Random.Range(0, carVisuals.allCarConfigs.Count), Random.Range(0, 2));
        carVisuals.UpdateVisuals(true);
        referenceManagement.switchingManagement.SetUpInitialCar(initialCar);
    }

    private void SetUpSecondCar() {
        SwitchingBehaviour secondCar;
        for (int i = 0; i < allSwitchingBehaviours.Count; i++) {
            if (allSwitchingBehaviours[i].isSecondCar) {
                secondCar = allSwitchingBehaviours[i];
                manualInitialCar = true;
                CarAI carAI = secondCar.GetCarAI();
                CarVisuals carVisuals = secondCar.GetComponent<CarVisuals>();
                carVisuals.SetCarVisuals(Random.Range(0, carVisuals.allCarConfigs.Count), Random.Range(0, 2));
                carVisuals.UpdateVisuals(true);
            }
        }
    }

    public CarAI CreateCarClone(int pathID) {
        GameObject cloneObj = Instantiate(carPrefab, Vector3.zero, Quaternion.identity);
        CarAI carAI = cloneObj.GetComponent<CarAI>();
        carAI.PathID = pathID;
        cloneObj.transform.SetParent(pathManagement.GetMyPath(pathID).transform);
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

        int count = carAIsInTunnel.Count - 1;
        for (int i = count; i >= 0; i--) { 
            if (carAIsInTunnel[i].HasClone) {
                carAIsInTunnel[i].ActiveCarHasReachedPortal();
            }
        }
    }

    private void SetAllCarsToStartConfig() {
        Debug.Log($"all car ais = {allCarAIs.Count}");
        foreach (CarAI car in allCarAIs) {
            if (car != switchingManagement.ActiveCar.GetCarAI())
            car.SetToStartConfig();
        }
    }

    public void ChangeToClone(bool isActiveCar, CarAI oldCar, CarAI newCar) {
        Debug.Log($"change to clone {newCar.name}");
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
            Camera camcam = newCar.cam.GetComponent<Camera>();
            camcam.enabled = true;
            cameraChanged.Invoke();
            switchingManagement.SetActiveCar(newSB);
            Destroy(oldCar.cam.gameObject);
        }
        Destroy(oldCar.gameObject);

        allSBChanged.Invoke();

        SetAllCarsToStartConfig();
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

    public void AddCarAI(CarAI carAI) {
        allCarAIs.Add(carAI);
    }

    public void RemoveCarAI(CarAI car) {
        allCarAIs.Remove(car);
    }

    public void AddSwitchingBehaviour(SwitchingBehaviour sb) {
        allSwitchingBehaviours.Add(sb);
        allSBChanged.Invoke();
    }

    public void RemoveSwitchingBehaviour(SwitchingBehaviour sb) {
        allSwitchingBehaviours.Remove(sb);
        allSBChanged.Invoke();
    }

    public bool HasManualInitialCar() {
        return manualInitialCar;
    }

    public void ClearSBs() {
        allSwitchingBehaviours.Clear();
    }

    public void ClearCarAIs() {
        allCarAIs.Clear();
    }
}
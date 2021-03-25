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

    private GameObject carPrefab;

    private List<CarAI> carAIsInTunnel;

    private int idCounter = 0;
    private string carName = "Car";

    private void OnEnable() {
        pathManagement = referenceManagement.pathManagement;
        allPathBehaviours = pathManagement.GetAllPaths();
        carPrefab = referenceManagement.carPrefab;
        switchingManagement = referenceManagement.switchingManagement;
    }

    private void Start() {
        InstantiateCars();
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
                CarAI carAI = car.GetComponent<CarAI>();
                SwitchingBehaviour sb = car.GetComponent<SwitchingBehaviour>();
                sb.id = idCounter;
                ++idCounter;
                car.name = carName + (idCounter - 1);
                allSwitchingBehaviours.Add(sb);
                allCarAIs.Add(carAI);

                // set path id
                carAI.PathID = i;

                // set and save start position
                carAI.CreateStartConfig(pos, dir);
                carAI.SetToStartConfig();

                // initial car
                if (!setInitial && j == initPosIndex) {
                    setInitial = true;
                    carAI.cam = referenceManagement.cam;
                    carAI.SetUpInititalCar();
                    referenceManagement.switchingManagement.SetUpInitialCar(sb);
                }
            }
        }
        carsCreated.Invoke();
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
        carAIsInTunnel = allPathBehaviours[activeCar.PathID].endTunnel.GetCarAIsInTunnel();
        foreach (CarAI car in carAIsInTunnel) {
            if (car.HasClone) {
                car.ActiveCarHasReachedPortal();
            }
        }
    }

    public void ChangeToClone(bool isActiveCar, CarAI oldCar, CarAI newCar) {
        newCar.MakeMainCar(isActiveCar);
        allSwitchingBehaviours.Remove(oldCar.GetComponent<SwitchingBehaviour>());
        allCarAIs.Remove(oldCar);
        allCarAIs.Add(newCar);
        SwitchingBehaviour newSB = newCar.GetComponent<SwitchingBehaviour>();
        allSwitchingBehaviours.Add(newSB);
        newSB.id = idCounter;                   
        ++idCounter;
        newCar.gameObject.name = carName + (idCounter - 1);
        allPathBehaviours[oldCar.PathID].endTunnel.CarIsDestroyed(oldCar);
        if (isActiveCar) {
            referenceManagement.cam = newCar.cam;
            cameraChanged.Invoke();
            switchingManagement.SetActiveCar(newSB);
            Destroy(oldCar.cam.gameObject);
        }
        Destroy(oldCar.gameObject);
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
}
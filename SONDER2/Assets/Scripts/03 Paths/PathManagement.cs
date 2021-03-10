using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PathManagement : MonoBehaviourReferenced {
	private List<PathBehaviour> paths = new List<PathBehaviour>();

    private void Start() {
    }

    public void AddToPaths(PathBehaviour pb) {
        pb.SetID(paths.Count);
        paths.Add(pb);
    }

    public PathBehaviour GetMyPath(int id) {
        return paths[id];
    }

    public List<PathBehaviour> GetAllPaths() {
        return paths;
    }
}
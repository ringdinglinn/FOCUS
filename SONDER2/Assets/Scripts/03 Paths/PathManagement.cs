using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PathManagement : MonoBehaviourReferenced {
	private List<PathBehaviour> paths = new List<PathBehaviour>();
    private List<TunnelBehaviour> tunnels = new List<TunnelBehaviour>();

    public void AddToPaths(PathBehaviour pb) {
        paths.Add(pb);
        if (pb.endTunnel != null) pb.endTunnel.SetEndTunnelID(pb.id);
    }

    public void AddToTunnels(TunnelBehaviour tunnel) {
        tunnel.SetID(tunnels.Count);
        tunnels.Add(tunnel);
    }

    public PathBehaviour GetMyPath(int id) {
        for (int i = 0; i < paths.Count; i++) {
            if (paths[i].id == id) {
                return paths[i];
            }
        }
        Debug.LogError("Path not Found");
        return null;
    }

    public List<PathBehaviour> GetAllPaths() {
        return paths;
    }
}
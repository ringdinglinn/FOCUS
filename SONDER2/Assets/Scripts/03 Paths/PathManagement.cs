using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PathManagement : MonoBehaviourReferenced {
	private List<PathBehaviour> paths = new List<PathBehaviour>();
    private List<TunnelBehaviour> tunnels = new List<TunnelBehaviour>();

    public void AddToPaths(PathBehaviour pb) {
        pb.SetID(paths.Count);
        paths.Add(pb);
        if (pb.endTunnel != null) pb.endTunnel.SetEndTunnelID(pb.Id);
    }

    public void AddToTunnels(TunnelBehaviour tunnel) {
        tunnel.SetID(tunnels.Count);
        tunnels.Add(tunnel);
    }

    public PathBehaviour GetMyPath(int id) {
        return paths[id];
    }

    public List<PathBehaviour> GetAllPaths() {
        return paths;
    }
}
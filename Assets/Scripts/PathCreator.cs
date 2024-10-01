using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class PathCreator : MonoBehaviour
{
    [System.Serializable]
    public class PathSetting {
        public string name;
        public int startEndpointIndex, endEndpointIndex;
        public PathAsset pathAsset;
        public bool showDebug;
        public Color debugColor = Color.yellow;
    }

    public LevelController lvlController;
    public List<PathSetting> pathSettings = new List<PathSetting>();

    #if UNITY_EDITOR
    void OnDrawGizmos() {
        foreach(PathSetting ps in pathSettings) {
            if (!ps.showDebug) continue;
            Vector2Int indices = new Vector2Int(ps.startEndpointIndex, ps.endEndpointIndex); 
            if (indices.x == indices.y) return;
            if (indices.x < 0 || indices.x >= lvlController.endpoints.Length || indices.y < 0 || indices.y >= lvlController.endpoints.Length) return;
            Gizmos.color = ps.debugColor;
            Vector3 start = lvlController.endpoints[indices.x].position;
            Vector3 end = lvlController.endpoints[indices.y].position;
            Gizmos.DrawSphere(start, 0.05f);
            Gizmos.DrawSphere(end, 0.05f);
            Gizmos.DrawLine(start, end);
        }
    }

    private void Update() {
        for(int i = 0; i < pathSettings.Count; i++) {
            if (pathSettings[i].pathAsset != null) {
                pathSettings[i].pathAsset.pathName = pathSettings[i].name;
                pathSettings[i].pathAsset.startEndpointIndex = pathSettings[i].startEndpointIndex;
                pathSettings[i].pathAsset.endEndpointIndex = pathSettings[i].endEndpointIndex;
            }
        }
    }
    #endif
    
}

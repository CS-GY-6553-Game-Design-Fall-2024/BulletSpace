using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PathVisualizer : MonoBehaviour
{
    public LevelController lvlController;
    public Vector2Int pathIndices = new Vector2Int(0,0);
    public PathAsset pathAsset;
    public Color pathColor = Color.yellow;

    #if UNITY_EDITOR
    void OnDrawGizmos() {
        if (Application.isPlaying) return;
        if (lvlController == null) return;
        
        Vector2Int indices = (pathAsset != null) ? new Vector2Int(pathAsset.startEndpointIndex, pathAsset.endEndpointIndex) : pathIndices; 
        if (indices.x == indices.y) return;
        if (indices.x < 0 || indices.x >= lvlController.endpoints.Length || indices.y < 0 || indices.y >= lvlController.endpoints.Length) return;
        Gizmos.color = pathColor;
        Vector3 start = lvlController.endpoints[indices.x].position;
        Vector3 end = lvlController.endpoints[indices.y].position;
        Gizmos.DrawSphere(start, 0.05f);
        Gizmos.DrawSphere(end, 0.05f);
        Gizmos.DrawLine(start, end);
    }
    #endif
}

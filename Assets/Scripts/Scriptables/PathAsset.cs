using UnityEngine;
using UnityEngine.Splines;

[CreateAssetMenu(fileName="Path",menuName="Pathing/Path",order=1)]
public class PathAsset : ScriptableObject
{
    public string pathName;
    public int startEndpointIndex, endEndpointIndex;
}

[System.Serializable]
public class Path {
    public string pathName;
    public Vector3 start, end;
    public float distance;

    public Path(string pathName, Vector3 start, Vector3 end, float distance) {
        this.pathName = pathName;
        this.start = start;
        this.end = end;
        this.distance = distance;
    }
}
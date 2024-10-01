using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="Match",menuName="Pathing/Matches",order=2)]
public class MatchAsset : ScriptableObject
{
    public List<Match> matches;
}

[System.Serializable]
public class Match {
    public string name;
    public float timeToWait = 1f;
    public Enemy enemy;
    public PathAsset pathAsset;
    public AnimationCurve speedCurve;
    public Vector3 offset;
    public bool reverse;
    public bool setCheckpoint;
    public bool active;
}
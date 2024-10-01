using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MatchTotalScoreCalculator : MonoBehaviour
{
    public MatchAsset m_match;
    [SerializeField] private int m_totalScore;

    #if UNITY_EDITOR
    private void Update() {
        if (m_match == null) return;
        
        m_totalScore = 0;
        foreach(Match m in m_match.matches) {
            if (m.enemy != null) m_totalScore += m.enemy.resources;
        }
    }
    #endif
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class VisibilityCheck : MonoBehaviour
{
    public Enemy enemyParent;
    void OnBecameVisible() {
        if (enemyParent != null) enemyParent.BecameVisible();
    }
}

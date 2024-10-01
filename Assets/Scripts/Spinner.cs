using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    public enum SpinDirection { Clockwise=1, CounterClockwise=-1 }
    [SerializeField] private SpinDirection m_spinDir = SpinDirection.Clockwise;
    [SerializeField] private float m_spinSpeed = 15f;

    public void Update() {
        float spinDir = Mathf.Sign((int)m_spinDir);
        transform.Rotate(Vector3.forward, spinDir * m_spinSpeed * Time.deltaTime);
    }
}

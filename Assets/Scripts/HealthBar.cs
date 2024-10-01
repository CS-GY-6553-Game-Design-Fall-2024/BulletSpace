using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider m_slider;
    [SerializeField] private Transform targetPos;
    [SerializeField] private Vector2 m_offset = Vector2.zero;
    
    public void SetSliderValue(float current, float max) {
        m_slider.value = Mathf.Clamp(current / max, 0f, 1f);
    }

    private void Update() {
        transform.rotation = Camera.main.transform.rotation;
        transform.position = targetPos.position + (Vector3)m_offset;
    }
}

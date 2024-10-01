using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody2D m_rb;
    [SerializeField] private float m_launchForce;
    [SerializeField] private float m_destroyAfterSeconds;
    [SerializeField] private float m_damage;
    public float damage => m_damage;

    private void Awake()
    {
        LevelController.current.addBullet(this);
    }

    public void Fire(Vector2 direction, float d) {
        m_rb.velocity = direction.normalized * m_launchForce;
        m_damage = d;
    }

    public void Fire(Vector3 direction, float d) {
        m_rb.velocity = (new Vector2(direction.x, direction.y)).normalized * m_launchForce;
        m_damage = d;
    }

    private void Update() {
        Destroy(this.gameObject, m_destroyAfterSeconds);
    }
}

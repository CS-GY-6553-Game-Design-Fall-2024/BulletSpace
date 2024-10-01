using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public enum AimTarget { ToShipForward, ToGunForward, ToDirection, toPlayer }

    [Header("=== Enemy Stats ===")]
    [SerializeField] private HealthBar m_healthBar;
    [SerializeField] private float m_maxHealth = 100f;
    [SerializeField] private int m_resources = 10;
    public int resources => m_resources;
    private float m_health;
    [SerializeField] private bool m_initialized = false;
    private bool m_onScreen = false;

    [Header("=== Movement Settings ===")]
    [SerializeField] private Path m_path;
    [SerializeField] private Vector3 m_startPos, m_endPos, m_pathVector;
    [SerializeField] private AnimationCurve m_speedCurve;
    [SerializeField] private float m_progressAlongPath = 0f;
    private bool m_slatedToDelete = false;

    [Header("=== Attack Settings ===")]
    [SerializeField] private List<Gun> m_guns;
    [SerializeField] private AimTarget m_aimBehavior = AimTarget.ToDirection;
    [SerializeField] private Vector2 m_aimDirection = Vector2.down;
    [SerializeField] private bool m_isSub = false;
    [SerializeField] private List<Enemy> m_subEnemies;

    private void Awake() {
        m_health = m_maxHealth;
        if (m_healthBar != null) m_healthBar.SetSliderValue(m_health, m_maxHealth);
    }

    public void BecameVisible() {
        // Initialize the guns
        foreach(Gun g in m_guns) g.StartFireCycle();
        m_onScreen = true;
    }

    public void Initialize(Path p, bool reverse, AnimationCurve speedCurve, Vector3 offset) {
        // Get the received path
        m_path = p;
        m_speedCurve = speedCurve;

        // Based on the received path, reposition ourselves to match the lsited offset
        m_startPos = (reverse) ? p.end + offset : p.start + offset;
        m_endPos = (reverse) ? p.start + offset : p.end + offset;
        m_pathVector = m_endPos - m_startPos;

        // Initialize the enemy and any sub-enemies
        m_initialized = true;
        if (m_subEnemies.Count > 0) {
            foreach(Enemy se in m_subEnemies) se.InitializeAsSub();
        }
    }
    public void InitializeAsSub() {
        // Sub-enemies still fire and do their thing. But they don't move and aren't expected to move.
        m_isSub = true;
        foreach(Gun g in m_guns) g.StartFireCycle();
        m_initialized = true;
    }

    private void Update() {
        // Don't do anything if we're not initialized or if we're slated to be deleted.
        if (!m_initialized || m_slatedToDelete) return;

        // Move the enemy
        if (!m_isSub) Movement();

        // Determine the aim direction
        Aim();

        // Check if we have to be deleted
        if (!m_isSub) CheckStatus();
    }


    public void SetAimDirection(Vector3 dir) {
        m_aimBehavior = AimTarget.ToDirection;
        m_aimDirection = dir;
    }

    public void Movement() {
        // 1. Given the current progress along path, calculate the current speed and the resulting displacement
        float currentSpeed = m_speedCurve.Evaluate(m_progressAlongPath);
        float displacement = currentSpeed * Time.deltaTime;

        // 2. Given the displacement amount, update `m_progressAlongPath`. Note that if we're more than 100% along the path, we terminate movement early
        m_progressAlongPath = Mathf.Clamp(m_progressAlongPath + (displacement/m_path.distance), 0f, 1f);
        if (m_progressAlongPath >= 1.0f) return;

        // 3. Given the path's spline, get the position the enemy SHOULD be at along the path.
        Vector3 newPos = m_startPos + m_pathVector*m_progressAlongPath;

        // 4. Perform the necessary transformations, not just position but also rotation. Note that rotation must be along z-axis this time.
        // THough this operation is in 3D, the cooridnates for the player's position is technically in 2D. So we usually only pay attention to the XY coordinates.
        // In this case, we want any rotations to occur around the Z-axis, not the X or Y axes. So we conduct Atan2 to get which angle the entity must be.
        Vector3 targetDir = newPos - transform.position;
        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle); // Rotate around the z-axis only
        transform.position = newPos;
    }

    private void Aim() {
        switch(m_aimBehavior) {
            case AimTarget.ToShipForward:
                m_aimDirection = transform.rotation * Vector2.right;
                foreach(Gun g in m_guns) g.SetDirection(m_aimDirection);
                break;
            case AimTarget.ToGunForward:
                foreach(Gun g in m_guns) g.SetDirection(g.transform.right);
                break;
            case AimTarget.toPlayer:
                foreach(Gun g in m_guns) {
                    g.SetDirection((Vector2)(PlayerController.current.transform.position - g.transform.position));
                }
                break;
            default:
                foreach(Gun g in m_guns) g.SetDirection(m_aimDirection);
                break;
        }
    }

    private void CheckStatus() {
        // Update the health slider
       if (m_healthBar != null) m_healthBar.SetSliderValue(m_health, m_maxHealth);

        // We despawn if either our health is 0 (the player killed us) or if we naturally reached the end of our path
        if (m_health <= 0f ) {
            LevelController.current.EnemyDied(this);
            m_slatedToDelete = true;
        }
        else if (m_progressAlongPath >= 1f)
        {
            LevelController.current.EnemyDespawn(this);
            m_slatedToDelete = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {

        // Grab reference to this other collider's bullet and subsequently its damage
        Bullet b = other.gameObject.GetComponent<Bullet>();
        
        // Only receive damage if we're on the screen
        if (m_onScreen) m_health -= b.damage;

        // Destroy the enemy bullet
        Destroy(other.gameObject);


    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("=== Gun Characteristics ===")]

    [SerializeField, Tooltip("The bullet prefab it will launch. Inspector only.")]
    private Bullet m_bullet;
    [SerializeField, Tooltip("The direction the gun will fire. Can be programmatically set with SetDirection(Vector2)")]
    private Vector2 m_direction = Vector2.down;
    [SerializeField, Tooltip("The amount of damage the gun does per bullet upon contact. Can be programmatically set with SetDamage(float)")]
    private float m_damage = 1f;
    [SerializeField, Tooltip("How long is the cooldown between shots, if not reloading? Can be programmatically set with SetCooldownTime(float)")]
    private float m_cooldownTime = 0.5f;
    [SerializeField, Tooltip("How long is the reload time after using all capacity? Can be programmatically set with SetReloadTime(float)")]
    private float m_reloadTime = 2f;
    [SerializeField, Tooltip("How many bullets will the gun fire between reloads? Can be programmatically set with SetCapacity*int)")]
    private int m_bulletCapacity = 1;
    [SerializeField, Tooltip("The layer name to assign to bullets fired by this gun.")]
    private string m_bulletLayer = "GameLayer";
    //[SerializeField] private bool m_printDebug = false;

    [Header("=== Audio Clips ===")]
    [SerializeField, Tooltip("Sound effect to play when the player fires.")]
    private AudioClip m_playerFireSound;
    [SerializeField, Tooltip("Sound effect to play when the enemy fires.")]
    private AudioClip m_enemyFireSound;

    [Header("=== Boolean Checks - don't touch these! ===")]
    [SerializeField] private bool m_firing = false;
    [SerializeField] private bool m_continuousFiring = false;
    [SerializeField] private int m_numBulletsFired = 0;

    private WaitForSeconds m_cooldownWait, m_reloadWait;
    private IEnumerator m_cooldownCycle, m_reloadCycle, m_fireCycle;
    [SerializeField] private AudioSource m_audioSource;

    private void Awake()
    {
        m_cooldownWait = new WaitForSeconds(m_cooldownTime);
        m_reloadWait = new WaitForSeconds(m_reloadTime);
        m_audioSource = GetComponent<AudioSource>();
    }

    // Any external script can tell the gun "continuous fire"
    public void StartFireCycle()
    {
        //if (m_printDebug) Debug.Log("Starting firing sequence");
        if (!m_continuousFiring)
        {
            m_continuousFiring = true;
            if (m_fireCycle == null)
            {
                //if (m_printDebug) Debug.Log("Fire Sequence Coroutine Set");
                m_fireCycle = FireCycle();
                StartCoroutine(m_fireCycle);
            }
        }
    }

    // Any external script can tell the gun "stop continuouis fire"
    public void StopFireCycle()
    {
        if (m_continuousFiring)
        {
            if (m_fireCycle != null) StopCoroutine(m_fireCycle);
            m_fireCycle = null;
            m_continuousFiring = false;
        }
    }

    // This forces the gun to loop its fire mechanism over and over. 
    private IEnumerator FireCycle()
    {
        while (true)
        {
            if (!m_firing)
            {
                //if (m_printDebug) Debug.Log("Firing Weapon");   
                Fire();
            }
            yield return null;
        }
    }

    // Any external script can tell the gun "Aim in this direction"
    public void SetDirection(Vector2 direction)
    {
        m_direction = direction;
    }

    // Any external script can tell the gun "do this amount of damage"
    public void SetDamage(float d)
    {
        m_damage = d;
    }

    // Any external script can tell the gun "take this long to cool down"
    public void SetCooldownTime(float t)
    {
        m_cooldownTime = t;
        m_cooldownWait = new WaitForSeconds(m_cooldownTime);
    }

    // Any external script can tell the gun "Take this long to reload"
    public void SetReloadTime(float t)
    {
        m_reloadTime = t;
        m_reloadWait = new WaitForSeconds(m_reloadTime);
    }

    // Any external script can tell the gun "Hold this number of bullets"
    public void SetCapacity(int c)
    {
        m_bulletCapacity = c;
    }

    public void SetLayer(string s)
    {
        m_bulletLayer = s;
    }

    // Any external script can tell the gun "Fire in your direction"
    // The only caveat is that we expect at least a cooldown period AND a reload time. 
    // If the gun is in cooldown or is reloading, we ain't gonna fire.
    public void Fire()
    {
        if (m_firing)
        {
            return;
        }
        m_firing = true;

        // Instantiate and set up the bullet
        Bullet b = Instantiate(m_bullet, transform.position, Quaternion.identity);
        b.gameObject.layer = LayerMask.NameToLayer(m_bulletLayer);

        // Rotate the bullet to face the firing direction
        float angle = Mathf.Atan2(m_direction.y, m_direction.x) * Mathf.Rad2Deg;
        angle -= 90f;
        b.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Fire the bullet
        b.Fire(m_direction, m_damage);
        m_numBulletsFired++;

        // Play the appropriate firing sound based on m_bulletLayer
        if (m_bulletLayer == "GameLayer")
        {
            if (m_enemyFireSound != null)
            {
                m_audioSource.PlayOneShot(m_enemyFireSound);
            }
        }
        else
        {
            if (m_playerFireSound != null)
            {
                m_audioSource.PlayOneShot(m_playerFireSound);
            }
        }

        // Handle cooldown or reload cycles
        if (m_numBulletsFired >= m_bulletCapacity)
        {
            if (m_reloadCycle != null) StopCoroutine(m_reloadCycle);
            m_reloadCycle = ReloadCycle();
            StartCoroutine(m_reloadCycle);
        }
        else
        {
            if (m_cooldownCycle != null) StopCoroutine(m_cooldownCycle);
            m_cooldownCycle = CooldownCycle();
            StartCoroutine(m_cooldownCycle);
        }
    }


    // Any external script can force the gun to reload early.
    public void Reload()
    {
        if (m_firing)
        {
            StopCoroutine(m_cooldownCycle);
            StopCoroutine(m_reloadCycle);
        }
        StartCoroutine(m_reloadCycle);
    }

    // Only called when the gun fires and we haven't reached the full bullet capacity yet. We force the gun to wait for a certain amount of time before it can fire again.
    private IEnumerator CooldownCycle()
    {
        //if (m_printDebug) Debug.Log("Cooldown");
        yield return m_cooldownWait;
        m_cooldownCycle = null;
        m_firing = false;
    }

    // Only called when the gun fires and we've reached teh bullet capacity, forcing us to "reload".
    private IEnumerator ReloadCycle()
    {
        //if (m_printDebug) Debug.Log("Reloading");
        yield return m_reloadWait;
        m_numBulletsFired = 0;
        m_reloadCycle = null;
        m_firing = false;
    }
}
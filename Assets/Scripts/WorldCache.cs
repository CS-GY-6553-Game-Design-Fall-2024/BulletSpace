using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCache : MonoBehaviour
{
    public static WorldCache current;

    [Header("=== Cache ===")]
    [SerializeField] private float m_maxHealth;
    public float maxHealth => m_maxHealth;
    [SerializeField] private float m_health;
    public float health => m_health;
    [SerializeField] private int m_score;
    public int score => m_score;
    [SerializeField] private float m_damage;
    public float damage => m_damage;

    private int dmgIncCost = 30;
    public int dmgCost => dmgIncCost;
    private int healthIncCost = 30;
    public int hpCost => healthIncCost;

    private void Awake() {
        if (current != null) {
            Destroy(gameObject);
            return;
        }
        // Set this component as the singleton class
        current = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetHealth(float h) {
        m_health = h;
    }
    public void SetScore(int r) {
        m_score = r;
    }
    /*
    public void IncreaseMaxHealth()
    {
        if (m_resources > healthIncCost)
        {
            m_resources -= healthIncCost;
            m_maxHealth += 20;
            healthIncCost *= 2;
            m_health = maxHealth;
        }
    }
    public void IncreaseDamage() 
    {
        if (m_resources > dmgIncCost)
        {
            m_resources -= dmgIncCost;
            m_damage += 5;
            dmgIncCost *= 2;
        }
    }
    */
}

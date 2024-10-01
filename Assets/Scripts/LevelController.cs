using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Splines;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelController : MonoBehaviour
{
    public static LevelController current;

    [Header("=== Paths ===")]
    [SerializeField] private Transform m_endpointParent;
    [SerializeField] private Transform[] m_endpoints;
    public Transform[] endpoints => m_endpoints;
    [SerializeField] private List<PathAsset> m_paths;
    [SerializeField] private bool m_debugShowEndpoints;
    [SerializeField] private Dictionary<string, Path> m_pathsDict;
    [SerializeField] private MatchAsset m_match;
    [SerializeField] private int m_checkpointIndex = 0;

    [Header("=== Player Stats ===")]
    /*
    [SerializeField] private float m_maxHealth;
    [SerializeField] private float m_currentHealth;
    */
    [SerializeField] private int m_score;
    [SerializeField] private float m_damage;

    [Header("=== UI BOOLS ===")]
    [SerializeField] private bool m_spawningEnemies;
    [SerializeField] private bool roundStarted;

    [Header("=== UI SCREENS ===")]
    public GameObject m_playerUI;
    public GameObject m_rdyScreen;
    public GameObject m_winScreen;
    public GameObject m_loseScreen;
    public GameObject m_retryButton;
    /*
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI hpCostText;
    public TextMeshProUGUI dmgText;
    public TextMeshProUGUI dmgCostText;
    public Slider hp;
    */
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI endScoreText;

    private IEnumerator m_spawnCoroutine;

    public List<Enemy> toDelete = new List<Enemy>();

    [SerializeField] private List<Enemy> m_activeEnemies = new List<Enemy>();
    private HashSet<Bullet> m_activeBullets = new HashSet<Bullet>();
    
    //public GameObject m_playerUI;

    #if UNITY_EDITOR
    private bool showGizmos => m_debugShowEndpoints;
    private void OnDrawGizmos() {
        if (!Application.isPlaying || !showGizmos) return;
        Gizmos.color = Color.yellow;
        for(int i = 0; i < m_endpoints.Length; i++) {
            Gizmos.DrawSphere(m_endpoints[i].position, 0.1f);
        }
    }
    #endif

    private void Awake()
    {
        // Singleton Logic
        current = this;
        
        // Setting the spawn cycle coroutine reference early
        m_spawnCoroutine = SpawnCycle();

        // Disabling and enabling the different UI canvas
        m_playerUI.SetActive(false);
        m_winScreen.SetActive(false);
        m_loseScreen.SetActive(false);
        m_spawningEnemies = false;
        roundStarted = false;
    }

    private void Start()
    {
        // Based on the Vector2Int `m_numLanes`, we determine the appropriate positions for 
        CalculatePaths();

        // Extract the resources of the player from World Cache
        m_score = WorldCache.current.score;

        // Make the ready screen visible
        m_rdyScreen.SetActive(true);
    }

    private void Update()
    {
        // If the round has started, we must update the relevant UI texts and other elements
        if (roundStarted) {
            scoreText.text = "Score: " + m_score.ToString();
            //hp.value = PlayerController.current.healthRatio;
        }

        //if round has started, no more enemies are spawning, and all enemies are gone
        if (m_activeEnemies.Count == 0 && !m_spawningEnemies && roundStarted) {
            PlayerController.current.ResetHealth();
            m_winScreen.SetActive(true);
            /*
            //update ui
            hpText.text = "HP - " + WorldCache.current.health;
            dmgText.text = "DMG - " + WorldCache.current.damage;
            hpCostText.text = WorldCache.current.hpCost.ToString();
            dmgCostText.text = WorldCache.current.dmgCost.ToString();
            */
        }
    }

    //when ready button pressed start round
    public void ReadyForRound() {
        // Set canvas UI
        m_rdyScreen.SetActive(false);
        m_playerUI.SetActive(true);

        // Set relevant booleans
        roundStarted = true;
        m_spawningEnemies = true;

        //start spawning enemies
        StartCoroutine(m_spawnCoroutine);
    }

    public void ResetRound()
    {
        m_spawningEnemies = false;
        roundStarted = false;
        m_loseScreen.SetActive(false);
        m_winScreen.SetActive(false);
        m_score = 0;
        deleteAllBullets();
        while(m_activeEnemies.Count > 0) {
            if (m_activeEnemies[0] != null) {
                GameObject eg = m_activeEnemies[0].gameObject;
                Destroy(eg);
            }
            m_activeEnemies.RemoveAt(0);
        }
        m_checkpointIndex = 0;
        PlayerController.current.ResetPosition();
        PlayerController.current.TogglePlayerActions(true);
        m_spawnCoroutine = SpawnCycle();
        ReadyForRound();
    }
    public void ResetRoundFromCheckpoint() {
        m_spawningEnemies = false;
        roundStarted = false;
        m_loseScreen.SetActive(false);
        m_winScreen.SetActive(false);
        m_score = 0;
        deleteAllBullets();
        while(m_activeEnemies.Count > 0) {
            if (m_activeEnemies[0] != null) {
                GameObject eg = m_activeEnemies[0].gameObject;
                Destroy(eg);
            }
            m_activeEnemies.RemoveAt(0);
        }
        PlayerController.current.ResetPosition();
        PlayerController.current.TogglePlayerActions(true);
        m_spawnCoroutine = SpawnCycle(m_checkpointIndex);
        ReadyForRound();
    }

    // This special function deals with calculating the start and end positions of each enemy
    private void CalculatePaths() {
        // Given the aspect ratio's determination of the world width and height...
        float worldWidth = AspectRatioController.current.worldWidth;
        float worldHeight = AspectRatioController.current.worldHeight;

        // .. We scale ourselves to match them
        m_endpointParent.localScale = new Vector3(worldWidth+0.5f, worldHeight+0.5f, 1f);

        // We then need to read the match scriptable object asset that contains all the paths (still in 1:1:1 space) and adjusts them
        m_pathsDict = new Dictionary<string, Path>();
        foreach(PathAsset pa in m_paths) {
            if (m_pathsDict.ContainsKey(pa.name)) {
                Debug.LogError($"ERROR: Path with name \"{pa.name}\" has a duplicate!");
                continue;
            }
            if (pa.startEndpointIndex >= m_endpoints.Length || pa.endEndpointIndex >= m_endpoints.Length) {
                Debug.LogError($"ERROR: Path with endpoints {pa.startEndpointIndex} and {pa.endEndpointIndex} don't match the listed endpoints!");
                continue;
            }
            Vector3 s = m_endpoints[pa.startEndpointIndex].position;
            Vector3 e = m_endpoints[pa.endEndpointIndex].position;
            float d = Vector3.Distance(s,e);
            m_pathsDict.Add(pa.name, new Path(pa.name, s, e, d));
        }
    }

    
    private IEnumerator SpawnCycle(int startIndex = 0) {
        for(int i = startIndex; i < m_match.matches.Count; i++) {
            Match match = m_match.matches[i];
            if (!match.active) continue;
            if (match.setCheckpoint) m_checkpointIndex = i;

            // Wait for the predefined number of seconds
            yield return new WaitForSeconds(match.timeToWait);

            // Only spawn an enemy if it's actually set.
            if (match.enemy != null) {
                if (!m_pathsDict.ContainsKey(match.pathAsset.pathName)) {
                    Debug.LogError($"ERROR: Cannot spawn enemy who wants to spawn on path \"{match.pathAsset.pathName}\"");
                    continue;
                }
                Path p = m_pathsDict[match.pathAsset.pathName];
                Vector3 s = match.reverse ? p.end + match.offset : p.start + match.offset;
                Enemy e = Instantiate(match.enemy, s, Quaternion.identity) as Enemy;
                e.Initialize(p, match.reverse, match.speedCurve, match.offset);
                m_activeEnemies.Add(e);
            }
        }
        m_spawningEnemies = false;
    }

    private void LateUpdate() {
        if (toDelete.Count == 0) return;
        while(toDelete.Count > 0) {
            Enemy e = toDelete[0];
            toDelete.Remove(e);
            Destroy(e.gameObject); 
        }
    }

    private void OnDestroy() {
        StopCoroutine(m_spawnCoroutine);
        
    }
    private void OnApplicationQuit() {
        StopCoroutine(m_spawnCoroutine);
    }

    public void GameOver()
    {
        endScoreText.text = "SCORE \n" + m_score.ToString();
        Debug.Log("Game Over!");
        // Implement game over logic, load game over scene, stop spawning enemies, etc.
        StopCoroutine(m_spawnCoroutine);  // Stop enemy spawning
        // Additional game over handling here
        PlayerController.current.TogglePlayerActions(false);
        m_retryButton.SetActive(m_checkpointIndex > 0);
        m_playerUI.SetActive(false);
        m_loseScreen.SetActive(true);

        foreach(Enemy e in m_activeEnemies)
        {
            if (e != null && e.tag == "Boss")
            {
                Destroy(e.gameObject);
            }        
        }
    }

    public void EnemyDied(Enemy e)
    {
        Debug.Log("Enemy Died");
        if (m_loseScreen.activeSelf == false)
        {
            m_score += e.resources;
        }

        if (m_activeEnemies.Contains(e)) {
            m_activeEnemies.Remove(e);
        }
        Destroy(e.gameObject);
    }
    
    public void EnemyDespawn(Enemy e)
    {
        Debug.Log("Enemy Despawned");
        if (m_activeEnemies.Contains(e)) {
            m_activeEnemies.Remove(e);
        }
        Destroy(e.gameObject);
    }

    public void addBullet(Bullet b)
    {
        m_activeBullets.Add(b);
    }

    private void deleteAllBullets()
    {
        foreach (Bullet b in m_activeBullets)
        {
            if (b != null)
            {
                Destroy(b.gameObject);
            }
        }
        m_activeBullets.Clear();
    }
}

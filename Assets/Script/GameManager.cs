using UnityEngine;
using TMPro; 
using UnityEngine.UI; // สำหรับจัดการ Slider (หลอดเลือด)
using UnityEngine.SceneManagement; // สำหรับจัดการ Scene (ปุ่มเล่นใหม่)

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("ระบบคะแนน")]
    public int score = 0;
    public int combo = 0;

    [Header("UI คะแนนและคอมโบ")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;

    [Header("รูปภาพ Judgment ที่จะให้เด้ง")]
    public GameObject perfectPrefab;
    public GameObject greatPrefab;
    public GameObject missPrefab;

    [Header("ระบบเลือดและฉากจบ")]
    public int maxHP = 100;
    public int currentHP;
    public Slider hpSlider;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    private bool isGameOver = false; // เอาไว้เช็คว่าเกมจบหรือยัง

    // 👇👇👇 สิ่งที่เพิ่มเข้ามาใหม่: ระบบเพลง 👇👇👇
    [Header("ระบบเพลง")]
    public AudioSource bgm;
    // 👆👆👆 ============================== 👆👆👆

    private bool isShuttingDown = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        currentHP = maxHP;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        Time.timeScale = 1f; 

        UpdateUI();
    }

    // 👇👇👇 สิ่งที่เพิ่มเข้ามาใหม่: เช็คเพลงจบ 👇👇👇
    void Update()
    {
        // ถ้าเกมยังไม่จบ และมีการใส่เพลงไว้
        if (!isGameOver && bgm != null)
        {
            // ถ้าเพลงหยุดเล่นแล้ว (เล่นจนจบเพลง)
            if (!bgm.isPlaying)
            {
                Victory(); // สั่งให้ชนะทันที!
            }
        }
    }
    // 👆👆👆 ============================== 👆👆👆

    public void AddScore(int baseAmount)
    {
        if (isGameOver) return; 

        combo++; 

        float multiplier = 1.0f; 

        if (combo >= 50) multiplier = 1.5f; 
        else if (combo >= 40) multiplier = 1.4f; 
        else if (combo >= 30) multiplier = 1.3f; 
        else if (combo >= 20) multiplier = 1.2f; 
        else if (combo >= 10) multiplier = 1.1f; 

        int finalScore = Mathf.RoundToInt(baseAmount * multiplier);
        score += finalScore;

        UpdateUI();
    }

    public void ResetCombo()
    {
        if (isGameOver) return; 
        combo = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = score.ToString("D6"); 

        if (comboText != null) 
        {
            if (combo > 0) 
            {
                comboText.gameObject.SetActive(true); 
                comboText.text = combo.ToString();
            }
            else 
            {
                comboText.gameObject.SetActive(false); 
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isGameOver || isShuttingDown) return; 

        currentHP -= damageAmount;
        if (hpSlider != null) hpSlider.value = currentHP;

        if (currentHP <= 0)
        {
            currentHP = 0;
            GameOver();
        }
    }

    public void Heal(int healAmount)
    {
        if (isGameOver || isShuttingDown) return;

        currentHP += healAmount;
        if (currentHP > maxHP) currentHP = maxHP;

        if (hpSlider != null) hpSlider.value = currentHP;
    }

    public void GameOver()
    {
        isGameOver = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void Victory()
    {
        if (isGameOver) return; 
        isGameOver = true;
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
        Time.timeScale = 1f; 
    }

    void OnApplicationQuit()
    {
        isShuttingDown = true; 
    }

    public void ShowJudgment(string type, Transform spawnLocation)
    {
        if (isShuttingDown) return;

        GameObject prefabToSpawn = null;

        if (type == "Perfect") prefabToSpawn = perfectPrefab;
        else if (type == "Great") prefabToSpawn = greatPrefab;
        else if (type == "Miss") prefabToSpawn = missPrefab;

        if (prefabToSpawn != null && spawnLocation != null)
        {
            Vector3 offsetPos = spawnLocation.position + new Vector3(-1.5f, 1.5f, 0); 
            Instantiate(prefabToSpawn, offsetPos, Quaternion.identity);
        }
    }
}
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

    private bool isShuttingDown = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // ตั้งค่าเลือดเริ่มต้น
        currentHP = maxHP;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        // ซ่อนหน้าจอจบเกมตอนเริ่ม
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        Time.timeScale = 1f; // ทำให้เวลาเดินปกติ

        UpdateUI();
    }

    public void AddScore(int baseAmount)
    {
        if (isGameOver) return; // ถ้าตายแล้ว ไม่ต้องบวกคะแนนเพิ่ม

        combo++; 

        // ระบบคูณคะแนนสไตล์ Muse Dash: เพิ่มโบนัส 10% ทุกๆ 10 คอมโบ (สูงสุด 50%)
        float multiplier = 1.0f; // เริ่มต้นที่ 100% ของคะแนนปกติ

        if (combo >= 50) {
            multiplier = 1.5f; // คอมโบ 50 ขึ้นไป ได้คะแนน 150%
        }
        else if (combo >= 40) {
            multiplier = 1.4f; // คอมโบ 40-49 ได้คะแนน 140%
        }
        else if (combo >= 30) {
            multiplier = 1.3f; // คอมโบ 30-39 ได้คะแนน 130%
        }
        else if (combo >= 20) {
            multiplier = 1.2f; // คอมโบ 20-29 ได้คะแนน 120%
        }
        else if (combo >= 10) {
            multiplier = 1.1f; // คอมโบ 10-19 ได้คะแนน 110%
        }

        // เอาคะแนนมาคูณ แล้วปัดเศษให้เป็นเลขจำนวนเต็มล้วนๆ
        int finalScore = Mathf.RoundToInt(baseAmount * multiplier);
        
        score += finalScore;

        UpdateUI();
    }

    public void ResetCombo()
    {
        if (isGameOver) return; // ถ้าตายแล้ว ไม่ต้องอัปเดตคอมโบ
        combo = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        // 1. จัดการตัวเลขคะแนน (Score)
        if (scoreText != null) 
        {
            scoreText.text = score.ToString("D6"); 
        }

        // 2. จัดการตัวเลขคอมโบ (Combo)
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

    // --- ฟังก์ชันลดเลือดเวลาตีพลาด ---
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

    // --- ฟังก์ชันเพิ่มเลือดเมื่อตีโดน ---
    public void Heal(int healAmount)
    {
        if (isGameOver || isShuttingDown) return;

        currentHP += healAmount;

        // ป้องกันไม่ให้เลือดเกินค่าสูงสุดที่ตั้งไว้ (Max HP)
        if (currentHP > maxHP) currentHP = maxHP;

        // อัปเดตหลอดเลือดบนจอ
        if (hpSlider != null) hpSlider.value = currentHP;
    }

    // --- ฟังก์ชันแพ้ ---
    public void GameOver()
    {
        isGameOver = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // หยุดเวลาในเกม
    }

    // --- ฟังก์ชันชนะ ---
    public void Victory()
    {
        if (isGameOver) return; 
        isGameOver = true;
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f; // หยุดเวลาในเกม
    }

    // --- ฟังก์ชันสำหรับผูกกับปุ่ม เล่นใหม่ ---
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
        Time.timeScale = 1f; 
    }

    // เพิ่มฟังก์ชันเช็คสถานะการปิดเกม
    void OnApplicationQuit()
    {
        isShuttingDown = true; 
    }

    // ฟังก์ชันรับค่า 2 อย่าง: type (รูปอะไร) และ spawnLocation (ตำแหน่งเลนไหน)
    public void ShowJudgment(string type, Transform spawnLocation)
    {
        // --- เพิ่มบรรทัดนี้เพื่อดักไว้ ---
        if (isShuttingDown) return;

        GameObject prefabToSpawn = null;

        if (type == "Perfect") prefabToSpawn = perfectPrefab;
        else if (type == "Great") prefabToSpawn = greatPrefab;
        else if (type == "Miss") prefabToSpawn = missPrefab;

        if (prefabToSpawn != null && spawnLocation != null)
        {
            // แกน X ใส่ -1.5f เพื่อให้ขยับซ้าย, แกน Y ใส่ 1.5f เพื่อให้ลอยขึ้น
            Vector3 offsetPos = spawnLocation.position + new Vector3(-1.5f, 1.5f, 0); 

            Instantiate(prefabToSpawn, offsetPos, Quaternion.identity);
        }
    }
}
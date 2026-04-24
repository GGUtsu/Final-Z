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

    [Header("UI สรุปผลตอนจบเกม")]
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverHighScoreText;
    public TextMeshProUGUI victoryScoreText;
    public TextMeshProUGUI victoryHighScoreText;

    // 👇👇👇 สิ่งที่เพิ่มเข้ามาใหม่: ระบบเพลง 👇👇👇
    [Header("ระบบเพลง")]
    public AudioSource bgm;
    // 👆👆👆 ============================== 👆👆👆

    [Header("หน้าต่างตั้งค่า (Settings UI)")]
    public GameObject soundSettingPanel;

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
    private bool hasSongStarted = false;
    public bool isChangingScene = false; // ป้องกันบั๊ก Victory เด้งตอนเปลี่ยนฉาก

    void Update()
    {
        if (isChangingScene) return;

        // ถ้าเกมยังไม่จบ และมีการใส่เพลงไว้
        if (!isGameOver && bgm != null)
        {
            // ดักจับว่าเพลงได้เริ่มเล่นแล้วหรือยัง (กันบั๊กตอนเริ่มเกมใหม่ๆ)
            if (bgm.isPlaying)
            {
                hasSongStarted = true;
            }

            // ถ้าเพลงเคยเริ่มเล่นไปแล้ว + ตอนนี้เพลงหยุดเล่นแล้ว + ไม่ได้อยู่ในหน้าจอ Pause (Time.timeScale > 0)
            if (hasSongStarted && !bgm.isPlaying && Time.timeScale > 0f)
            {
                // เช็คว่าเพลงหยุดเพราะเล่นจบจริงๆ ใช่ไหม (เวลาเพลงจบ ค่า time จะถูกรีเซ็ตเป็น 0 หรือไปถึงจุดจบ)
                // ป้องกันปัญหาตอนพับจอ (Tab out) แล้วเกมคิดว่าเพลงจบ
                if (bgm.time == 0f || (bgm.clip != null && bgm.time >= bgm.clip.length - 0.1f))
                {
                    Victory(); // สั่งให้ชนะทันที!
                }
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
        if (scoreText != null) scoreText.text = score.ToString(); 

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

        // หยุดเพลงตอนตาย
        if (bgm != null) bgm.Stop();

        SaveAndDisplayHighScore(gameOverScoreText, gameOverHighScoreText);
    }

    public void Victory()
    {
        if (isGameOver) return; 
        isGameOver = true;
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f; 

        // เผื่อไว้ให้หยุดเพลงเหมือนกัน (กรณีเวลาจบพอดีแล้วเสียงมันค้าง)
        if (bgm != null) bgm.Stop();

        SaveAndDisplayHighScore(victoryScoreText, victoryHighScoreText);
    }

    private void SaveAndDisplayHighScore(TextMeshProUGUI finalScoreText, TextMeshProUGUI highScoreText)
    {
        // ใช้ชื่อ Scene ปัจจุบันเป็น Key สำหรับเซฟ High Score ของด่านนี้
        string sceneName = SceneManager.GetActiveScene().name;
        string highScoreKey = "HighScore_" + sceneName;

        // ดึงค่า High Score เดิมออกมา (ถ้าไม่มีจะเป็น 0)
        int highScore = PlayerPrefs.GetInt(highScoreKey, 0);

        // ถ้าคะแนนรอบนี้มากกว่า High Score เดิม ให้เซฟใหม่
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(highScoreKey, highScore);
            PlayerPrefs.Save();
        }

        // แสดงผลขึ้นจอ
        if (finalScoreText != null) finalScoreText.text = score.ToString();
        if (highScoreText != null) highScoreText.text = highScore.ToString();
    }

    public void RestartGame()
    {
        isChangingScene = true;
        Time.timeScale = 1f; 
        AudioListener.pause = false; // ป้องกันเสียงค้าง
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    // ฟังก์ชันสำหรับปุ่ม Menu ในหน้า Game Over / Victory เพื่อกลับไปหน้า Setting A
    public void LoadSettingA()
    {
        isChangingScene = true;
        Time.timeScale = 1f; 
        AudioListener.pause = false; // ป้องกันเสียงค้าง/หายเวลาเปลี่ยนซีน
        SceneManager.LoadScene("Setting A"); 
    }

    // ฟังก์ชันเสริม เผื่อต้องการกำหนดชื่อ Scene เองในปุ่ม (ให้เลือกใช้แบบ Dynamic string)
    public void LoadSceneByName(string sceneName)
    {
        isChangingScene = true;
        Time.timeScale = 1f; 
        AudioListener.pause = false;
        SceneManager.LoadScene(sceneName); 
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

    // ฟังก์ชันสำหรับเปิดหน้าต่างตั้งค่าเสียง
    public void OpenSoundSettingPanel()
    {
        if (soundSettingPanel != null) soundSettingPanel.SetActive(true);
    }

    // ฟังก์ชันสำหรับปิดหน้าต่างตั้งค่าเสียง
    public void CloseSoundSettingPanel()
    {
        if (soundSettingPanel != null) soundSettingPanel.SetActive(false);
    }
}
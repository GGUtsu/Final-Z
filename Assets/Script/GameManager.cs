using UnityEngine;
using TMPro; 

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
    // (ลบตัวแปร judgmentSpawnPoint ตรงกลางจอออกไปแล้ว เพราะเราจะใช้ตำแหน่งของแต่ละเลนแทน)

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        combo++;
        UpdateUI();
    }

    public void ResetCombo()
    {
        combo = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "คะแนน: " + score;
        if (comboText != null) comboText.text = "คอมโบ: " + combo;
    }

    // ฟังก์ชันรับค่า 2 อย่าง: type (รูปอะไร) และ spawnLocation (ตำแหน่งเลนไหน)
    public void ShowJudgment(string type, Transform spawnLocation)
    {
        GameObject prefabToSpawn = null;

        if (type == "Perfect") prefabToSpawn = perfectPrefab;
        else if (type == "Great") prefabToSpawn = greatPrefab;
        else if (type == "Miss") prefabToSpawn = missPrefab;

        // เสกรูปออกมาที่ตำแหน่ง spawnLocation ที่เลนนั้นๆ ส่งมาให้
        if (prefabToSpawn != null && spawnLocation != null)
        {
            Instantiate(prefabToSpawn, spawnLocation.position, Quaternion.identity);
        }
    }
}
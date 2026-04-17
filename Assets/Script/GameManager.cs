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

    // เพิ่มตัวแปรนี้เข้าไป
    private bool isShuttingDown = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddScore(int baseAmount)
    {
        // 1. บวกคอมโบเพิ่มไปก่อน 1
        combo++; 

        // 2. ระบบคำนวณตัวคูณ (Multiplier) ตาม GDD ของคุณ
        int multiplier = 1; // เริ่มต้นที่ x1

        // ตัวอย่างการตั้งค่าตัวคูณ: (แก้ตัวเลขตรงนี้ให้ตรงกับ GDD ได้เลย)
        if (combo >= 50) {
            multiplier = 4; // คอมโบ 50 ขึ้นไป เอาคะแนน x4
        }
        else if (combo >= 30) {
            multiplier = 3; // คอมโบ 30-49 เอาคะแนน x3
        }
        else if (combo >= 10) {
            multiplier = 2; // คอมโบ 10-29 เอาคะแนน x2
        }

        // 3. เอาคะแนนพื้นฐาน (100 หรือ 50) ไปคูณกับ multiplier
        int finalScore = baseAmount * multiplier;
        
        // 4. เอาคะแนนที่คูณแล้วไปบวกเข้าคะแนนรวม
        score += finalScore;

        UpdateUI();
    }

    public void ResetCombo()
    {
        combo = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        // 1. จัดการตัวเลขคะแนน (Score)
        if (scoreText != null) 
        {
            // ใช้ "D6" เพื่อให้มีเลข 0 นำหน้าให้ครบ 6 หลัก (เช่น 000150) 
            // จะทำให้ดูเป็นเกมแนวมิวสิค/อาเขตมากขึ้นครับ
            scoreText.text = score.ToString("D6"); 
            
            // ปล. ถ้าอยากได้แค่เลขเพียวๆ ไม่มี 0 นำหน้า ให้เปลี่ยนเป็น: 
            // scoreText.text = score.ToString(); 
        }

        // 2. จัดการตัวเลขคอมโบ (Combo)
        if (comboText != null) 
        {
            // ทริคเกม Rhythm: ถ้าคอมโบยังเป็น 0 จะซ่อนตัวเลขไว้ก่อน 
            // พอตีโดนตัวแรก (คอมโบ > 0) ค่อยโชว์ตัวเลข
            if (combo > 0) 
            {
                comboText.gameObject.SetActive(true); // เปิดโชว์ตัวหนังสือ
                comboText.text = combo.ToString();
            }
            else 
            {
                comboText.gameObject.SetActive(false); // ซ่อนตัวหนังสือตอนหลุดคอมโบ
            }
        }
    }

    // เพิ่มฟังก์ชันเช็คสถานะการปิดเกม
    void OnApplicationQuit()
    {
        isShuttingDown = true; // บอกให้รู้ว่าเกมกำลังจะปิดแล้วนะ
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
            // --- เปลี่ยนตรงนี้ ---
            // แกน X ใส่ -1.5f เพื่อให้ขยับซ้าย, แกน Y ใส่ 1.5f เพื่อให้ลอยขึ้น
            // (สามารถปรับตัวเลข -1.5f ให้มากหรือน้อยลงได้ตามชอบเลยครับ)
            Vector3 offsetPos = spawnLocation.position + new Vector3(-1.5f, 1.5f, 0); 

            Instantiate(prefabToSpawn, offsetPos, Quaternion.identity);
        }
    }
}
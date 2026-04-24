using UnityEngine;
using TMPro;

public class HighScoreDisplay : MonoBehaviour
{
    [Header("ตั้งค่าโชว์คะแนน")]
    [Tooltip("ใส่ชื่อ Scene ของด่านที่ต้องการดึงคะแนนมาโชว์ (ต้องพิมพ์ให้ตรงกับชื่อ Scene ที่เล่น)")]
    public string targetSceneName;

    [Tooltip("ลาก Text ที่อยู่ในหน้า Setting A มาใส่ตรงนี้")]
    public TextMeshProUGUI highScoreText;

    void Start()
    {
        // ดึงคะแนน High Score ของฉากที่กำหนดมาจาก PlayerPrefs (ถ้าไม่เคยเล่นจะได้ 0)
        string key = "HighScore_" + targetSceneName;
        int savedHighScore = PlayerPrefs.GetInt(key, 0);

        // อัปเดตข้อความบนหน้าจอ
        if (highScoreText != null)
        {
            // สามารถแก้คำว่า "High Score: " เป็นอะไรก็ได้ตามต้องการ
            highScoreText.text = savedHighScore.ToString(); 
        }
    }
}

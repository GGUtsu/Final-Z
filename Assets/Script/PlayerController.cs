using UnityEngine;

public class PlayerController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) 
        {
            AttackLane("Top");
        }
        else if (Input.GetKeyDown(KeyCode.J)) 
        {
            AttackLane("Bottom");
        }
    }

    void AttackLane(string lane)
    {
        // 1. เล่นแอนิเมชันตัวละครโจมตีเลนบนหรือล่าง
        // 2. เรียกฟังก์ชันเช็ค Hit Window สั่งทำลายโน้ตและคำนวณคะแนน
    }
}
using System.Collections.Generic;
using UnityEngine;

public class HitZone : MonoBehaviour
{
    [Header("ตั้งค่าปุ่มกด")]
    public KeyCode hitKey;
    public string laneName;

    [Header("ตั้งค่าภาพเป้ารับโน้ต (Visual)")]
    public SpriteRenderer buttonGraphic; // ลากภาพวงกลมเป้าหมายมาใส่ช่องนี้
    public Color defaultColor = Color.white; // สีปกติตอนไม่ได้กด
    public Color pressedColor = Color.gray;  // สีที่จะเปลี่ยนตอนนิ้วกดลงไป

    private List<GameObject> notesInZone = new List<GameObject>();

    void Update()
    {
        // จังหวะกดปุ่ม
        if (Input.GetKeyDown(hitKey))
        {
            if (buttonGraphic != null) buttonGraphic.color = pressedColor; 

            // ถ้ามีโน้ตในกล่อง
            if (notesInZone.Count > 0)
            {
                GameObject noteToHit = notesInZone[0];
                float distance = Mathf.Abs(noteToHit.transform.position.x - transform.position.x);

                // --- เปลี่ยนตัวเลขคะแนนตรงนี้ครับ ---
                if (distance <= 0.5f) 
                {
                    // Perfect (ได้ 1000 คะแนน + เพิ่มเลือด 4 ส่วน)
                    if(GameManager.instance != null) {
                        GameManager.instance.AddScore(1000); 
                        GameManager.instance.Heal(4); // <-- เพิ่มบรรทัดนี้
                        GameManager.instance.ShowJudgment("Perfect", transform); 
                    }
                }
                else 
                {
                    // Great (ได้ 500 คะแนน + เพิ่มเลือด 2 ส่วน)
                    if(GameManager.instance != null) {
                        GameManager.instance.AddScore(500); 
                        GameManager.instance.Heal(2); // <-- เพิ่มบรรทัดนี้
                        GameManager.instance.ShowJudgment("Great", transform); 
                    }
                }

                notesInZone.Remove(noteToHit);

                // 👇👇👇 เพิ่มบรรทัดนี้เข้าไปสับขาหลอก Unity! 👇👇👇
                noteToHit.tag = "Untagged"; 

                Destroy(noteToHit);
            }
        }

        // จังหวะปล่อยปุ่ม
        if (Input.GetKeyUp(hitKey))
        {
            if (buttonGraphic != null) buttonGraphic.color = defaultColor;
        }
    }

    // เอาโน้ตเข้า List เมื่อวิ่งเข้ามาในกล่อง
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Note")) notesInZone.Add(other.gameObject);
    }

    // ปล่อยโน้ตหลุดกรอบไป (ลบออก + ตัดคอมโบ + เด้ง Miss + ลดเลือด 10)
    private void OnTriggerExit2D(Collider2D collision)
    {
        // ถ้าสิ่งที่หลุดกรอบออกไปคือ "โน้ต"
        if (collision.CompareTag("Note"))
        {
            notesInZone.Remove(collision.gameObject);

            if (GameManager.instance != null)
            {
                GameManager.instance.ResetCombo(); // ตัดคอมโบ
                GameManager.instance.ShowJudgment("Miss", transform); // โชว์รูป Miss

                // --- เพิ่มคำสั่งลดเลือด 10 หน่วย ตรงนี้ครับ! ---
                GameManager.instance.TakeDamage(10); 
            }

            Destroy(collision.gameObject); // ทำลายโน้ตทิ้ง
        }
    }
}
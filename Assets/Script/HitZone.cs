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
                    // ระยะแม่นยำ = Perfect (ได้ฐาน 1000 คะแนน)
                    if(GameManager.instance != null) {
                        GameManager.instance.AddScore(1000); 
                        GameManager.instance.ShowJudgment("Perfect", transform); 
                    }
                }
                else 
                {
                    // ระยะ Great (ได้ฐาน 500 คะแนน หรือจะปรับตาม GDD ก็ได้ครับ)
                    if(GameManager.instance != null) {
                        GameManager.instance.AddScore(500); 
                        GameManager.instance.ShowJudgment("Great", transform); 
                    }
                }

                notesInZone.Remove(noteToHit);
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

    // ปล่อยโน้ตหลุดกรอบไป (ลบออก + ตัดคอมโบ + เด้ง Miss)
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Note") && notesInZone.Contains(other.gameObject))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.ResetCombo();
                GameManager.instance.ShowJudgment("Miss", transform); // เพิ่ม transform เข้าไป
            }
            notesInZone.Remove(other.gameObject);
            Destroy(other.gameObject);
        }
    }
}
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
        // 1. จังหวะที่นิ้วกดปุ่มลงไป (เปลี่ยนสี + เช็คตีโน้ต)
        if (Input.GetKeyDown(hitKey))
        {
            // เปลี่ยนสีปุ่มให้ดูเหมือนโดนกด
            if (buttonGraphic != null) buttonGraphic.color = pressedColor; 

            // --- ระบบเช็คการตีโน้ต ---
            if (notesInZone.Count > 0)
            {
                GameObject noteToHit = notesInZone[0];
                float distance = Mathf.Abs(noteToHit.transform.position.x - transform.position.x);

                // --- เปลี่ยนมาเรียก GameManager แบบใหม่ --- (เพิ่ม transform เข้าไป)
                if (distance <= 0.5f) 
                {
                    if(GameManager.instance != null) 
                    {
                        GameManager.instance.AddScore(100);
                        GameManager.instance.ShowJudgment("Perfect", transform); 
                    }
                }
                else if (distance <= 1.5f) 
                {
                    if(GameManager.instance != null) 
                    {
                        GameManager.instance.AddScore(50);
                        GameManager.instance.ShowJudgment("Great", transform); 
                    }
                }
                else 
                {
                    if(GameManager.instance != null) 
                    {
                        GameManager.instance.ResetCombo();
                        GameManager.instance.ShowJudgment("Miss", transform); 
                    }
                }

                notesInZone.Remove(noteToHit);
                Destroy(noteToHit);
            }
        }

        // 2. จังหวะที่ปล่อยนิ้วออกจากปุ่ม (ให้สีกลับมาเป็นปกติ)
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
            if(GameManager.instance != null)
            {
                GameManager.instance.ResetCombo();
                GameManager.instance.ShowJudgment("Miss", transform); // เพิ่ม transform เข้าไป
            }
            notesInZone.Remove(other.gameObject);
            Destroy(other.gameObject); 
        }
    }
}
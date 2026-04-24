using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("การตั้งค่าแอนิเมชัน")]
    public Animator anim; // ลากตัวละครที่มี Animator มาใส่ช่องนี้

    [Header("ชื่อ Trigger ใน Animator")]
    public string attackTopTrigger = "AttackTop";    // ชื่อ Trigger สำหรับท่ากด F
    public string attackBottomTrigger = "AttackBottom"; // ชื่อ Trigger สำหรับท่ากด K

    [Header("การกลับสู่ท่าวิ่ง/ท่ายืน")]
    public string idleStateName = "Run"; // พิมพ์ชื่อ State ท่าวิ่งหรือยืนใน Animator

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) 
        {
            AttackLane("Top");
        }
        // เปลี่ยนจากปุ่ม J เป็น K ตามที่คุณขอครับ
        else if (Input.GetKeyDown(KeyCode.K)) 
        {
            AttackLane("Bottom");
        }
    }

    void AttackLane(string lane)
    {
        // 1. เล่นแอนิเมชันตัวละครโจมตีเลนบนหรือล่าง
        if (anim != null)
        {
            if (lane == "Top")
            {
                anim.SetTrigger(attackTopTrigger); // สั่งเล่นท่าตอนกด F
            }
            else if (lane == "Bottom")
            {
                anim.SetTrigger(attackBottomTrigger); // สั่งเล่นท่าตอนกด K
            }

            // รีเซ็ตเวลากลับไปท่ายืนใหม่ (เผื่อผู้เล่นกดตีรัวๆ)
            StopAllCoroutines();
            StartCoroutine(ReturnToIdle());
        }
    }

    private System.Collections.IEnumerator ReturnToIdle()
    {
        // ต้องรอให้ผ่านไป 1 เฟรมก่อน เพื่อให้ Animator เปลี่ยน State ท่าตีก่อน ถึงจะดึงเวลาได้ถูก
        yield return null; 
        
        if (anim != null)
        {
            // เช็คความยาวของแอนิเมชันท่าที่กำลังเล่นอยู่ (ท่าตี/ท่าเตะ) แบบเป๊ะๆ
            float currentAnimLength = anim.GetCurrentAnimatorStateInfo(0).length;
            
            // รอจนกว่าท่าตีจะเล่นจบตามความยาวของมัน
            yield return new WaitForSeconds(currentAnimLength);
            
            // สั่งให้กลับไปท่าวิ่ง(หรือ Idle) ทันที
            anim.Play(idleStateName);
        }
    }
}
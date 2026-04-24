using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Background Settings")]
    public GameObject backgroundPrefab; 
    
    public float scrollSpeed = 5f;
    
    [Tooltip("ใส่ 0 ไว้ให้ระบบคำนวณขนาดภาพอัตโนมัติ (ช่วยแก้ปัญหาภาพไม่ต่อกัน)")]
    public float backgroundWidth = 0f;
    
    [Tooltip("ตำแหน่งแกน X ซ้ายสุดที่ฉากจะถูกย้ายไปต่อท้าย")]
    public float destroyX = -25f;
    
    // จำนวนฉากที่จะสร้างเวียน (ใช้แค่ 3 รูปวนลูปไปเรื่อยๆ)
    private int poolSize = 3; 
    private List<GameObject> backgrounds = new List<GameObject>();

    void Start()
    {
        if (backgroundPrefab == null)
        {
            Debug.LogError("BackgroundScroller: Please assign a background prefab in the inspector!");
            return;
        }

        // คำนวณความกว้างอัตโนมัติจาก SpriteRenderer (ถ้าผู้ใช้ตั้งเป็น 0)
        if (backgroundWidth <= 0f)
        {
            SpriteRenderer sr = backgroundPrefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                backgroundWidth = sr.bounds.size.x;
            }
            else
            {
                backgroundWidth = 20f; // ค่าเผื่อฉุกเฉิน
            }
        }

        // สร้างฉากเตรียมไว้และจัดเรียงให้ติดกันเป๊ะๆ
        for (int i = 0; i < poolSize; i++)
        {
            Vector3 spawnPos = new Vector3(transform.position.x + (i * backgroundWidth), transform.position.y, transform.position.z);
            GameObject bg = Instantiate(backgroundPrefab, spawnPos, Quaternion.identity, transform);
            backgrounds.Add(bg);
        }
    }

    void Update()
    {
        if (backgrounds.Count == 0) return;

        // เลื่อนฉากทั้งหมดไปทางซ้าย
        for (int i = 0; i < backgrounds.Count; i++)
        {
            backgrounds[i].transform.position += Vector3.left * scrollSpeed * Time.deltaTime;
        }

        // เช็คตัวหน้าสุด (ซ้ายสุด) ว่าหลุดขอบจอตามระยะ destroyX หรือยัง
        GameObject leftMostBg = backgrounds[0];
        if (leftMostBg.transform.position.x < destroyX)
        {
            // หาตัวหลังสุด (ขวาสุด)
            GameObject rightMostBg = backgrounds[backgrounds.Count - 1];

            // จับตัวซ้ายสุด ย้ายไปต่อท้ายตัวขวาสุด (ห่างกันเท่ากับ backgroundWidth เป๊ะๆ ทำให้รอยต่อเนียน)
            Vector3 newPos = leftMostBg.transform.position;
            newPos.x = rightMostBg.transform.position.x + backgroundWidth;
            leftMostBg.transform.position = newPos;

            // สลับตำแหน่งตัวแปรในลิสต์ ให้ตัวที่เพิ่งย้ายไปอยู่หลังสุด
            backgrounds.RemoveAt(0);
            backgrounds.Add(leftMostBg);
        }
    }
}

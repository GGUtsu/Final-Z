using UnityEngine;

public class LogoBreathe : MonoBehaviour
{
    [Header("ตั้งค่าการย่อขยาย")]
    [Tooltip("ขนาดเล็กสุดเมื่อเทียบกับขนาดเดิม (เช่น 0.95 = 95%)")]
    public float minScale = 0.95f;
    
    [Tooltip("ขนาดใหญ่สุดเมื่อเทียบกับขนาดเดิม (เช่น 1.05 = 105%)")]
    public float maxScale = 1.05f;
    
    [Tooltip("ความเร็วในการย่อขยาย")]
    public float speed = 2f;

    private Vector3 originalScale;

    void Start()
    {
        // จำขนาดเริ่มต้นของโลโก้ไว้ เพื่อไม่ให้สัดส่วนเพี้ยน
        originalScale = transform.localScale;
    }

    void Update()
    {
        // ใช้ฟังก์ชัน Sin เพื่อสร้างคลื่นจังหวะที่นุ่มนวล (จะคืนค่า -1 ถึง 1)
        // เราเอามา +1 และหาร 2 เพื่อปรับให้อยู่ในช่วง 0 ถึง 1 แทน
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;

        // คำนวณขนาดตัวคูณใหม่ โดยให้วิ่งสลับไปมาระหว่าง minScale และ maxScale
        float currentScaleMultiplier = Mathf.Lerp(minScale, maxScale, t);

        // อัปเดตขนาดของโลโก้
        transform.localScale = originalScale * currentScaleMultiplier;
    }
}

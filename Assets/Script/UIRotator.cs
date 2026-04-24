using UnityEngine;
using UnityEngine.EventSystems;

public class UIRotator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("ความเร็วในการหมุน (ค่าลบ = ตามเข็มนาฬิกา)")]
    public float rotationSpeed = -100f;

    [Tooltip("ให้หมุนเฉพาะตอนเอาเมาส์ไปชี้ (Hover) ใช่หรือไม่?")]
    public bool spinOnlyWhenHover = true;

    [Tooltip("เมื่อเอาเมาส์ออก ให้หมุนกลับไปที่มุมเดิมแบบนุ่มนวลหรือไม่?")]
    public bool returnToOriginal = true;

    [Tooltip("ความเร็วในการหมุนกลับที่เดิม (ยิ่งมากยิ่งกลับเร็ว)")]
    public float returnSpeed = 10f;

    private bool isSpinning = false;
    private Quaternion originalRotation; // เก็บค่ามุมเริ่มต้นเอาไว้

    void Start()
    {
        // จำมุมตอนเริ่มต้นของรูปภาพเอาไว้
        originalRotation = transform.localRotation;

        if (!spinOnlyWhenHover)
        {
            isSpinning = true;
        }
    }

    void Update()
    {
        if (isSpinning)
        {
            // สั่งให้หมุนติ้วๆ
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
        else if (returnToOriginal)
        {
            // ถ้าไม่ได้ชี้เมาส์แล้ว ให้ค่อยๆ หมุนกลับไปที่มุมเดิมอย่างนุ่มนวล (Lerp)
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, Time.deltaTime * returnSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spinOnlyWhenHover)
        {
            isSpinning = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spinOnlyWhenHover)
        {
            isSpinning = false; // หยุดหมุน
        }
    }
}

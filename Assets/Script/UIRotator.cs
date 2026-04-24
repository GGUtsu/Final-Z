using UnityEngine;
using UnityEngine.EventSystems;

public class UIRotator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Rotation Settings")]
    [Tooltip("ความเร็วในการหมุน (ค่าลบ = ตามเข็มนาฬิกา)")]
    public float rotationSpeed = -100f;

    [Tooltip("ให้หมุนเฉพาะตอนเอาเมาส์ไปชี้ (Hover) ใช่หรือไม่?")]
    public bool spinOnlyWhenHover = true;

    [Tooltip("เมื่อเอาเมาส์ออก ให้หมุนกลับไปที่มุมเดิมแบบนุ่มนวลหรือไม่?")]
    public bool returnToOriginal = true;

    [Tooltip("ความเร็วในการหมุนกลับที่เดิม (ยิ่งมากยิ่งกลับเร็ว)")]
    public float returnSpeed = 10f;

    [Header("Audio Settings")]
    [Tooltip("เสียงหรือเพลงที่จะเล่นตอนที่รูปหมุน")]
    public AudioClip spinMusic;

    [Tooltip("เริ่มเล่นเพลงใหม่ตั้งแต่ต้นทุกครั้งที่เอาเมาส์ชี้หรือไม่? (ถ้าปิด จะเล่นต่อจากจุดที่หยุด)")]
    public bool playFromStartEveryTime = false;

    private bool isSpinning = false;
    private Quaternion originalRotation; // เก็บค่ามุมเริ่มต้นเอาไว้
    private AudioSource audioSource;

    void Start()
    {
        // จำมุมตอนเริ่มต้นของรูปภาพเอาไว้
        originalRotation = transform.localRotation;

        // เตรียม AudioSource สำหรับเล่นเพลง
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        if (spinMusic != null)
        {
            audioSource.clip = spinMusic;
        }

        if (!spinOnlyWhenHover)
        {
            StartSpinning();
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
            StartSpinning();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spinOnlyWhenHover)
        {
            StopSpinning();
        }
    }

    private void StartSpinning()
    {
        isSpinning = true;

        // เล่นเพลงเมื่อเริ่มหมุน
        if (spinMusic != null)
        {
            if (playFromStartEveryTime)
            {
                audioSource.Stop();
                audioSource.Play();
            }
            else if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    private void StopSpinning()
    {
        isSpinning = false; // หยุดหมุน

        // หยุดเพลงเมื่อหยุดหมุน
        if (spinMusic != null && audioSource.isPlaying)
        {
            if (playFromStartEveryTime)
            {
                audioSource.Stop();
            }
            else
            {
                audioSource.Pause();
            }
        }
    }
}

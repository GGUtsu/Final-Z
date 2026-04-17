using UnityEngine;

public class NoteMovement : MonoBehaviour
{
    public float speed = 5f; // ปรับความเร็วของโน้ตได้ตามต้องการ

    void Update()
    {
        // สั่งให้วิ่งไปทางซ้าย (แกน X ติดลบ) ตลอดเวลา
        transform.position += Vector3.left * speed * Time.deltaTime;
    }
}
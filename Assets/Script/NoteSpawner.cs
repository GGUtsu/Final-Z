using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public float spawnRate = 1.5f; // ระยะเวลาในการปล่อยโน้ต (วินาที)
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // เมื่อเวลาผ่านไปถึงรอบที่กำหนด ให้สร้างโน้ตใหม่
        if (timer >= spawnRate)
        {
            SpawnNote();
            timer = 0f; // รีเซ็ตเวลาใหม่
        }
    }

    void SpawnNote()
    {
        // สร้าง Note ออกมา ณ ตำแหน่งของ Spawner
        Instantiate(notePrefab, transform.position, Quaternion.identity);
    }
}
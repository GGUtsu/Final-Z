using UnityEngine;

public class JudgmentEffect : MonoBehaviour
{
    public float lifetime = 0.5f;   // เวลาที่จะให้รูปโชว์อยู่ (วินาที)
    public float floatSpeed = 2f;   // ความเร็วในการลอยขึ้น

    // เพิ่มฟังก์ชันนี้เข้ามาตามคำสั่ง prompt
    public void ShowJudgment(string type, Transform spawnLocation)
    {
        GameObject prefabToSpawn = null;

        // กำหนด prefab ที่จะเสกตามประเภทที่ส่งมา
        if (type == "Perfect") prefabToSpawn = GameManager.instance.perfectPrefab;
        else if (type == "Great") prefabToSpawn = GameManager.instance.greatPrefab;
        else if (type == "Miss") prefabToSpawn = GameManager.instance.missPrefab;

        // เสกรูปออกมาที่ตำแหน่ง spawnLocation ที่เลนนั้นๆ ส่งมาให้
        if (prefabToSpawn != null && spawnLocation != null)
        {
            Instantiate(prefabToSpawn, spawnLocation.position, Quaternion.identity);
        }
    }

    void Start()
    {
        // สั่งทำลาย Object นี้ทิ้งทันทีเมื่อเวลาผ่านไปเท่ากับ lifetime
        Destroy(gameObject, lifetime); 
    }

    void Update()
    {
        // ทำให้รูปลอยขึ้นข้างบนเรื่อยๆ 
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
    }
}
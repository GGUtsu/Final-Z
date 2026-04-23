using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NoteData
{
    public float spawnTime; 
    public int lane; 
}

public class BeatmapSpawner : MonoBehaviour
{
    [Header("ระบบเพลง")]
    public AudioSource songBgm; 

    [Header("จุดปล่อยโน้ต")]
    public Transform topSpawnPoint; 
    public Transform bottomSpawnPoint; 

    [Header("Prefab ขนม 3 แบบ")]
    public GameObject[] notePrefabs; 

    // 👇 สิ่งที่เพิ่มเข้ามา: ระบบคำนวณเวลาเดินทาง 👇
    [Header("เวลาเดินทางของโน้ต (วินาที)")]
    [Tooltip("ถ้าโน้ตมาถึงเป้าช้ากว่าเสียงเพลง ให้เพิ่มเลขนี้ / ถ้ามาถึงเร็วกว่า ให้ลดเลขนี้")]
    public float travelTime = 2.0f; 

    [Header("คิวปล่อยโน้ต (Beatmap)")]
    public List<NoteData> noteList; 
    private int currentIndex = 0;

    void Update()
    {
        if (songBgm == null || !songBgm.isPlaying) return;
        if (currentIndex >= noteList.Count) return;

        // 👇 แก้ตรงนี้: เอาเวลาคิวมาลบกับเวลาเดินทาง มันจะได้เกิดล่วงหน้า! 👇
        while (currentIndex < noteList.Count && songBgm.time >= (noteList[currentIndex].spawnTime - travelTime))
        {
            SpawnNote(noteList[currentIndex]); 
            currentIndex++; 
        }
    }

    void SpawnNote(NoteData noteData)
    {
        Transform spawnPos = (noteData.lane == 0) ? topSpawnPoint : bottomSpawnPoint;

        if (notePrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, notePrefabs.Length); 
            Instantiate(notePrefabs[randomIndex], spawnPos.position, Quaternion.identity);
        }
    }
}
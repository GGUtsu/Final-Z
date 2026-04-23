using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("ระบบแก้ปัญหาโน้ตมาถึงช้า (ดีเลย์เพลง)")]
    [Tooltip("เวลาที่โน้ตใช้เดินทางจากจุดเกิดไปถึงเป้าหมาย (ค่าเดิมใน Beatmap คือ 2.0)")]
    public float travelTime = 2.0f;

    [Header("จุดปล่อยโน้ต (ลากตำแหน่งที่ต้องการมาใส่ เช่น เลนบน/เลนล่าง)")]
    public Transform[] spawnPoints;

    [Header("ตั้งค่ารูปแบบโน้ต (ใส่ได้ 3 แบบหรือมากกว่า)")]
    public GameObject[] notePrefabs;

    [Header("ตั้งค่าเสียงเพลง")]
    public AudioSource audioSource;
    public int sampleSize = 64;    

    [Header("--- ระบบไต่ระดับความยาก (Dynamic Difficulty) ---")]
    [Tooltip("ความเร็วโน้ตตอนเริ่มเพลง (ค่าเริ่มต้นของเกมคือ 5)")]
    public float startSpeed = 5f;
    [Tooltip("ความเร็วโน้ตตอนท้ายเพลง (ยิ่งวิ่งเร็ว โน้ตจะเกิดไกลขึ้นเพื่อรักษาจังหวะให้ตรงเป๊ะ)")]
    public float endSpeed = 12f;
    
    [Space(10)]
    [Tooltip("ความยากในการออกโน้ตตอนเริ่มเพลง (ค่ามาก = โน้ตออกยาก)")]
    [Range(0.001f, 0.5f)] public float startThreshold = 0.08f; 
    [Tooltip("ความยากตอนท้ายเพลง (ค่าน้อย = โน้ตจะออกถี่ยิบ รับทุกจังหวะดนตรี)")]
    [Range(0.001f, 0.5f)] public float endThreshold = 0.02f; 
    
    [Space(10)]
    [Tooltip("ดีเลย์ป้องกันโน้ตทับกันตอนเริ่มเพลง")]
    public float startCooldown = 0.25f; 
    [Tooltip("ดีเลย์ช่วงท้ายเพลง (ลดลงเพื่อให้มีท่อนรัวนิ้วได้)")]
    public float endCooldown = 0.1f; 
    
    [Space(10)]
    [Range(0f, 1f)] [Tooltip("โอกาสเกิดโน้ตคู่ตอนเริ่มเพลง")]
    public float startDoubleChance = 0.0f;
    [Range(0f, 1f)] [Tooltip("โอกาสเกิดโน้ตคู่ตอนจบเพลง (เพิ่มความท้าทายสุดๆ)")]
    public float endDoubleChance = 0.6f;

    private float lastSpawnTime = 0f;
    private int lastSpawnIndex = -1; // ตัวแปรจำว่ารอบที่แล้วปล่อยเลนไหนไป
    private int consecutiveSameLane = 0; // นับว่าออกเลนซ้ำกี่ครั้งแล้ว

    [Header("ข้อมูล Real-time (อ่านได้อย่างเดียว)")]
    public float currentBassEnergy = 0f; 
    [Tooltip("บอกว่าตอนนี้เล่นเพลงไปแล้วกี่ %")]
    public float songProgressPercent = 0f; 

    private float[] spectrumData;
    private AudioSource analyzerSource;

    void Start()
    {
        spectrumData = new float[sampleSize];

        if (audioSource != null)
        {
            // 1. สร้างตัววิเคราะห์เสียงแบบพิเศษ
            GameObject analyzerObj = new GameObject("AnalyzerAudioDummy");
            analyzerSource = analyzerObj.AddComponent<AudioSource>();
            analyzerSource.clip = audioSource.clip;
            
            // ต้องเปิดเสียงเต็ม 100% เพื่อให้อ่านคลื่นเสียงได้
            analyzerSource.volume = 1f; 
            analyzerSource.playOnAwake = false;
            
            // ใช้สคริปต์ AudioMuter ตัวใหม่ที่เราเพิ่งสร้าง ไว้ปิดเสียงก่อนออกลำโพง
            analyzerObj.AddComponent<AudioMuter>();
            
            analyzerSource.Play();

            // 2. ดีเลย์เพลงหลักที่ผู้เล่นจะได้ยิน
            // สมมติโน้ตเดินทาง 2 วินาที เราก็จะหน่วงเพลงช้าลง 2 วินาที 
            // ทำให้ดูเหมือน "โน้ตถูกปล่อยออกมาก่อนจังหวะเพลง 2 วินาที" พอดีเป๊ะ!
            audioSource.Stop(); 
            audioSource.PlayDelayed(travelTime);
        }
    }

    void Update()
    {
        // ใช้ตัววิเคราะห์เสียงแทนตัวหลัก
        if (analyzerSource == null || !analyzerSource.isPlaying) return;

        analyzerSource.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);

        float sum = 0;
        int bassRange = sampleSize / 4; 
        for (int i = 0; i < bassRange; i++)
        {
            sum += spectrumData[i];
        }
        
        currentBassEnergy = sum / bassRange;

        // คำนวณเปอร์เซ็นต์ความก้าวหน้าของเพลง (0.0 ถึง 1.0)
        float progress = 0f;
        if (analyzerSource.clip != null && analyzerSource.clip.length > 0)
        {
            progress = analyzerSource.time / analyzerSource.clip.length;
        }
        songProgressPercent = progress * 100f; // เอาไว้โชว์ให้ผู้ใช้ดูใน Inspector

        // --- ระบบคำนวณความยากอัตโนมัติ (Dynamic Difficulty) ---
        // ใช้ Mathf.Lerp เพื่อไล่ระดับค่าต่างๆ จากช่วงต้นเพลง ไปสู่ช่วงท้ายเพลงอย่างนุ่มนวล
        float currentThreshold = Mathf.Lerp(startThreshold, endThreshold, progress);
        float currentCooldown = Mathf.Lerp(startCooldown, endCooldown, progress);
        float currentSpeed = Mathf.Lerp(startSpeed, endSpeed, progress);
        float currentDoubleChance = Mathf.Lerp(startDoubleChance, endDoubleChance, progress);

        if (currentBassEnergy > currentThreshold && Time.time - lastSpawnTime >= currentCooldown)
        {
            SpawnNote(currentSpeed, currentDoubleChance);
            lastSpawnTime = Time.time; 
        }
    }

    void SpawnNote(float currentSpeed, float currentDoubleChance)
    {
        if (notePrefabs == null || notePrefabs.Length == 0) return;

        bool spawnDouble = Random.value <= currentDoubleChance;

        if (spawnDouble && spawnPoints != null && spawnPoints.Length >= 2)
        {
            int point1 = Random.Range(0, spawnPoints.Length);
            int point2 = Random.Range(0, spawnPoints.Length);
            
            while (point1 == point2)
            {
                point2 = Random.Range(0, spawnPoints.Length);
            }

            SpawnSingleNoteAt(spawnPoints[point1].position, currentSpeed);
            SpawnSingleNoteAt(spawnPoints[point2].position, currentSpeed);
        }
        else
        {
            Vector3 spawnPosition = transform.position; 
            
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
                
                // อนุญาตให้ออกเลนซ้ำได้สูงสุด 2 ครั้งติดกัน (เพื่อความสมจริง)
                if (Time.time - lastSpawnTime < 0.3f && spawnPoints.Length >= 2)
                {
                    if (randomSpawnIndex == lastSpawnIndex)
                    {
                        consecutiveSameLane++;
                        // ถ้าซ้ำเกิน 1 ครั้ง (คือออกเลนเดิมมา 2 ตัวแล้ว) ให้บังคับสลับเลน
                        if (consecutiveSameLane >= 2)
                        {
                            while (randomSpawnIndex == lastSpawnIndex)
                            {
                                randomSpawnIndex = Random.Range(0, spawnPoints.Length);
                            }
                            consecutiveSameLane = 0; // สลับเลนแล้ว รีเซ็ตตัวนับ
                        }
                    }
                    else
                    {
                        consecutiveSameLane = 0; // ถ้าสุ่มได้เลนอื่นแต่แรก ก็รีเซ็ตตัวนับ
                    }
                }
                else
                {
                    consecutiveSameLane = 0; // ถ้าระยะห่างปกติ ไม่รัว ก็รีเซ็ต
                }

                if (spawnPoints[randomSpawnIndex] != null)
                {
                    spawnPosition = spawnPoints[randomSpawnIndex].position;
                }
                
                lastSpawnIndex = randomSpawnIndex; // จดจำไว้ว่ารอบนี้ออกเลนไหน
            }
            SpawnSingleNoteAt(spawnPosition, currentSpeed);
        }
    }

    void SpawnSingleNoteAt(Vector3 basePosition, float currentSpeed)
    {
        int randomIndex = Random.Range(0, notePrefabs.Length);
        GameObject selectedPrefab = notePrefabs[randomIndex];

        if (selectedPrefab != null)
        {
            // --- สมการรักษาจังหวะ (Dynamic Distance Sync) ---
            // ถ้ายิ่งวิ่งเร็วขึ้น ต้องถอยจุดปล่อยให้ไกลขึ้น (ไปทางขวา) เพื่อให้ใช้เวลาเดินทาง 2 วิเท่าเดิม
            // สูตร: ระยะทางที่ต้องถอยเพิ่ม = (ความเร็วใหม่ - ความเร็วเดิม) x เวลาเดินทาง
            float extraDistanceX = (currentSpeed - startSpeed) * travelTime;
            Vector3 finalSpawnPos = basePosition + new Vector3(extraDistanceX, 0, 0);

            // สร้างตัวโน้ตที่ตำแหน่งใหม่ที่ถูกคำนวณแล้ว
            GameObject newNote = Instantiate(selectedPrefab, finalSpawnPos, Quaternion.identity);

            // ส่งค่าสปีดใหม่ไปให้สคริปต์ NoteMovement บังคับให้โน้ตตัวนี้วิ่งเร็วขึ้น
            NoteMovement movement = newNote.GetComponent<NoteMovement>();
            if (movement == null) movement = newNote.GetComponentInChildren<NoteMovement>();
            
            if (movement != null)
            {
                movement.speed = currentSpeed;
            }
        }
    }
}

// คลาสพิเศษสำหรับปิดเสียง AudioSource โดยไม่กระทบคลื่น Spectrum
public class AudioMuter : MonoBehaviour
{
    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 0f;
        }
    }
}
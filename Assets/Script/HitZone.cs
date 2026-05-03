using System.Collections.Generic;
using UnityEngine;

public class HitZone : MonoBehaviour
{
    [Header("ตั้งค่าปุ่มกด (ใส่ได้หลายปุ่ม เช่น F, D, S)")]
    public KeyCode[] hitKeys;
    public string laneName;

    [Header("ตั้งค่าภาพเป้ารับโน้ต (Visual)")]
    public SpriteRenderer buttonGraphic; // ลากภาพวงกลมเป้าหมายมาใส่ช่องนี้
    public Color defaultColor = Color.white; // สีปกติตอนไม่ได้กด
    public Color pressedColor = Color.gray;  // สีที่จะเปลี่ยนตอนนิ้วกดลงไป

    [Header("ตั้งค่าเอฟเฟกต์ (Particles)")]
    public GameObject hitParticlePrefab; // ลาก Prefab พาร์ติเคิลตอนตีโดนมาใส่ช่องนี้

    [Header("ตั้งค่าเสียง (Sound Effects)")]
    public AudioClip keyPressSound; // เสียงตอนกดปุ่มเปล่าๆ
    public AudioClip hitSound;      // เสียงตอนตีโดนโน้ต (Perfect/Great)
    public AudioClip missSound;     // เสียงตอนพลาด (กดวืด หรือปล่อยโน้ตหลุดกรอบ)

    private AudioSource sfxSource; // สำหรับเสียงโดนโน้ตและพลาด (ยอมให้ทับกันได้)
    private AudioSource tapSource; // สำหรับเสียงเคาะเปล่าๆ (ตัดเสียงเก่าทิ้งกันหนวกหู)
    private List<GameObject> notesInZone = new List<GameObject>();

    // ตัวแปร static ป้องกันเสียงซ้อนกันเวลากดพร้อมกัน
    private static int lastHitFrame = -1;
    private static int lastMissFrame = -1;
    private static int lastTapFrame = -1;

    void Start()
    {
        // สร้าง AudioSource แยก 2 ตัว เพื่อไม่ให้เสียงตีโน้ตโดนตัดตอนกดรัวๆ
        sfxSource = gameObject.AddComponent<AudioSource>();
        tapSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        bool isHitKeyDown = false;
        bool isHitKeyUp = false;
        bool isAnyKeyHeld = false;

        // เช็คทุกปุ่มในอาเรย์
        if (hitKeys != null)
        {
            foreach (KeyCode key in hitKeys)
            {
                if (Input.GetKeyDown(key)) isHitKeyDown = true;
                if (Input.GetKeyUp(key)) isHitKeyUp = true;
                if (Input.GetKey(key)) isAnyKeyHeld = true;
            }
        }

        // จังหวะกดปุ่ม (กดปุ่มไหนก็ได้ใน List)
        if (isHitKeyDown)
        {
            if (buttonGraphic != null) buttonGraphic.color = pressedColor; 

            // ถ้ามีโน้ตในกล่อง
            if (notesInZone.Count > 0)
            {
                GameObject noteToHit = notesInZone[0];
                float distance = Mathf.Abs(noteToHit.transform.position.x - transform.position.x);

                // --- เช็คความแม่นยำตามระยะทาง ---
                if (distance <= 0.5f) 
                {
                    // Perfect (แม่นเป๊ะ)
                    if(GameManager.instance != null) {
                        GameManager.instance.AddScore(1000); 
                        GameManager.instance.Heal(4); 
                        GameManager.instance.ShowJudgment("Perfect", transform); 
                    }
                    PlaySFX(hitSound);
                    SpawnHitParticle(noteToHit.transform.position);
                }
                else if (distance <= 1.2f)
                {
                    // Great (เกือบเป๊ะ)
                    if(GameManager.instance != null) {
                        GameManager.instance.AddScore(500); 
                        GameManager.instance.Heal(2); 
                        GameManager.instance.ShowJudgment("Great", transform); 
                    }
                    PlaySFX(hitSound);
                    SpawnHitParticle(noteToHit.transform.position);
                }
                else 
                {
                    // Miss (กดตอนโน้ตอยู่ในกรอบจริง แต่กะจังหวะพลาดไปเยอะ = กดเร็วไป/ช้าไป)
                    if(GameManager.instance != null) {
                        GameManager.instance.ResetCombo(); 
                        GameManager.instance.TakeDamage(2); 
                        GameManager.instance.ShowJudgment("Miss", transform); 
                    }
                    PlaySFX(missSound);
                }

                notesInZone.Remove(noteToHit);

                // สับขาหลอก Unity
                noteToHit.tag = "Untagged"; 

                Destroy(noteToHit);
            }
            else
            {
                // กดแต่ไม่มีโน้ตในกรอบ (กดเปล่าๆ)
                PlayTapSFX();
            }
        }

        // จังหวะปล่อยปุ่ม (เช็คให้แน่ใจว่าไม่ได้กดปุ่มอื่นค้างอยู่ ถึงจะเปลี่ยนสีกลับ)
        if (isHitKeyUp && !isAnyKeyHeld)
        {
            if (buttonGraphic != null) buttonGraphic.color = defaultColor;
        }
    }

    // เอาโน้ตเข้า List เมื่อวิ่งเข้ามาในกล่อง
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Note")) notesInZone.Add(other.gameObject);
    }

    // ปล่อยโน้ตหลุดกรอบไป (ลบออก + ตัดคอมโบ + เด้ง Miss + ลดเลือด 10)
    private void OnTriggerExit2D(Collider2D collision)
    {
        // ถ้าสิ่งที่หลุดกรอบออกไปคือ "โน้ต"
        if (collision.CompareTag("Note"))
        {
            notesInZone.Remove(collision.gameObject);

            if (GameManager.instance != null)
            {
                GameManager.instance.ResetCombo(); // ตัดคอมโบ
                GameManager.instance.ShowJudgment("Miss", transform); // โชว์รูป Miss

                // --- เพิ่มคำสั่งลดเลือด 2 หน่วย ตรงนี้ครับ! ---
                GameManager.instance.TakeDamage(2); 
            }

            PlaySFX(missSound); // เล่นเสียงพลาดเมื่อโน้ตหลุดขอบจอ

            Destroy(collision.gameObject); // ทำลายโน้ตทิ้ง
        }
    }

    // ฟังก์ชันสำหรับเล่นเสียงทับกันได้ (ใช้กับ Hit และ Miss)
    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            if (clip == hitSound)
            {
                if (lastHitFrame == Time.frameCount) return;
                lastHitFrame = Time.frameCount;
            }
            else if (clip == missSound)
            {
                if (lastMissFrame == Time.frameCount) return;
                lastMissFrame = Time.frameCount;
            }

            sfxSource.PlayOneShot(clip);
        }
    }

    // ฟังก์ชันสำหรับเสียงกดเปล่าๆ (ตัดเสียงเก่าทิ้ง ป้องกันเสียงซ้อนกันจนดังเกินไป)
    private void PlayTapSFX()
    {
        if (keyPressSound != null && tapSource != null)
        {
            if (lastTapFrame == Time.frameCount) return;
            lastTapFrame = Time.frameCount;

            tapSource.clip = keyPressSound;
            tapSource.Play(); 
        }
    }

    // ฟังก์ชันสร้าง Particle ตอนตีโดนโน้ต
    private void SpawnHitParticle(Vector3 spawnPos)
    {
        if (hitParticlePrefab != null)
        {
            // สร้าง Particle ณ ตำแหน่งของโน้ตที่ถูกตีแตก
            GameObject particleObj = Instantiate(hitParticlePrefab, spawnPos, Quaternion.identity);
            
            // บังคับให้เล่น Particle ทันที (แก้ปัญหาโคลนมาแล้วไม่ยอมเล่น ถ้าลืมติ๊ก Play On Awake)
            ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play(true); // สั่งเล่นพร้อมกับตัวลูกทั้งหมด
            }

            // ตั้งเวลาให้มันทำลายตัวเองทิ้งภายใน 2 วินาที (กันขยะล้นฉาก)
            Destroy(particleObj, 2f); 
        }
    }
}
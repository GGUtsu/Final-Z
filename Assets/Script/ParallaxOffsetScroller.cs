using UnityEngine;

public enum LayerEffect
{
    None,            // เลื่อนฉากปกติอย่างเดียว
    ShootingStar,    // ดาวตก (พุ่งแล้ววนกลับมาใหม่)
    Pulse,           // ย่อขยาย (กระพริบ)
    Float,           // ลอยขึ้นลง
    Rotate           // หมุนติ้วๆ
}

public class ParallaxOffsetScroller : MonoBehaviour
{
    // โครงสร้างสำหรับเก็บข้อมูลแต่ละเลเยอร์ของฉากหลัง
    [System.Serializable]
    public class ParallaxLayer
    {
        [Tooltip("ลาก GameObject ของฉากหลังที่มีคอมโพเนนต์ SpriteRenderer มาใส่")]
        public Renderer layerRenderer; 
        
        [Header("การปรับสี (Color & Contrast)")]
        [Tooltip("ปรับสีเพื่อไม่ให้กลืนกับตัวละคร (แนะนำ: ให้ปรับสีเทาเข้มๆ เพื่อให้ฉากมืดลง หรือลด Alpha)")]
        public Color layerTint = Color.white;

        [Header("การแก้รอยต่อภาพ (Seam Fix)")]
        [Tooltip("ถ้าเห็นรอยต่อระหว่างภาพตอนเลื่อน ให้เพิ่มค่านี้ทีละนิด (เช่น 0.01 - 0.05) ภาพจะขยับมาเกยกันเพื่อปิดเส้นรอยต่อ")]
        public float seamOverlap = 0.02f;

        [Tooltip("ความเร็วเลื่อนฉาก (ค่าน้อย = อยู่ไกลและเลื่อนช้า, ค่ามาก = อยู่ใกล้และเลื่อนเร็ว)")]
        public float scrollSpeed;      

        [Header("ลูกเล่นเสริม (Special Effect)")]
        [Tooltip("เลือกเอฟเฟกต์พิเศษสำหรับเลเยอร์นี้")]
        public LayerEffect effect = LayerEffect.None;
        
        [Tooltip("ความเร็วของเอฟเฟกต์ (ความเร็วดาวตก, ความเร็วกระพริบ, ความเร็วหมุน)")]
        public float effectSpeed = 5f;
        [Tooltip("ขนาดเอฟเฟกต์ (ขนาดกระพริบ, ระยะลอยขึ้นลง)")]
        public float effectAmount = 0.5f;

        // ตัวแปรซ่อนสำหรับทำงานเบื้องหลัง
        [HideInInspector] public Vector3 startPosition;
        [HideInInspector] public float width;
        [HideInInspector] public Vector3 originalScale;
        [HideInInspector] public float randomOffset;
        [HideInInspector] public float shootingTimer;
        [HideInInspector] public bool isShooting;
        
        // เก็บอ้างอิง Sprite สำหรับอัปเดตสี
        [HideInInspector] public SpriteRenderer mainSpriteRenderer;
        [HideInInspector] public SpriteRenderer cloneSpriteRenderer;
    }

    [Header("การตั้งค่าเลเยอร์ (เรียงจากไกลสุดไปใกล้สุด)")]
    public ParallaxLayer[] layers;

    void Start()
    {
        // เริ่มต้นการตั้งค่าให้แต่ละเลเยอร์
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerRenderer != null)
            {
                Transform layerTransform = layers[i].layerRenderer.transform;
                layers[i].startPosition = layerTransform.position;
                layers[i].originalScale = layerTransform.localScale;
                layers[i].randomOffset = Random.Range(0f, 100f);
                
                layers[i].mainSpriteRenderer = layers[i].layerRenderer as SpriteRenderer;

                // ถ้าระบุว่าเป็นดาวตก ให้เริ่มการรีเซ็ตดาวตก
                if (layers[i].effect == LayerEffect.ShootingStar)
                {
                    ResetShootingStar(layers[i]);
                }

                // การเตรียมพร้อมสำหรับเลื่อนฉากปกติ (ยกเว้นดาวตกที่ไม่ต้องโคลนภาพ)
                if (layers[i].effect != LayerEffect.ShootingStar)
                {
                    // ลบค่า seamOverlap ออกจากความกว้าง เพื่อให้ภาพเวลาต่อกันมันเกยกันนิดนึง จะได้ไม่เกิดรอยต่อ
                    layers[i].width = layers[i].layerRenderer.bounds.size.x - layers[i].seamOverlap;

                    if (layers[i].mainSpriteRenderer != null && layers[i].width > 0)
                    {
                        GameObject clone = new GameObject(layerTransform.name + "_Clone");
                        clone.transform.SetParent(layerTransform); 
                        clone.transform.position = new Vector3(layers[i].startPosition.x + layers[i].width, layers[i].startPosition.y, layers[i].startPosition.z);
                        clone.transform.localScale = Vector3.one;

                        layers[i].cloneSpriteRenderer = clone.AddComponent<SpriteRenderer>();
                        layers[i].cloneSpriteRenderer.sprite = layers[i].mainSpriteRenderer.sprite;
                        // สีจะถูกอัปเดตแบบเรียลไทม์ในฟังก์ชัน Update() ด้านล่าง
                        layers[i].cloneSpriteRenderer.sortingLayerID = layers[i].mainSpriteRenderer.sortingLayerID;
                        layers[i].cloneSpriteRenderer.sortingOrder = layers[i].mainSpriteRenderer.sortingOrder;
                        layers[i].cloneSpriteRenderer.flipX = layers[i].mainSpriteRenderer.flipX;
                        layers[i].cloneSpriteRenderer.flipY = layers[i].mainSpriteRenderer.flipY;
                    }
                }
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerRenderer != null)
            {
                ParallaxLayer layer = layers[i];
                Transform t = layer.layerRenderer.transform;

                // 0. อัปเดตสีแบบเรียลไทม์ (ช่วยให้จูนสีตอนกด Play ได้ทันที)
                if (layer.mainSpriteRenderer != null)
                {
                    layer.mainSpriteRenderer.color = layer.layerTint;
                }
                if (layer.cloneSpriteRenderer != null)
                {
                    layer.cloneSpriteRenderer.color = layer.layerTint;
                }

                // 1. จัดการลูกเล่นเสริม (Effect)
                switch (layer.effect)
                {
                    case LayerEffect.Pulse:
                        float scaleOffset = Mathf.Sin((Time.time + layer.randomOffset) * layer.effectSpeed) * layer.effectAmount;
                        t.localScale = layer.originalScale + new Vector3(scaleOffset, scaleOffset, scaleOffset);
                        break;
                    case LayerEffect.Rotate:
                        t.Rotate(0, 0, layer.effectSpeed * Time.deltaTime);
                        break;
                    case LayerEffect.ShootingStar:
                        UpdateShootingStar(layer, t);
                        continue; // ข้ามการเลื่อนฉากปกติไปเลย
                }

                // 2. เลื่อนฉากปกติ (Parallax Scroll) และเอฟเฟกต์ Float
                if (layer.width > 0 && layer.effect != LayerEffect.ShootingStar)
                {
                    float newPos = Mathf.Repeat(Time.time * layer.scrollSpeed, layer.width);
                    
                    float currentY = layer.startPosition.y;
                    if (layer.effect == LayerEffect.Float)
                    {
                        currentY += Mathf.Sin((Time.time + layer.randomOffset) * layer.effectSpeed) * layer.effectAmount;
                    }

                    t.position = new Vector3(layer.startPosition.x - newPos, currentY, layer.startPosition.z);
                }
            }
        }
    }

    void UpdateShootingStar(ParallaxLayer layer, Transform t)
    {
        if (layer.isShooting)
        {
            // ทิศทางพุ่งเฉียงซ้ายล่าง
            Vector3 direction = new Vector3(-1, -0.5f, 0).normalized;
            t.Translate(direction * layer.effectSpeed * Time.deltaTime, Space.World);
            
            // เช็คระยะทาง ถ้ายิงไปไกลแล้วให้รีเซ็ต
            if (Vector3.Distance(layer.startPosition, t.position) > 40f)
            {
                ResetShootingStar(layer);
            }
        }
        else
        {
            // รอเวลาเพื่อเกิดใหม่
            layer.shootingTimer -= Time.deltaTime;
            if (layer.shootingTimer <= 0)
            {
                layer.isShooting = true;
                t.localScale = layer.originalScale;
            }
        }
    }

    void ResetShootingStar(ParallaxLayer layer)
    {
        layer.isShooting = false;
        if (layer.layerRenderer != null)
        {
            layer.layerRenderer.transform.position = layer.startPosition;
            layer.layerRenderer.transform.localScale = Vector3.zero; // ซ่อนตัวก่อน
        }
        layer.shootingTimer = Random.Range(2f, 6f); // สุ่มเวลารอ 2 ถึง 6 วินาที
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // เพิ่มอันนี้สำหรับระบบหน่วงเวลา

public class SceneController : MonoBehaviour
{
    [Header("เสียงตอนเปิดฉาก (เช่น BGM ตอนเข้าเกม)")]
    public AudioClip sceneStartSound; 
    public bool loopSceneSound = true; // ให้เสียงเล่นวนลูปไหม?

    [Header("ตั้งค่าเสียงปุ่ม")]
    public AudioClip buttonClickSound; // ลากเสียงปุ่มมาใส่ช่องนี้
    private AudioSource audioSource;
    private AudioSource bgmSource;

    void Start()
    {
        // 1. ระบบเล่นเสียงตอนเปิดฉากอัตโนมัติ
        if (sceneStartSound != null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = sceneStartSound;
            bgmSource.loop = loopSceneSound;
            bgmSource.playOnAwake = true;
            bgmSource.Play();
        }

        // 2. สร้างระบบเสียงปุ่มอัตโนมัติ
        if (buttonClickSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    // ฟังก์ชันสำหรับปุ่มข้ามไปหน้า Setting A (แบบมีเสียง)
    public void LoadSettingA()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
            StartCoroutine(WaitAndLoad("Setting A", 0.3f)); // รอ 0.3 วิให้เสียงเล่นออกมาก่อนค่อยเปลี่ยนหน้า
        }
        else
        {
            SceneManager.LoadScene("Setting A");
        }
    }

    // ฟังก์ชันเปลี่ยนหน้าแบบพิมพ์ชื่อเอง (มีเสียง)
    public void LoadSceneWithSound(string sceneName)
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
            StartCoroutine(WaitAndLoad(sceneName, 0.3f));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator WaitAndLoad(string sceneName, float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // ใช้ Realtime เผื่อเวลาเกมถูก Pause อยู่
        SceneManager.LoadScene(sceneName);
    }

    // ฟังก์ชันเก่า (เปลี่ยนหน้าทันที ไม่มีเสียง)
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ฟังก์ชันสำหรับสั่งหยุด BGM ฉากชั่วคราว
    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    // ฟังก์ชันสำหรับสั่งเล่น BGM ฉากต่อ
    public void ResumeBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("ตั้งค่าหน้าต่าง UI Pause")]
    public GameObject pausePanel; // ลาก UI Panel ของหน้า Pause มาใส่ที่นี่

    private bool isPaused = false;

    void Start()
    {
        // เริ่มเกมมา ให้ซ่อนหน้า Pause ไว้ก่อน
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    void Update()
    {
        // กดปุ่ม ESC บนคีย์บอร์ด เพื่อหยุด/เล่นต่อ
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // หยุดเวลาในเกม (ทำให้ตัวโน้ตหยุดวิ่ง)

        if (pausePanel != null)
        {
            pausePanel.SetActive(true); // โชว์หน้าต่าง UI
        }

        // *** ใช้ AudioListener เพื่อหยุดเสียง "ทั้งหมด" ในเกมทันที (กันโน้ตเพี้ยน) ***
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // ให้เวลาเดินเป็นปกติ

        if (pausePanel != null)
        {
            pausePanel.SetActive(false); // ซ่อนหน้าต่าง UI
        }

        // เล่นเสียงต่อ
        AudioListener.pause = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // ต้องคืนค่าเวลาก่อนรีสตาร์ท ไม่งั้นฉากใหม่จะค้าง
        AudioListener.pause = false; // อย่าลืมปลด Pause เสียงก่อนเริ่มใหม่
        if (GameManager.instance != null) GameManager.instance.isChangingScene = true; // ป้องกันบั๊กเด้งหน้า Win
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; 
        AudioListener.pause = false;
        if (GameManager.instance != null) GameManager.instance.isChangingScene = true; // ป้องกันบั๊กเด้งหน้า Win
        SceneManager.LoadScene("Setting A"); // เปลี่ยนให้ตรงกับชื่อฉากเมนูของคุณ
    }
}

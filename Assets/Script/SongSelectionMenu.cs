using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class SongData
{
    public string songTitle;
    public Sprite coverArt;
    
    [Tooltip("Name of the scene to load when playing this song (e.g., 'Gameplay1')")]
    public string sceneToLoad; 
}

public class SongSelectionMenu : MonoBehaviour
{
    [Header("Songs Database")]
    public SongData[] songs;

    [Header("UI References (Optional Left Panel)")]
    public Image coverImageDisplay; 
    public TMP_Text titleTextDisplay; 

    [Header("Back Button Settings")]
    [Tooltip("Name of the Main Menu scene to go back to")]
    public string mainMenuSceneName = "MainMenu";

    // ฟังก์ชันนี้สำหรับปุ่มเลือกเพลง (กดแล้วโหลดเข้าเกมเลย)
    // Parameter 'index': 0 สำหรับเพลงที่ 1, 1 สำหรับเพลงที่ 2, 2 สำหรับเพลงที่ 3
    public void SelectAndPlaySong(int index)
    {
        if (index < 0 || index >= songs.Length) return;

        SongData selectedSong = songs[index];

        // เผื่อว่าคุณยังมีรูปปกโชว์อยู่ (ถ้าลบไปแล้วก็ไม่เป็นไร โค้ดจะไม่พัง)
        if (coverImageDisplay != null && selectedSong.coverArt != null) 
        {
            coverImageDisplay.sprite = selectedSong.coverArt;
        }
            
        if (titleTextDisplay != null) 
        {
            titleTextDisplay.text = selectedSong.songTitle;
        }

        // เซฟชื่อเพลงไว้ เผื่อในฉากเกมเพลย์ต้องการใช้
        PlayerPrefs.SetString("SelectedSong", selectedSong.songTitle);

        // โหลดเข้าฉากเกมทันที!
        if (!string.IsNullOrEmpty(selectedSong.sceneToLoad))
        {
            SceneManager.LoadScene(selectedSong.sceneToLoad);
        }
        else
        {
            Debug.LogError("SongSelectionMenu: ยังไม่ได้ใส่ชื่อ Scene To Load สำหรับเพลง " + selectedSong.songTitle);
        }
    }

    // ฟังก์ชันนี้สำหรับปุ่ม BACK
    public void GoBack()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("SongSelectionMenu: ยังไม่ได้ตั้งค่าชื่อ Scene ของ Main Menu");
        }
    }

    // ฟังก์ชันสำหรับปุ่ม Reset คะแนนทั้งหมด
    public void ResetAllHighScores()
    {
        if (songs != null)
        {
            // วนลูปตั้งค่าคะแนนของทุกเพลงในลิสต์ให้เป็น 0
            foreach (SongData song in songs)
            {
                if (!string.IsNullOrEmpty(song.sceneToLoad))
                {
                    string key = "HighScore_" + song.sceneToLoad;
                    PlayerPrefs.DeleteKey(key); // ลบคะแนนทิ้ง
                }
            }
            PlayerPrefs.Save(); // บันทึกการลบ

            // รีโหลดหน้านี้ใหม่ เพื่อให้ตัวเลขคะแนนที่โชว์อยู่บนจออัปเดตกลับเป็น 0
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

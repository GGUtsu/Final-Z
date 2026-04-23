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

    [Header("UI References (Left Panel)")]
    public Image coverImageDisplay; // The big image on the left
    public TMP_Text titleTextDisplay; // The text showing the current song name

    private int currentSelectedIndex = 0;

    void Start()
    {
        // Select the first song by default on start
        if (songs != null && songs.Length > 0)
        {
            SelectSong(0);
        }
    }

    // Link this to the OnClick event of your Song Buttons
    // Parameter 'index': 0 for the first button, 1 for the second, etc.
    public void SelectSong(int index)
    {
        if (index < 0 || index >= songs.Length) return;

        currentSelectedIndex = index;
        SongData selectedSong = songs[index];

        // Update the UI on the left panel
        if (coverImageDisplay != null && selectedSong.coverArt != null) 
        {
            coverImageDisplay.sprite = selectedSong.coverArt;
        }
            
        if (titleTextDisplay != null) 
        {
            titleTextDisplay.text = selectedSong.songTitle;
        }
    }

    // Link this to a "Play" or "Start Game" button
    public void PlaySelectedSong()
    {
        if (songs == null || songs.Length == 0) return;

        SongData selectedSong = songs[currentSelectedIndex];

        // Save the selected song title to PlayerPrefs so the Gameplay scene knows what was chosen
        PlayerPrefs.SetString("SelectedSong", selectedSong.songTitle);

        // Load the scene
        if (!string.IsNullOrEmpty(selectedSong.sceneToLoad))
        {
            SceneManager.LoadScene(selectedSong.sceneToLoad);
        }
        else
        {
            Debug.LogError("SongSelectionMenu: 'Scene To Load' is empty for " + selectedSong.songTitle);
        }
    }
}

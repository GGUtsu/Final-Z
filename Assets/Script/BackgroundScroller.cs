using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Background Settings")]
    // The Prefab to spawn (Make sure to make your background a prefab)
    public GameObject backgroundPrefab; 
    
    public float scrollSpeed = 5f;
    public float backgroundWidth = 20f;
    
    // The X position where the background will be destroyed (left side of camera)
    public float destroyX = -25f;
    
    // The X position threshold to spawn the next background (right side of camera)
    public float spawnXThreshold = 20f;

    private List<GameObject> activeBackgrounds = new List<GameObject>();

    void Start()
    {
        if (backgroundPrefab == null)
        {
            Debug.LogError("BackgroundScroller: Please assign a background prefab in the inspector!");
            return;
        }

        // Spawn initial backgrounds (one at center, one to the right)
        SpawnBackground(transform.position.x);
        SpawnBackground(transform.position.x + backgroundWidth);
    }

    void Update()
    {
        if (activeBackgrounds.Count == 0) return;

        // Move all active backgrounds to the left
        for (int i = 0; i < activeBackgrounds.Count; i++)
        {
            if (activeBackgrounds[i] != null)
            {
                activeBackgrounds[i].transform.position += Vector3.left * scrollSpeed * Time.deltaTime;
            }
        }

        // Check if we need to spawn a new background to the right
        GameObject rightMostBg = activeBackgrounds[activeBackgrounds.Count - 1];
        if (rightMostBg != null && rightMostBg.transform.position.x < spawnXThreshold)
        {
            SpawnBackground(rightMostBg.transform.position.x + backgroundWidth);
        }

        // Check if we need to destroy the leftmost background that went out of screen
        GameObject leftMostBg = activeBackgrounds[0];
        if (leftMostBg != null && leftMostBg.transform.position.x < destroyX)
        {
            activeBackgrounds.RemoveAt(0);
            Destroy(leftMostBg);
        }
    }

    void SpawnBackground(float xPos)
    {
        Vector3 spawnPos = new Vector3(xPos, transform.position.y, transform.position.z);
        // Instantiate and set this object as the parent
        GameObject newBg = Instantiate(backgroundPrefab, spawnPos, Quaternion.identity, transform);
        activeBackgrounds.Add(newBg);
    }
}

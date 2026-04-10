using UnityEngine;
using System.Collections.Generic;

public class BackgroundManager : MonoBehaviour
{
    public GameObject ballObject;
    public GameObject backgroundPrefab;
    public int maxBackgroundCount = 3;
    public float backgroundYOffset = 31200f; // Y축 오프셋을 조절할 수 있는 새로운 변수

    private float backgroundWidth;
    private List<GameObject> backgroundObjects = new List<GameObject>();
    private float lastBackgroundX;

    void Start()
    {
        if (ballObject == null || backgroundPrefab == null)
        {
            Debug.LogError("Ball or Background Prefab not assigned in the inspector!");
            return;
        }
        CalculateBackgroundWidth();
    }

    void FixedUpdate()
    {
        // Check if we need to spawn a new background
        if (ballObject.transform.position.x > lastBackgroundX - backgroundWidth - 20f && ballObject.transform.position.x > 2)
        {
            SpawnBackground();
            Debug.Log(backgroundObjects.Count);
        }
        // Remove off-screen background objects
        if (backgroundObjects.Count > 2)
        {
            GameObject oldestBackground = backgroundObjects[0];
            if (oldestBackground.transform.position.x + backgroundWidth < ballObject.transform.position.x)
            {
                backgroundObjects.RemoveAt(0);
                Destroy(oldestBackground);
            }
        }
    }

    void SpawnBackground()
    {
        // Y 위치에 backgroundYOffset을 적용합니다.
        Vector3 spawnPosition = new Vector3(lastBackgroundX+2700, backgroundYOffset, 0.2f);
        GameObject newBackground = Instantiate(backgroundPrefab, spawnPosition, Quaternion.identity);
        backgroundObjects.Add(newBackground);
        lastBackgroundX = newBackground.transform.position.x + 30f * backgroundWidth;
    }

    void CalculateBackgroundWidth()
    {
        SpriteRenderer backgroundSpriteRenderer = backgroundPrefab.GetComponent<SpriteRenderer>();
        if (backgroundSpriteRenderer != null && backgroundSpriteRenderer.sprite != null)
        {
            backgroundWidth = backgroundSpriteRenderer.sprite.bounds.size.x;
        }
        else
        {
            Debug.LogError("Background Prefab does not have a valid Sprite Renderer or Sprite!");
        }
    }
}
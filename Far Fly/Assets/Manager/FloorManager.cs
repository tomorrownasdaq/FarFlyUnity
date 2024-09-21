using UnityEngine;
using System.Collections.Generic;

public class FloorManager : MonoBehaviour
{
    public GameObject ballObject;
    public GameObject floorPrefab;
    public int maxFloorCount = 4;
    private float floorWidth;
    private List<GameObject> floorObjects = new List<GameObject>();
    private float lastFloorX;

    void Start()
    {
        if (ballObject == null || floorPrefab == null)
        {
            Debug.LogError("Ball or Floor Prefab not assigned in the inspector!");
            return;
        }
        CalculateFloorWidth();
    }

    void FixedUpdate()
    {
        // Check if we need to spawn a new floor
        if (ballObject.transform.position.x > lastFloorX - floorWidth - 80f && ballObject.transform.position.x > 2)
        {
            SpawnFloor();
            Debug.Log(floorObjects.Count);
        }

        // Remove off-screen floor objects
        if (floorObjects.Count > 4)
        {
            GameObject oldestFloor = floorObjects[0];
            if (oldestFloor.transform.position.x + floorWidth < ballObject.transform.position.x)
            {
                floorObjects.RemoveAt(0);
                Destroy(oldestFloor);
            }
        }
    }

    void SpawnFloor()
    {
        Vector3 spawnPosition = new Vector3(lastFloorX, floorPrefab.transform.position.y, 0.1f);
        GameObject newFloor = Instantiate(floorPrefab, spawnPosition, Quaternion.identity);
        floorObjects.Add(newFloor);
        lastFloorX = newFloor.transform.position.x + floorWidth + 1000f;
    }

    void CalculateFloorWidth()
    {
        SpriteRenderer floorSpriteRenderer = floorPrefab.GetComponent<SpriteRenderer>();
        if (floorSpriteRenderer != null && floorSpriteRenderer.sprite != null)
        {
            floorWidth = floorSpriteRenderer.sprite.bounds.size.x;
        }
        else
        {
            Debug.LogError("Floor Prefab does not have a valid Sprite Renderer or Sprite!");
        }
    }
}
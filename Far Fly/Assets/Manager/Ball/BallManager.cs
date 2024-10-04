using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.Networking;

public class BallManager : MonoBehaviour
{
    public float accelerationRate = 8000f;
    public float maxSpeed = 20f;
    public float deceleration = 500f;
    public float maxXPosition = 100f;
    public float maxMapPosition = 100f;

    private Rigidbody2D rb;
    private float currentSpeed = 0f;
    private bool isAccelerating = false;
    private SpriteRenderer spriteRenderer;
    private StageInventoryManager inventoryManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing from the ball!");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component is missing from the ball!");
        }

        inventoryManager = FindObjectOfType<StageInventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogError("PlayFabInventoryManager not found in the scene!");
        }

        LoadValuesFromPlayFab();
        LoadSelectedItemImage();
    }

    void Update()
    {
        // Check for keyboard input
        if (Input.GetKey(KeyCode.Space))
        {
            isAccelerating = true;
        }
        // Check for touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                isAccelerating = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (isAccelerating && transform.position.x < maxXPosition)
        {
            // Accelerate
            currentSpeed = rb.velocity.x + accelerationRate * Time.fixedDeltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed); // Cap the speed
            rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
        }
        else
        {
            // Decelerate
            currentSpeed = Mathf.MoveTowards(rb.velocity.x, 0, deceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
        }
        // Reset acceleration flag
        isAccelerating = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Reverse the x-velocity
            rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y);
        }
    }

    void LoadValuesFromPlayFab()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), OnTitleDataReceived, OnError);
    }

    void OnTitleDataReceived(GetTitleDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("ACC"))
        {
            if (float.TryParse(result.Data["ACC"], out float loadedAccelerationRate))
            {
                accelerationRate = loadedAccelerationRate;
                Debug.Log($"Loaded ACC value from PlayFab Title Data: {accelerationRate}");
            }
            else
            {
                Debug.LogWarning("Failed to parse ACC value from PlayFab Title Data.");
            }
        }
        else
        {
            Debug.Log("ACC key not found in PlayFab Title Data. Checking Player Data...");
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnPlayerDataReceived, OnError);
        }

        // Continue to load other values from User Data
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnUserDataReceived, OnError);
    }

    void OnPlayerDataReceived(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("ACC"))
        {
            if (float.TryParse(result.Data["ACC"].Value, out float loadedAccelerationRate))
            {
                accelerationRate = loadedAccelerationRate;
                Debug.Log($"Loaded ACC value from PlayFab Player Data: {accelerationRate}");
            }
            else
            {
                Debug.LogWarning("Failed to parse ACC value from PlayFab Player Data. Using default value.");
            }
        }
        else
        {
            Debug.Log("ACC key not found in PlayFab Player Data. Using default acceleration rate.");
        }
    }

    void OnUserDataReceived(GetUserDataResult result)
    {
        if (result.Data != null)
        {
            if (result.Data.ContainsKey("Deceleration"))
            {
                if (float.TryParse(result.Data["Deceleration"].Value, out float loadedDeceleration))
                {
                    deceleration = loadedDeceleration;
                    Debug.Log($"Loaded Deceleration value from PlayFab User Data: {deceleration}");
                }
            }
            // You can add more user data checks here if needed
        }
        else
        {
            Debug.Log("No User Data found in PlayFab. Using default values for other parameters.");
        }
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError("PlayFab Error: " + error.GenerateErrorReport());
        Debug.Log("Using default values due to PlayFab error.");
    }

    void LoadSelectedItemImage()
    {
        if (inventoryManager != null)
        {
            inventoryManager.LoadSelectedItemData((itemId, accDescription, imageUrl) =>
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    StartCoroutine(LoadImageFromUrl(imageUrl));
                }
                else
                {
                    Debug.LogWarning("No image URL found for the selected item.");
                }

                // ACC 값을 적용합니다 (선택적)
                if (float.TryParse(accDescription, out float loadedAcc))
                {
                    accelerationRate = loadedAcc;
                    Debug.Log($"Updated acceleration rate from selected item: {accelerationRate}");
                }
            });
        }
        else
        {
            Debug.LogError("PlayFabInventoryManager is not available.");
        }
    }

    IEnumerator LoadImageFromUrl(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = newSprite;
                    Debug.Log("Successfully updated ball sprite with selected item image.");
                }
                else
                {
                    Debug.LogError("SpriteRenderer is null. Cannot apply the loaded image.");
                }
            }
            else
            {
                Debug.LogError($"Failed to load image from URL: {webRequest.error}");
            }
        }
    }
}
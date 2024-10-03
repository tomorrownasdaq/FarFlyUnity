using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class BallManager : MonoBehaviour
{
    public float accelerationRate = 8000f; // Default value
    public float maxSpeed = 20f; // Maximum speed cap
    public float deceleration = 500f; // Default value
    public float maxXPosition = 100f; // Maximum x position for acceleration
    public float maxMapPosition = 100f; // Maximum x position for acceleration

    private Rigidbody2D rb;
    private float currentSpeed = 0f;
    private bool isAccelerating = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing from the ball!");
        }
        // Load values from PlayFab when the game starts
        LoadValuesFromPlayFab();
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

    // Load values from PlayFab
    void LoadValuesFromPlayFab()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), OnTitleDataReceived, OnError);
    }

    // Callback for when title data is received from PlayFab
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

    // Callback for when player data is received from PlayFab
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

    // Callback for when user data is received from PlayFab
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

    // Error callback
    void OnError(PlayFabError error)
    {
        Debug.LogError("PlayFab Error: " + error.GenerateErrorReport());
        Debug.Log("Using default values due to PlayFab error.");
    }
}
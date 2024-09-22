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
            if (currentSpeed > 0)
            {
                currentSpeed = rb.velocity.x - deceleration * Time.fixedDeltaTime;
                rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
            }
            else
            {
                currentSpeed = rb.velocity.x + deceleration * Time.fixedDeltaTime;
                rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
            }
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
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnError);
    }

    // Callback for when data is received from PlayFab
    void OnDataReceived(GetUserDataResult result)
    {
        if (result.Data != null)
        {
            if (result.Data.ContainsKey("AccelerationRate"))
            {
                if (float.TryParse(result.Data["AccelerationRate"].Value, out float loadedAccelerationRate))
                {
                    accelerationRate = loadedAccelerationRate;
                }
            }
            if (result.Data.ContainsKey("Deceleration"))
            {
                if (float.TryParse(result.Data["Deceleration"].Value, out float loadedDeceleration))
                {
                    deceleration = loadedDeceleration;
                }
            }
        }
        else
        {
            Debug.Log("No data found in PlayFab. Using default values: AccelerationRate = 8000, Deceleration = 500");
        }
    }

    // Error callback
    void OnError(PlayFabError error)
    {
        Debug.LogError("PlayFab Error: " + error.GenerateErrorReport());
        Debug.Log("Using default values: AccelerationRate = 8000, Deceleration = 500");
    }
}
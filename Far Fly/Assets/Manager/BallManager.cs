using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BallManager : MonoBehaviour
{
    public float accelerationRate = 5f; // Units per second squared
    public float maxSpeed = 20f; // Maximum speed cap
    public float deceleration = 0.1f; // Rate of slowing down when not accelerating
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


        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Wall"))

            {
                // Reverse the x-velocity
                rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y);

            }
        }


        // Apply the velocity
        //Vector2 newVelocity = rb.velocity;
        //newVelocity.x = currentSpeed;
        //rb.velocity = newVelocity;


        // Clamp the ball's position to prevent it from going beyond maxXPosition
        //if (transform.position.x > maxMapPosition)
        //{
        //    transform.position = new Vector2(maxXPosition, transform.position.y);
        //    currentSpeed = 0f; // Stop the ball
        //}

        // Reset acceleration flag
        isAccelerating = false;
    }
}
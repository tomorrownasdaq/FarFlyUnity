using UnityEngine;
using MoreMountains.Feedbacks;

public class Ball2DCollision : MonoBehaviour
{
    public MMF_Player mmfPlayer;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb.linearVelocity.magnitude >= 400f && mmfPlayer != null && transform.position.x > -200f)
        {
            mmfPlayer.PlayFeedbacks();
        }
    }
}
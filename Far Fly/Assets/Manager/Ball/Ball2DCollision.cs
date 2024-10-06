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

    private void Update()
    {
        if (rb.velocity.magnitude >= 10f && mmfPlayer == null)
        {
            mmfPlayer = GetComponent<MMF_Player>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (mmfPlayer != null && rb.velocity.magnitude >= 100f)
        {
            mmfPlayer.PlayFeedbacks();
        }
    }
}
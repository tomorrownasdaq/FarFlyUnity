using UnityEngine;
using MoreMountains.Feedbacks;

public class BallCollisionEffect : MonoBehaviour
{
    public MMF_Player mmfPlayer;
    public float minSpeedThreshold = 10f;
    private Rigidbody2D rb;

    private void Awake()
    {
        if (mmfPlayer == null)
        {
            mmfPlayer = GetComponent<MMF_Player>();
        }

        if (mmfPlayer != null)
        {
            mmfPlayer.StopFeedbacksOnDisable = true;
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (mmfPlayer != null && rb != null)
        {
            float speed = rb.velocity.magnitude;

            if (speed > minSpeedThreshold)
            {
                EnableMMFPlayer(true);
                mmfPlayer.PlayFeedbacks();
            }
            else
            {
                
                EnableMMFPlayer(false);
                mmfPlayer.StopFeedbacks(true);
                Debug.Log("Ball speed too low for effect: " + speed);
            }
        }
    }

    private void EnableMMFPlayer(bool enable)
    {
        if (mmfPlayer != null)
        {
            mmfPlayer.enabled = enable;
            foreach (var feedback in mmfPlayer.Feedbacks)
            {
                if (feedback != null)
                {
                    feedback.Active = enable;
                }
            }
        }
    }
}
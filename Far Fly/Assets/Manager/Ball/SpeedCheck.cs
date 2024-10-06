using UnityEngine;

public class SpeedChecker : MonoBehaviour
{
    public Rigidbody targetRigidbody; // 속도를 체크할 대상 Rigidbody
    public float speedThreshold = 10f; // 속도 임계값

    public void Update()
    {
        if (targetRigidbody != null && targetRigidbody.velocity.magnitude <= speedThreshold)
        {
            // 속도가 10 이하일 때 실행될 코드
            Debug.Log("속도가 10 이하입니다!");
            // 여기에 원하는 추가 동작을 구현하세요
        }
    }
}
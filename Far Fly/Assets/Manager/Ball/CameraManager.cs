using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform target; // 따라갈 공(ball)
    public float smoothSpeed = 0.125f; // 카메라가 따라가는 속도
    public float leftOffset = 2f; // 공을 화면 왼쪽에 두기 위한 X축 오프셋
    public float verticalOffset = 1f; // 수직 오프셋 (필요에 따라 조정)

    private Camera cam;
    private float halfWidth;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component not found!");
            return;
        }

        // 카메라의 절반 너비 계산
        halfWidth = cam.orthographicSize * cam.aspect;
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("No target set for camera to follow!");
            return;
        }

        // 목표 위치 계산 (공의 오른쪽, 약간 위)
        Vector3 desiredPosition = new Vector3(
            target.position.x + halfWidth - leftOffset,
            target.position.y + verticalOffset,
            transform.position.z
        );

        // 현재 위치에서 목표 위치로 부드럽게 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 카메라 위치 업데이트
        transform.position = smoothedPosition;
    }
}
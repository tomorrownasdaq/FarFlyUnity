using UnityEngine;

public class SpeedChecker : MonoBehaviour
{
    public Rigidbody targetRigidbody; // �ӵ��� üũ�� ��� Rigidbody
    public float speedThreshold = 10f; // �ӵ� �Ӱ谪

    public void Update()
    {
        if (targetRigidbody != null && targetRigidbody.linearVelocity.magnitude <= speedThreshold)
        {
            // �ӵ��� 10 ������ �� ����� �ڵ�
            Debug.Log("�ӵ��� 10 �����Դϴ�!");
            // ���⿡ ���ϴ� �߰� ������ �����ϼ���
        }
    }
}
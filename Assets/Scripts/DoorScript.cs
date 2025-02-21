using System.Collections;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    private static bool isOpen = false; // ��� ���� �����ϴ� ���� ����
    private static bool isAnimating = false; // �ִϸ��̼� ���� �� ����

    private float closedRotationY = 0f; // ���� ���� Y ȸ����
    private float openRotationY; // ���� ���� Y ȸ����
    private float rotationSpeed = 2.0f; // ȸ�� �ӵ� ����

    [SerializeField] private bool isRightDoor = true; // ������ ������ ���� (true = Right, false = Left)

    private void Start()
    {
        // ������ ���̸� 60��, ���� ���̸� -60�� ����
        openRotationY = isRightDoor ? 60f : -60f;

        // ������ Y ȸ���� 0���� ����
        transform.localRotation = Quaternion.Euler(0, closedRotationY, 0);
    }

    public static void ToggleAllDoors()
    {
        if (isAnimating) return; // �ִϸ��̼� ���� ���̸� �ߺ� ���� ����

        isOpen = !isOpen; // ���� ����
        DoorScript[] doors = FindObjectsOfType<DoorScript>(); // ��� DoorScript ã��

        foreach (DoorScript door in doors)
        {
            door.StartCoroutine(door.RotateDoor()); // ��� ���� ���� �ڷ�ƾ ����
        }
    }

    private IEnumerator RotateDoor()
    {
        isAnimating = true; // �ִϸ��̼� ����

        float startRotationY = transform.localEulerAngles.y;
        float targetRotationY = isOpen ? openRotationY : closedRotationY; // ��ǥ ȸ���� ����

        float elapsedTime = 0f;
        float duration = 1.0f; // ���� ������ �����ų� ������ �� �ɸ��� �ð�

        while (elapsedTime < duration)
        {
            float newY = Mathf.LerpAngle(startRotationY, targetRotationY, elapsedTime / duration);
            transform.localRotation = Quaternion.Euler(0, newY, 0);
            elapsedTime += Time.deltaTime * rotationSpeed;
            yield return null;
        }

        // ��Ȯ�� ��ǥ ������ ����
        transform.localRotation = Quaternion.Euler(0, targetRotationY, 0);
        isAnimating = false; // �ִϸ��̼� ����
    }
}

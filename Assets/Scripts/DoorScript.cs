using System.Collections;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    private static bool isOpen = false; // 모든 문이 공유하는 상태 변수
    private static bool isAnimating = false; // 애니메이션 진행 중 여부

    private float closedRotationY = 0f; // 닫힌 상태 Y 회전값
    private float openRotationY; // 열린 상태 Y 회전값
    private float rotationSpeed = 2.0f; // 회전 속도 조절

    [SerializeField] private bool isRightDoor = true; // 오른쪽 문인지 여부 (true = Right, false = Left)

    private void Start()
    {
        // 오른쪽 문이면 60도, 왼쪽 문이면 -60도 설정
        openRotationY = isRightDoor ? 60f : -60f;

        // 강제로 Y 회전을 0도로 설정
        transform.localRotation = Quaternion.Euler(0, closedRotationY, 0);
    }

    public static void ToggleAllDoors()
    {
        if (isAnimating) return; // 애니메이션 진행 중이면 중복 실행 방지

        isOpen = !isOpen; // 상태 변경
        DoorScript[] doors = FindObjectsOfType<DoorScript>(); // 모든 DoorScript 찾기

        foreach (DoorScript door in doors)
        {
            door.StartCoroutine(door.RotateDoor()); // 모든 문에 대해 코루틴 실행
        }
    }

    private IEnumerator RotateDoor()
    {
        isAnimating = true; // 애니메이션 시작

        float startRotationY = transform.localEulerAngles.y;
        float targetRotationY = isOpen ? openRotationY : closedRotationY; // 목표 회전값 설정

        float elapsedTime = 0f;
        float duration = 1.0f; // 문이 완전히 열리거나 닫히는 데 걸리는 시간

        while (elapsedTime < duration)
        {
            float newY = Mathf.LerpAngle(startRotationY, targetRotationY, elapsedTime / duration);
            transform.localRotation = Quaternion.Euler(0, newY, 0);
            elapsedTime += Time.deltaTime * rotationSpeed;
            yield return null;
        }

        // 정확한 목표 각도로 설정
        transform.localRotation = Quaternion.Euler(0, targetRotationY, 0);
        isAnimating = false; // 애니메이션 종료
    }
}

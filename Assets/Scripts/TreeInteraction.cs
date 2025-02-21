using UnityEngine;
using TMPro; // TextMeshPro 사용을 위한 네임스페이스 추가

public class TreeInteraction : MonoBehaviour
{
    public GameObject cutTreePrefab; // 잘린 나무 프리팹
    public int maxHits = 3; // 나무가 잘리기까지 필요한 공격 횟수
    public float detectionRange = 5f; // 나무를 자를 수 있는 최대 거리
    public float detectionAngle = 30f; // 플레이어가 바라보는 방향에서 허용되는 각도 (30도)
    private int currentHits = 0; // 현재 공격 횟수

    // Wood 텍스트 참조
    private TextMeshProUGUI woodText;
    private static int woodCount = 0; // Wood 개수를 추적 (static으로 설정)

    private void Start()
    {
        // Hierarchy에서 "Canvas/Wood" 텍스트 찾기
        GameObject woodTextObject = GameObject.Find("Canvas/Wood");
        if (woodTextObject != null)
        {
            woodText = woodTextObject.GetComponent<TextMeshProUGUI>();
            UpdateWoodText(); // 초기 Wood 값을 설정
        }
        else
        {
            Debug.LogError("Wood Text (Canvas/Wood) not found in the scene.");
        }
    }

    private void Update()
    {
        // E키를 누르면 플레이어가 바라보고 있는 나무를 감지
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckIfTargetedAndHandleHit();
        }
    }

    private void CheckIfTargetedAndHandleHit()
    {
        // 플레이어의 위치와 방향 가져오기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 playerPosition = player.transform.position;
        Vector3 playerForward = player.transform.forward;

        // 나무와 플레이어 사이의 방향 계산
        Vector3 directionToTree = (transform.position - playerPosition).normalized;

        // 플레이어와 나무 사이의 거리 계산
        float distanceToTree = Vector3.Distance(playerPosition, transform.position);

        // 조건 1: 나무가 허용 거리 안에 있는지 확인
        if (distanceToTree > detectionRange)
        {
            return;
        }

        // 조건 2: 나무가 플레이어의 시야각 안에 있는지 확인
        float angleToTree = Vector3.Angle(playerForward, directionToTree);
        if (angleToTree > detectionAngle)
        {
            return;
        }

        // 조건을 모두 만족하면 나무 공격 처리
        HandleHit();
    }

    private void HandleHit()
    {
        currentHits++;

        // 나무 크기 축소 (흔들림 효과)
        transform.localScale *= 0.95f;

        // 공격 횟수가 최대치를 넘으면 나무 교체
        if (currentHits >= maxHits)
        {
            ReplaceWithCutTree();
        }
    }

    private void ReplaceWithCutTree()
    {
        // 현재 나무를 제거하고 잘린 나무로 교체
        Instantiate(cutTreePrefab, transform.position, transform.rotation); // 잘린 나무 생성
        Destroy(gameObject); // 현재 나무 제거

        // Wood 텍스트 값 증가
        if (woodText != null)
        {
            woodCount++;
            UpdateWoodText(); // 텍스트 업데이트
        }
        else
        {
            Debug.LogError("Wood Text reference is missing.");
        }
    }

    private void UpdateWoodText()
    {
        // Wood 텍스트 업데이트
        woodText.text = $"Wood: {woodCount}";
    }
}

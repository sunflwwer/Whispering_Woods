using UnityEngine;
/* using TMPro; */ // TextMeshPro ����� ���� ���ӽ����̽� �ּ� ó��

public class TreeInteraction : MonoBehaviour
{
    public GameObject cutTreePrefab; // �߸� ���� ������
    public int maxHits = 3; // ������ �߸������ �ʿ��� ���� Ƚ��
    public float detectionRange = 5f; // ������ �ڸ� �� �ִ� �ִ� �Ÿ�
    public float detectionAngle = 30f; // �÷��̾ �ٶ󺸴� ���⿡�� ���Ǵ� ���� (30��)
    private int currentHits = 0; // ���� ���� Ƚ��

    // Wood �ؽ�Ʈ ���� �ּ� ó��
    /*
    private TextMeshProUGUI woodText;
    private static int woodCount = 0; // Wood ������ ���� (static���� ����)
    */

    private void Start()
    {
        // Hierarchy���� "Canvas/Wood" �ؽ�Ʈ ã�� �ּ� ó��
        /*
        GameObject woodTextObject = GameObject.Find("Canvas/Wood");
        if (woodTextObject != null)
        {
            woodText = woodTextObject.GetComponent<TextMeshProUGUI>();
            UpdateWoodText(); // �ʱ� Wood ���� ����
        }
        else
        {
            //Debug.LogError("Wood Text (Canvas/Wood) not found in the scene.");
        }
        */
    }

    private void Update()
    {
        // EŰ �Է¿� ���� ��ȣ�ۿ� ����
        /*
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckIfTargetedAndHandleHit();
        }
        */
    }

    /*
    // EŰ�� ���� ��ȣ�ۿ� �����ϴ� �Լ� ����
    private void CheckIfTargetedAndHandleHit()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 playerPosition = player.transform.position;
        Vector3 playerForward = player.transform.forward;

        Vector3 directionToTree = (transform.position - playerPosition).normalized;
        float distanceToTree = Vector3.Distance(playerPosition, transform.position);

        if (distanceToTree > detectionRange)
        {
            return;
        }

        float angleToTree = Vector3.Angle(playerForward, directionToTree);
        if (angleToTree > detectionAngle)
        {
            return;
        }

        HandleHit();
    }
    */

    private void HandleHit()
    {
        currentHits++;

        // ���� ũ�� ��� (��鸲 ȿ��)
        transform.localScale *= 0.95f;

        // ���� Ƚ���� �ִ�ġ�� ������ ���� ��ü
        if (currentHits >= maxHits)
        {
            ReplaceWithCutTree();
        }
    }

    private void ReplaceWithCutTree()
    {
        // ���� ������ �����ϰ� �߸� ������ ��ü
        Instantiate(cutTreePrefab, transform.position, transform.rotation); // �߸� ���� ����
        Destroy(gameObject); // ���� ���� ����

        // Wood �ؽ�Ʈ �� ���� �ּ� ó��
        /*
        if (woodText != null)
        {
            woodCount++;
            UpdateWoodText(); // �ؽ�Ʈ ������Ʈ
        }
        else
        {
            Debug.LogError("Wood Text reference is missing.");
        }
        */
    }

    /*
    private void UpdateWoodText()
    {
        woodText.text = $"Wood: {woodCount}";
    }
    */
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Sample;
using TMPro; // TextMeshPro ���

public class SpiderScript : MonoBehaviour
{
    private Animator animator;
    private int hitCount = 0;
    private const int maxHits = 10;
    private bool isDead = false;
    private Transform player;
    private Slider healthBar;
    private Canvas healthCanvas;
    private Coroutine hideHealthBarCoroutine;
    private Rigidbody rb;

    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float detectRange = 50.0f;
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackCooldown = 2.0f;
    [SerializeField] private float repulsionForce = 0.0f;
    private float lastAttackTime = 0;

    // DayNightCycle ���� ���� �߰�
    private DayNightCycle dayNightCycle;
    private bool isNightState = false; // ���� �� ���� ����

    // ���� ���� �߰�
    private Vector3 originalScale; // ���� ũ�� ����
    private int originalAttackDamage; // ���� ���ݷ� ����
    private float originalAttackRange; // ���� ���� ���� ����

    private bool isPlayerDead = false; // �÷��̾ �׾����� ����

    // ���� �Ź� ī��Ʈ�� ���� ���� ����
    private static int totalSpidersKilled = 0;
    private TMP_Text spiderText; // Canvas���� Spider �ؽ�Ʈ ����

    // Gravestone ������Ʈ ���� �߰�
    private GameObject gravestone03;
    private GameObject gravestone033;


    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.mass = 1f;
        rb.drag = 5f;
        rb.angularDrag = 5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (animator == null)
        {
            Debug.LogError("Animator component is missing on Spider!");
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player object with tag 'Player' not found!");
        }

        Transform canvasTransform = transform.Find("Canvas");
        if (canvasTransform != null)
        {
            healthCanvas = canvasTransform.GetComponent<Canvas>();
            healthBar = canvasTransform.GetComponentInChildren<Slider>();

            if (healthBar != null)
            {
                healthBar.maxValue = maxHits;
                healthBar.value = maxHits;
                healthCanvas.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Slider component not found in Spider's Canvas!");
            }
        }
        else
        {
            Debug.LogError("Canvas not found in Spider prefab!");
        }

        // DayNightCycle ã��
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        if (dayNightCycle == null)
        {
            Debug.LogError("DayNightCycle is not found in the scene.");
        }

        // ���� ũ��� ���ݷ� ����
        originalScale = transform.localScale;
        originalAttackDamage = attackDamage;
        originalAttackRange = attackRange; // ���� ���� ���� ����

        CheckNightState(); // ���� �� �� ���� Ȯ��

        // Spider �ؽ�Ʈ �ʱ�ȭ
        GameObject spiderTextObject = GameObject.Find("Canvas/Spider");
        if (spiderTextObject != null)
        {
            spiderText = spiderTextObject.GetComponent<TextMeshProUGUI>();
            if (spiderText != null)
            {
                spiderText.text = $"Spider = {totalSpidersKilled}"; // �ʱⰪ ����
            }
            else
            {
                Debug.LogError("Spider �ؽ�Ʈ ������Ʈ�� ã�� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError("Canvas �ȿ� 'Spider' �ؽ�Ʈ ������Ʈ�� ã�� �� �����ϴ�.");
        }


        // Gravestone �׷� ���� ������Ʈ ã��
        GameObject etcGroup = GameObject.Find("ETC");
        if (etcGroup != null)
        {
            Transform gravestoneGroup = etcGroup.transform.Find("Gravestone group");
            if (gravestoneGroup != null)
            {
                gravestone03 = gravestoneGroup.Find("PT_Menhir_Rock_03")?.gameObject;
                gravestone033 = gravestoneGroup.Find("PT_Menhir_Rock_033")?.gameObject;

                if (gravestone03 == null || gravestone033 == null)
                {
                    Debug.LogError("Gravestone ������Ʈ���� ã�� �� �����ϴ�.");
                }
            }
        }

    }


    void Update()
    {
        if (player != null)
        {
            GhostScript playerScript = player.GetComponent<GhostScript>();
            if (playerScript != null)
            {
                isPlayerDead = playerScript.IsDead(); // �÷��̾ �׾����� Ȯ��
            }
        }

        if (isPlayerDead) // �÷��̾ ������ �ڿ������� Idle �ִϸ��̼����� ��ȯ
        {
            animator.SetBool("isWalking", false);
            animator.CrossFade("Idle", 0.5f); // 0.5�� ���� �ε巴�� Idle�� ��ȯ
            return; // ���� �ڵ� ���� �� ��
        }

        if (player != null && !isDead)
        {
            if (healthCanvas != null)
            {
                healthCanvas.transform.LookAt(player);
            }

            AvoidOtherSpiders();

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectRange)
            {
                ChasePlayer(distanceToPlayer);
            }
            else
            {
                animator.SetBool("isWalking", false);
            }
        }

        CheckNightState(); // �㳷 ���� üũ
    }



    private void CheckNightState()
    {
        if (dayNightCycle == null) return;

        bool currentNightState = dayNightCycle.IsEvening;

        if (currentNightState != isNightState)
        {
            isNightState = currentNightState;

            if (isNightState)
            {
                // ���� �Ǹ� ũ�� 1.5��, ���ݷ� 2��, ���� ���� 1.5�� ����
                StartCoroutine(ChangeSizeSmoothly(originalScale * 1.5f, 0.5f)); // 0.5�� ���� �ε巴�� ũ�� ����
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 2f); // �ݿø��Ͽ� ������ ��ȯ
                attackRange = originalAttackRange * 1.5f;
            }
            else
            {
                // ���� �Ǹ� ���� ũ��, ���ݷ�, ���� ������ ����
                StartCoroutine(ChangeSizeSmoothly(originalScale, 0.5f)); // 0.5�� ���� �ε巴�� ũ�� ����
                attackDamage = originalAttackDamage;
                attackRange = originalAttackRange;
            }

            //Debug.Log($"Night State Changed: isNightState = {isNightState}, attackRange = {attackRange}");
        }
    }

    private IEnumerator ChangeSizeSmoothly(Vector3 targetScale, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startingScale = transform.localScale;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(startingScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }


    private void ChasePlayer(float distanceToPlayer)
    {
        if (isPlayerDead) // �÷��̾ �׾��ٸ� ��� ���� �ߴ�
        {
            animator.SetBool("isWalking", false);
            return;
        }

        if (distanceToPlayer > attackRange) // ���� ���� ���̸� �̵�
        {
            Vector3 direction = (player.position - transform.position).normalized;

            Vector3 targetPosition = transform.position + direction * moveSpeed * Time.deltaTime;
            rb.MovePosition(targetPosition);

            Quaternion targetRotation = Quaternion.LookRotation(-direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

            animator.SetBool("isWalking", true);
        }
        else // ���� ���� �ȿ� ������ ����
        {
            animator.SetBool("isWalking", false);

            if (Time.time - lastAttackTime > attackCooldown)
            {
                lastAttackTime = Time.time;
                animator.SetTrigger("Attack2Trigger");

                GhostScript playerScript = player.GetComponent<GhostScript>();
                if (playerScript != null && !playerScript.IsDead()) // �÷��̾ ���� �ʾ��� ���� ����
                {
                    playerScript.TakeDamage(attackDamage);
                }
            }
        }
    }




    private void AvoidOtherSpiders()
    {
        float minDistance = 1.5f;
        Collider[] colliders = Physics.OverlapSphere(transform.position, minDistance);

        Vector3 avoidanceVector = Vector3.zero;
        int numAvoiding = 0;

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("SpiderEnemy"))
            {
                Vector3 pushDirection = transform.position - collider.transform.position;
                avoidanceVector += pushDirection.normalized;
                numAvoiding++;
            }
        }

        if (numAvoiding > 0)
        {
            avoidanceVector /= numAvoiding;
            avoidanceVector.y = 0;
            rb.MovePosition(rb.position + avoidanceVector * repulsionForce * Time.deltaTime);
        }
    }

    public void TakeDamage()
    {
        if (isDead) return;

        hitCount++;
        Debug.Log($"Spider hit {hitCount} times");

        if (healthBar != null)
        {
            healthBar.value = maxHits - hitCount;
        }

        ShowHealthBarTemporarily();

        if (hitCount >= maxHits)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("TakeDamageTrigger");
        }
    }

    private void ShowHealthBarTemporarily()
    {
        if (healthCanvas == null) return;

        healthCanvas.gameObject.SetActive(true);

        if (hideHealthBarCoroutine != null)
        {
            StopCoroutine(hideHealthBarCoroutine);
        }

        hideHealthBarCoroutine = StartCoroutine(HideHealthBarAfterDelay());
    }

    private IEnumerator HideHealthBarAfterDelay()
    {
        yield return new WaitForSeconds(2.0f);
        if (!isDead)
        {
            healthCanvas.gameObject.SetActive(false);
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Spider has died");
        animator.SetTrigger("DeathTrigger");

        if (healthCanvas != null)
        {
            healthCanvas.gameObject.SetActive(false);
        }


        // �Ź� ��� �� ī��Ʈ ���� �� �ؽ�Ʈ ������Ʈ
        totalSpidersKilled++;
        UpdateSpiderText();

        // 5���� �̻� �׾��� �� Gravestone ������Ʈ ��ȯ
        if (totalSpidersKilled >= 5)
        {
            ToggleGravestoneObjects();
        }


        StartCoroutine(RemoveSpiderAfterDeath());
    }

    private void ToggleGravestoneObjects()
    {
        if (gravestone03 != null && gravestone033 != null)
        {
            gravestone03.SetActive(false); // PT_Menhir_Rock_03 ��Ȱ��ȭ
            gravestone033.SetActive(true); // PT_Menhir_Rock_033 Ȱ��ȭ
            Debug.Log("PT_Menhir_Rock_03 ��Ȱ��ȭ, PT_Menhir_Rock_033 Ȱ��ȭ �Ϸ�");
        }
    }


    // Spider �ؽ�Ʈ ������Ʈ
    private void UpdateSpiderText()
    {
        if (spiderText != null)
        {
            spiderText.text = $"Spider = {totalSpidersKilled}";
        }
    }

    private IEnumerator RemoveSpiderAfterDeath()
    {
        yield return new WaitForSeconds(2.0f);
        TerrainObjectManager terrainManager = FindObjectOfType<TerrainObjectManager>();

        if (terrainManager != null)
        {
            terrainManager.OnSpiderRemoved(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


}

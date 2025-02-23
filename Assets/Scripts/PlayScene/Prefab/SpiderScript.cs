using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Sample;
using TMPro; // TextMeshPro 사용

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

    // DayNightCycle 참조 변수 추가
    private DayNightCycle dayNightCycle;
    private bool isNightState = false; // 현재 밤 상태 저장

    // 기존 변수 추가
    private Vector3 originalScale; // 원래 크기 저장
    private int originalAttackDamage; // 원래 공격력 저장
    private float originalAttackRange; // 원래 공격 범위 저장

    private bool isPlayerDead = false; // 플레이어가 죽었는지 여부

    // 죽은 거미 카운트를 위한 전역 변수
    private static int totalSpidersKilled = 0;
    private TMP_Text spiderText; // Canvas에서 Spider 텍스트 추적

    // Gravestone 오브젝트 참조 추가
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

        // DayNightCycle 찾기
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        if (dayNightCycle == null)
        {
            Debug.LogError("DayNightCycle is not found in the scene.");
        }

        // 원래 크기와 공격력 저장
        originalScale = transform.localScale;
        originalAttackDamage = attackDamage;
        originalAttackRange = attackRange; // 원래 공격 범위 저장

        CheckNightState(); // 시작 시 밤 상태 확인

        // Spider 텍스트 초기화
        GameObject spiderTextObject = GameObject.Find("Canvas/Spider");
        if (spiderTextObject != null)
        {
            spiderText = spiderTextObject.GetComponent<TextMeshProUGUI>();
            if (spiderText != null)
            {
                spiderText.text = $"Spider = {totalSpidersKilled}"; // 초기값 설정
            }
            else
            {
                Debug.LogError("Spider 텍스트 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("Canvas 안에 'Spider' 텍스트 오브젝트를 찾을 수 없습니다.");
        }


        // Gravestone 그룹 내의 오브젝트 찾기
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
                    Debug.LogError("Gravestone 오브젝트들을 찾을 수 없습니다.");
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
                isPlayerDead = playerScript.IsDead(); // 플레이어가 죽었는지 확인
            }
        }

        if (isPlayerDead) // 플레이어가 죽으면 자연스럽게 Idle 애니메이션으로 전환
        {
            animator.SetBool("isWalking", false);
            animator.CrossFade("Idle", 0.5f); // 0.5초 동안 부드럽게 Idle로 전환
            return; // 이후 코드 실행 안 함
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

        CheckNightState(); // 밤낮 상태 체크
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
                // 밤이 되면 크기 1.5배, 공격력 2배, 공격 범위 1.5배 증가
                StartCoroutine(ChangeSizeSmoothly(originalScale * 1.5f, 0.5f)); // 0.5초 동안 부드럽게 크기 변경
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 2f); // 반올림하여 정수로 변환
                attackRange = originalAttackRange * 1.5f;
            }
            else
            {
                // 낮이 되면 원래 크기, 공격력, 공격 범위로 복귀
                StartCoroutine(ChangeSizeSmoothly(originalScale, 0.5f)); // 0.5초 동안 부드럽게 크기 변경
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
        if (isPlayerDead) // 플레이어가 죽었다면 즉시 공격 중단
        {
            animator.SetBool("isWalking", false);
            return;
        }

        if (distanceToPlayer > attackRange) // 공격 범위 밖이면 이동
        {
            Vector3 direction = (player.position - transform.position).normalized;

            Vector3 targetPosition = transform.position + direction * moveSpeed * Time.deltaTime;
            rb.MovePosition(targetPosition);

            Quaternion targetRotation = Quaternion.LookRotation(-direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

            animator.SetBool("isWalking", true);
        }
        else // 공격 범위 안에 들어오면 공격
        {
            animator.SetBool("isWalking", false);

            if (Time.time - lastAttackTime > attackCooldown)
            {
                lastAttackTime = Time.time;
                animator.SetTrigger("Attack2Trigger");

                GhostScript playerScript = player.GetComponent<GhostScript>();
                if (playerScript != null && !playerScript.IsDead()) // 플레이어가 죽지 않았을 때만 공격
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


        // 거미 사망 시 카운트 증가 및 텍스트 업데이트
        totalSpidersKilled++;
        UpdateSpiderText();

        // 5마리 이상 죽었을 때 Gravestone 오브젝트 전환
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
            gravestone03.SetActive(false); // PT_Menhir_Rock_03 비활성화
            gravestone033.SetActive(true); // PT_Menhir_Rock_033 활성화
            Debug.Log("PT_Menhir_Rock_03 비활성화, PT_Menhir_Rock_033 활성화 완료");
        }
    }


    // Spider 텍스트 업데이트
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

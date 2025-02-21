using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Sample
{
    public class GhostScript : MonoBehaviour
    {
        private Animator Anim;
        private CharacterController Ctrl;
        private Vector3 MoveDirection = Vector3.zero;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
        private static readonly int SurprisedState = Animator.StringToHash("Base Layer.surprised");
        private static readonly int AttackState = Animator.StringToHash("Base Layer.attack_shift");
        private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");
        private static readonly int AttackTag = Animator.StringToHash("Attack");

        [SerializeField] private SkinnedMeshRenderer[] MeshR;
        private float Dissolve_value = 1;
        private bool DissolveFlg = false;
        private const int maxHP = 100;
        private int HP = 50;
        private TextMeshProUGUI HP_text;

        [SerializeField] private float Speed = 3;
        [SerializeField] private float sprintSpeed = 5.5f;

        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform playerBody;
        [SerializeField] private float mouseSensitivity = 100f;

        private float xRotation = 0f;

        private bool isJumping = false;
        [SerializeField] private float jumpForce = 4.5f;
        [SerializeField] private float fallMultiplier = 2.0f;
        private float verticalVelocity = 0f;

        private bool isMoving = false;

        [SerializeField] private float waterSpeedMultiplier = 0.5f;
        private bool isInWater = false;

        // [SerializeField] private float waterHeight = 0f;

        // �߰��� ����
        private GameObject currentRock = null; // ���� �� �� �ִ� ��
        private bool isPushing = false; // �� �б� ����
        [SerializeField] private float pushSpeed = 3f; // �� �б� �ӵ�

        private bool isDead = false; // �÷��̾ �׾����� ����


        void Start()
        {
            Anim = this.GetComponent<Animator>();
            Ctrl = this.GetComponent<CharacterController>();

            initialPosition = this.transform.position;
            initialRotation = this.transform.rotation;

            try
            {
                HP_text = GameObject.Find("Canvas/HP").GetComponent<TextMeshProUGUI>();
                if (HP_text != null)
                {
                    HP_text.text = "HP " + HP.ToString();
                }
                else
                {
                    Debug.LogError("HP_text is null. Please check the Canvas/HP setup.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error finding HP text: " + e.Message);
            }

            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
            }

            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            HandleMouseLook();
            STATUS();
            MOVE();
            HandleKeyActions();
            HandlePush(); // �߰��� �� �б� ����
            Respawn();

            if (HP <= 0 && !DissolveFlg)
            {
                Anim.CrossFade(DissolveState, 0.1f, 0, 0);
                DissolveFlg = true;
            }
            else if (HP == maxHP && DissolveFlg)
            {
                DissolveFlg = false;
            }
        }

        private const int Dissolve = 1;
        private const int Attack = 2;
        private const int Surprised = 3;
        private Dictionary<int, bool> PlayerStatus = new Dictionary<int, bool>
        {
            { Dissolve, false },
            { Attack, false },
            { Surprised, false },
        };

        private void STATUS()
        {
            if (DissolveFlg && HP <= 0)
            {
                PlayerStatus[Dissolve] = true;
            }
            else if (!DissolveFlg)
            {
                PlayerStatus[Dissolve] = false;
            }

            if (Anim.GetCurrentAnimatorStateInfo(0).tagHash == AttackTag)
            {
                PlayerStatus[Attack] = true;
            }
            else if (Anim.GetCurrentAnimatorStateInfo(0).tagHash != AttackTag)
            {
                PlayerStatus[Attack] = false;
            }

            if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash == SurprisedState)
            {
                PlayerStatus[Surprised] = true;
            }
            else if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash != SurprisedState)
            {
                PlayerStatus[Surprised] = false;
            }
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            playerBody.Rotate(Vector3.up * mouseX);

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 60f);

            Vector3 targetCameraPosition = cameraTransform.localPosition;
            float verticalOffset = Mathf.Lerp(0.5f, 4.0f, Mathf.InverseLerp(-90f, 60f, xRotation));
            targetCameraPosition.y = verticalOffset;
            targetCameraPosition.z = -3f;

            cameraTransform.localPosition = targetCameraPosition;
            cameraTransform.LookAt(playerBody.position + Vector3.up * 1.5f);
        }

        private void MOVE()
        {
            if (isDead) return; // �÷��̾ �׾����� �̵� ���� ���� �� ��

            ApplyGravity();

            float currentSpeed = Speed;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = sprintSpeed;
            }

            Vector3 inputDirection = new Vector3(
                Input.GetAxis("Horizontal"),
                0,
                Input.GetAxis("Vertical")
            );

            if (inputDirection.magnitude > 0.1f)
            {
                inputDirection.Normalize();

                Vector3 moveDirection = playerBody.forward * inputDirection.z + playerBody.right * inputDirection.x;
                Vector3 horizontalMove = moveDirection * currentSpeed;
                MoveDirection = new Vector3(horizontalMove.x, MoveDirection.y, horizontalMove.z);

                Ctrl.Move(MoveDirection * Time.deltaTime);

                if (!isMoving)
                {
                    Anim.CrossFade(MoveState, 0.1f, 0, 0);
                    isMoving = true;
                }
            }
            else
            {
                MoveDirection = Vector3.Lerp(MoveDirection, Vector3.zero, 10 * Time.deltaTime);
                Ctrl.Move(MoveDirection * Time.deltaTime);

                if (isMoving)
                {
                    Anim.CrossFade(IdleState, 0.1f, 0, 0);
                    isMoving = false;
                }
            }
        }

        private void HandleKeyActions()
        {
            if (isDead) return; // �÷��̾ �׾����� ���� �Ұ���

            if (Input.GetMouseButtonDown(0)) // ���콺 ���� ��ư���� ����
            {
                Anim.CrossFade(AttackState, 0.1f, 0, 0);

                Vector3 rayStartPos = cameraTransform.position + Vector3.up * 0.2f; // �������� 0.2f ������ ����


                // Ray ���� (��¦ �Ʒ��� ����)
                Vector3 rayDirection = cameraTransform.forward + Vector3.down * 0.3f;
                rayDirection.Normalize(); // ����ȭ

                // Ray �Ÿ�
                float rayDistance = 6.0f;

                // Debug�� Ray �ð�ȭ
                Debug.DrawRay(rayStartPos, rayDirection * rayDistance, Color.red, 1.0f);

                // RaycastAll ��� �� Ʈ���ű��� ���� ����
                RaycastHit[] hits = Physics.RaycastAll(rayStartPos, rayDirection, rayDistance);

                // ���� ����� �Ź� �� ������ �����ϱ� ���� ���� �߰�
                RaycastHit? closestHit = null;
                float closestDistance = float.MaxValue;

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("SpiderEnemy"))
                    {
                        float distance = Vector3.Distance(rayStartPos, hit.point);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestHit = hit;
                        }
                    }
                }

                // ���� ����� �Ź� �� ������ ����
                if (closestHit.HasValue)
                {
                    SpiderScript spider = closestHit.Value.collider.GetComponentInParent<SpiderScript>();
                    if (spider != null)
                    {
                        Debug.Log("Spider hit detected! Calling TakeDamage()");
                        spider.TakeDamage();
                    }
                    else
                    {
                        Debug.LogError("SpiderScript not found on hit object!");
                    }
                }
                else
                {
                    Debug.Log("No valid hit detected with Raycast.");
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1)) // ���콺 ������ ��ư���� �� ȸ��
            {
                InteractWithFence();
                Debug.Log("InteractWithFence.");
            }

        }

        private void InteractWithFence()
        {
            float interactionDistance = 7.0f; // ������ ��ȣ�ۿ� �Ÿ�
            Vector3 rayStartPos = cameraTransform.position; // ī�޶� ��ġ���� ����
            Vector3 rayDirection = cameraTransform.forward; // �÷��̾ �ٶ󺸴� ����

            Debug.DrawRay(rayStartPos, rayDirection * interactionDistance, Color.blue, 2.0f); // Ray�� �ð������� Ȯ��

            RaycastHit[] hits = Physics.RaycastAll(rayStartPos, rayDirection, interactionDistance, ~0, QueryTriggerInteraction.Collide);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Fence")) // Fence �±� ����
                {
                    Debug.Log("Fence detected, toggling doors...");
                    DoorScript.ToggleAllDoors(); // ��� �� ���ÿ� ���ų� �ݱ�
                    return;
                }
            }
        }





        public void TakeDamage(int damage)
        {
            if (isDead) return; // �̹� �׾����� �߰� ���� ���� ����

            HP = Mathf.Max(HP - damage, 0);
            UpdateHPUI(); // ü�� UI ������Ʈ

            if (HP > 0)
            {
                // ���� �ִϸ��̼� ���� (���� ���� ���)
                Anim.CrossFade(SurprisedState, 0.1f, 0, 0);
            }
            else
            {
                // HP�� 0�̸� ��� ó��
                isDead = true;
                Die();
            }
        }


        private void Die()
        {
            isDead = true;

            Debug.Log("Player has died!");
            Anim.CrossFade(DissolveState, 0.1f, 0, 0); // ��� �ִϸ��̼� ����

            // �̵� �� �ǰ� �Ұ� ó��
            Ctrl.enabled = false;
            GetComponent<Collider>().enabled = false;

            // 1�� �� Dissolve ȿ�� ����
            StartCoroutine(WaitBeforeDissolve());
        }

        // 1�� ��� �� Dissolve ȿ�� ����
        private IEnumerator WaitBeforeDissolve()
        {
            yield return new WaitForSeconds(1.0f); // 1�� ���
            StartCoroutine(DissolveEffect()); // Dissolve ȿ�� ����
        }

        // ������ ������� ȿ�� �ڷ�ƾ
        private IEnumerator DissolveEffect()
        {
            while (Dissolve_value > 0)
            {
                Dissolve_value -= Time.deltaTime * 0.5f; // õõ�� ��������� ����
                foreach (var mesh in MeshR)
                {
                    mesh.material.SetFloat("_Dissolve", Dissolve_value);
                }
                yield return null;
            }
        }



        private void HandlePush()
        {
            // VŰ�� ������ �� �б� ����
            if (Input.GetKeyDown(KeyCode.V) && currentRock != null)
            {
                isPushing = true;
                Debug.Log("Started pushing the rock");
            }

            // VŰ�� ���� �� �б� ����
            if (Input.GetKeyUp(KeyCode.V))
            {
                isPushing = false;
                Debug.Log("Stopped pushing the rock");

                // ���� �ӵ��� ȸ���� ������ ����
                if (currentRock != null)
                {
                    Rigidbody rockRb = currentRock.GetComponent<Rigidbody>();
                    if (rockRb != null)
                    {
                        rockRb.velocity = Vector3.zero;
                        rockRb.angularVelocity = Vector3.zero;
                    }
                }
            }

            // �а� �ִ� ������ ���� �� �̵�
            if (isPushing && currentRock != null)
            {
                Rigidbody rockRb = currentRock.GetComponent<Rigidbody>();
                Vector3 pushDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

                if (pushDirection.magnitude > 0.1f)
                {
                    // ���� �Է� �������� �̵�
                    rockRb.velocity = pushDirection * pushSpeed;

                    // ���� �׻� ���� �پ� �ֵ��� ���� (�߷� ����)
                    Vector3 correctedPosition = currentRock.transform.position;
                    correctedPosition.y = Mathf.Max(correctedPosition.y, transform.position.y - 0.1f);
                    currentRock.transform.position = correctedPosition;
                }
                else
                {
                    // �Է��� ������ �ӵ��� ����
                    rockRb.velocity = Vector3.zero;
                }
            }
            else if (currentRock != null)
            {
                // VŰ�� ������ �ʾ��� �� �ӵ��� ������ 0���� ����
                Rigidbody rockRb = currentRock.GetComponent<Rigidbody>();
                if (rockRb != null)
                {
                    rockRb.velocity = Vector3.zero;

                    // �߷� �������� ���� ���� �پ� �ֵ��� ����
                    Vector3 correctedPosition = currentRock.transform.position;
                    correctedPosition.y = Mathf.Max(correctedPosition.y, transform.position.y - 0.1f);
                    currentRock.transform.position = correctedPosition;
                }
            }
        }





        private void ApplyGravity()
        {
            if (CheckGrounded())
            {
                if (!isJumping)
                {
                    verticalVelocity = -0.1f;
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    verticalVelocity = jumpForce;
                    isJumping = true;
                }
            }
            else
            {
                if (verticalVelocity < 0)
                {
                    verticalVelocity += Physics.gravity.y * fallMultiplier * Time.deltaTime;
                }
                else
                {
                    verticalVelocity += Physics.gravity.y * Time.deltaTime;
                }
            }

            if (isJumping && verticalVelocity <= 0 && CheckGrounded())
            {
                isJumping = false;
            }

            MoveDirection.y = verticalVelocity;
        }

        private bool CheckGrounded()
        {
            if (Ctrl.isGrounded)
            {
                return true;
            }

            Ray ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
            float range = isInWater ? 0.5f : 0.3f; // �������� �� ���� ������ ����
            Debug.DrawRay(ray.origin, ray.direction * range, Color.red);

            // Trigger Collider�� �����ϵ��� QueryTriggerInteraction.Ignore ����
            return Physics.Raycast(ray, range, ~0, QueryTriggerInteraction.Ignore);
        }

        private void Respawn()
        {
            if (Input.GetKeyDown(KeyCode.R)) // RŰ�� ������ ������ ����
            {
                Debug.Log("Respawning player...");

                // ü�� 50���� ȸ��
                HP = 50;
                UpdateHPUI(); // ü�� UI ������Ʈ

                // �ʱ� ��ġ �� ���� ����
                this.transform.position = initialPosition;
                this.transform.rotation = initialRotation;

                // �̵� �� �ǰ� �����ϵ��� ����
                Ctrl.enabled = true;
                GetComponent<Collider>().enabled = true;

                // Dissolve ȿ�� �ʱ�ȭ
                Dissolve_value = 1;
                foreach (var mesh in MeshR)
                {
                    mesh.material.SetFloat("_Dissolve", Dissolve_value);
                }

                // �ִϸ��̼� �ʱ�ȭ
                Anim.CrossFade(IdleState, 0.1f, 0, 0);

                isDead = false; // �ٽ� ��Ƴ�
                Debug.Log("Player respawned at initial position with HP: " + HP);
            }
        }


        public bool IsDead()
        {
            return isDead;
        }



        public void Heal(int amount)
        {
            HP = Mathf.Min(HP + amount, maxHP);

            if (HP_text != null)
            {
                HP_text.text = "HP " + HP.ToString();
            }
            else
            {
                Debug.LogError("HP_text is null. Cannot update HP text.");
            }
        }

        public int GetCurrentHP()
        {
            return HP;
        }

        private void UpdateHPUI()
        {
            if (HP_text != null)
            {
                HP_text.text = "HP " + HP.ToString();
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            // �� ��ȣ�ۿ�
            if (other.CompareTag("Water"))
            {
                Debug.Log("Entered Water Trigger");
                if (!isInWater)
                {
                    isInWater = true;
                    Speed *= waterSpeedMultiplier;
                    sprintSpeed *= waterSpeedMultiplier;
                }
            }

            // �� ��ȣ�ۿ�
            if (other.CompareTag("PushableRock"))
            {
                currentRock = other.gameObject;
                Debug.Log($"Entered range of rock: {currentRock.name}");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // �� ��ȣ�ۿ� ����
            if (other.CompareTag("Water"))
            {
                Debug.Log("Exited Water Trigger");
                if (isInWater)
                {
                    isInWater = false;
                    Speed /= waterSpeedMultiplier;
                    sprintSpeed /= waterSpeedMultiplier;
                }
            }

            // �� ��ȣ�ۿ� ����
            if (other.CompareTag("PushableRock") && currentRock == other.gameObject)
            {
                Debug.Log($"Exited range of rock: {currentRock.name}");
                currentRock = null;
                isPushing = false;
            }
        }

    }
}

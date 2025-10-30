using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    #region 이동 및 점프 설정
    [Header("속도 설정")]
    [SerializeField] private float speedWalking = 5f;      // 걷기 속도
    [SerializeField] private float speedRunning = 9f;      // 달리기 속도
    [SerializeField] private float jumpForce = 5f;         // 점프 힘
    #endregion

    #region 발걸음 오디오
    [Header("오디오 클립")]
    [SerializeField] private AudioClip audioClipWalking;
    [SerializeField] private AudioClip audioClipRunning;
    private AudioSource audioSource;
    #endregion

    #region 체력 설정
    [Header("체력 설정")]
    public int maxHealth = 100;
    private int currentHealth;
    public Slider healthBar;

    [Header("피격 설정")]
    public int damagePerHit = 20;
    public float invincibleTime = 1f;
    public float blinkInterval = 0.1f;
    private bool isInvincible = false;
    private Renderer playerRenderer;
    #endregion

    #region 내부 컴포넌트
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private bool grounded = false;
    private readonly RaycastHit[] groundHits = new RaycastHit[8];
    #endregion

    #region Unity 기본 함수

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true; // ✅ 중력 활성화

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.clip = audioClipWalking;
        }

        playerRenderer = GetComponentInChildren<Renderer>();

        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.maxValue = maxHealth;
    }

    private void Update()
    {
        // 점프 입력
        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        PlayFootstepSounds();
    }

    private void FixedUpdate()
    {
        MoveCharacter();

        // 다음 물리 프레임 전 grounded 리셋
        grounded = false;
    }

    private void OnCollisionStay()
    {
        // 바닥 체크
        Bounds bounds = capsule.bounds;
        float radius = bounds.extents.x - 0.01f;

        Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
            groundHits, bounds.extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);

        if (groundHits.Any(hit => hit.collider != null && hit.collider != capsule))
        {
            grounded = true;
        }

        // 배열 초기화
        for (int i = 0; i < groundHits.Length; i++)
            groundHits[i] = new RaycastHit();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            TryTakeDamage(damagePerHit);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            TryTakeDamage(damagePerHit);
        }
    }

    #endregion

    #region 이동 처리

    private void MoveCharacter()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 이동 입력
        Vector3 movement = new Vector3(horizontal, 0, vertical);

        // Shift + 전진(W)일 때만 달리기
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && vertical > 0f;
        movement *= isRunning ? speedRunning : speedWalking;

        // 로컬 → 월드 좌표
        movement = transform.TransformDirection(movement);

        // 기존 Y속도 그대로 유지 (점프/중력)
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
    }



    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // 기존 상승속도 초기화
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);     // 점프력 적용
    }

    private void PlayFootstepSounds()
    {
        if (audioSource == null) return;

        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (grounded && flatVelocity.sqrMagnitude > 0.1f)
        {
            audioSource.clip = Input.GetKey(KeyCode.LeftShift) ? audioClipRunning : audioClipWalking;
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Pause();
        }
    }

    #endregion

    #region 체력 및 피격 처리

    private void TryTakeDamage(int amount)
    {
        if (!isInvincible)
        {
            TakeDamage(amount);
            StartCoroutine(InvincibleRoutine());
        }
    }

    private void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (healthBar != null)
            healthBar.value = currentHealth;

        Debug.Log($"플레이어 체력: {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        float timer = 0f;
        while (timer < invincibleTime)
        {
            if (playerRenderer != null)
                playerRenderer.enabled = !playerRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        if (playerRenderer != null)
            playerRenderer.enabled = true;

        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("💀 플레이어 사망!");
        gameObject.SetActive(false);
    }

    #endregion
}

using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    [Header("이동 관련 설정")]
    public float moveSpeed = 2f;             // 이동 속도
    public float chaseDistance = 10f;        // 추적 시작 거리
    public float attackDistance = 2f;        // 공격 거리

    [Header("체력 및 공격")]
    public float maxHealth = 100f;           // 최대 체력
    public float damage = 10f;               // 플레이어에게 줄 데미지
    private float currentHealth;             // 현재 체력

    [Header("애니메이터 연결")]
    public Animator animator;                // 직접 드래그해서 연결

    private Transform player;
    private NavMeshAgent agent;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
            agent.speed = moveSpeed;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < attackDistance)
        {
            // 공격 모션
            agent.isStopped = true;
            animator.SetBool("IsWalking", false);
            animator.SetTrigger("Attack");
        }
        else if (distance < chaseDistance)
        {
            // 이동 모션
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("IsWalking", true);
        }
        else
        {
            // Idle
            agent.isStopped = true;
            animator.SetBool("IsWalking", false);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hit");
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        agent.isStopped = true;
        Destroy(gameObject, 5f); // 5초 뒤에 제거
    }
}

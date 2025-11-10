using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public Transform player;              // 플레이어 Transform 직접 연결
    public float chaseDistance = 8f;      // 추적 시작 거리
    public float attackDistance = 2f;     // 공격 거리

    private NavMeshAgent agent;           // 👈 NavMeshAgent 참조 추가
    private Animator animator;            // 애니메이터 (있다면)

    // 👇 상태를 위한 enum 선언
    private enum SlimeAnimationState
    {
        Idle,
        Walk,
        Attack
    }

    private SlimeAnimationState currentState = SlimeAnimationState.Idle;  // 👈 현재 상태 변수 추가

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();   // NavMeshAgent 가져오기
        animator = GetComponent<Animator>();    // Animator 가져오기 (있으면)
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 플레이어가 가까이 오면 추적
        if (distanceToPlayer <= chaseDistance && distanceToPlayer > attackDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            currentState = SlimeAnimationState.Walk;
        }
        else if (distanceToPlayer <= attackDistance)
        {
            agent.isStopped = true;
            currentState = SlimeAnimationState.Attack;
        }
        else
        {
            // 플레이어 멀리 있으면 Idle
            agent.isStopped = true;
            currentState = SlimeAnimationState.Idle;
        }

        HandleAnimationState();
    }

    void HandleAnimationState()
    {
        // 애니메이션 처리 예시
        switch (currentState)
        {
            case SlimeAnimationState.Idle:
                animator.SetTrigger("Idle");
                break;
            case SlimeAnimationState.Walk:
                animator.SetTrigger("Walk");
                break;
            case SlimeAnimationState.Attack:
                animator.SetTrigger("Attack");
                break;
        }
    }
}

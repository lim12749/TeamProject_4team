using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [Header("Target")]
    public string playerTag = "Player";

    [Header("Detection (정면만 인식)")]
    public float detectionRadius = 8f;    // 플레이어가 이 반경 안으로 들어오면 체크
    [Range(0, 180)] public float viewAngle = 90f; // 정면 시야 각도(절반 기준)
    public LayerMask obstacleMask = ~0;   // 시야를 가리는 장애물 레이어 마스크

    [Header("Chase")]
    public float stopDistance = 1.5f;     // 플레이어에 접근했을 때 멈출 거리
    public float loseSightTime = 2.0f;    // 플레이어가 범위를 벗어난 후 얼마나 오래 추적할지

    [Header("Visualization")]
    public bool showGizmos = true;

    NavMeshAgent agent;
    Transform player;
    float lastSeenTime = Mathf.NegativeInfinity;
    bool isChasing => Time.time - lastSeenTime <= loseSightTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        FindPlayer();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        // 정면(콘) + 거리 + 장애물 체크로 플레이어 인식 판단
        if (CanSeePlayer())
        {
            lastSeenTime = Time.time;
        }

        if (isChasing)
        {
            Chase();
        }
        else
        {
            if (agent != null)
                agent.isStopped = true;
        }
    }

    void FindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) player = go.transform;
    }

    bool CanSeePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist > detectionRadius) return false;

        Vector3 dir = toPlayer.normalized;
        // 전방과의 각도 체크 (정면만 인식)
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        // 장애물(레이캐스트) 검사
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, detectionRadius, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == player || hit.collider.transform.IsChildOf(player))
                return true; // 플레이어가 보임
            return false; // 장애물에 가려짐
        }

        // 레이캐스트에 아무것도 맞지 않으면 플레이어가 범위 내에 있지만 레이어 마스크 때문에 못맞는 경우가 있으므로
        // 추가로 거리 내이면 인식하도록 허용하려면 true 반환 (현재는 false)
        return false;
    }

    void Chase()
    {
        if (agent == null || !agent.isOnNavMesh || player == null) return;

        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
        {
            agent.isStopped = true;
            // 도달 시 공격 로직 추가 가능
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 정면 콘 표시 (라인으로 양쪽 경계)
        Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + left * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * detectionRadius);
    }
}
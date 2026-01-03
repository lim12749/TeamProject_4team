using UnityEngine;
using UnityEngine.AI;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;        // 플레이어 Transform (인스펙터에서 할당 or 자동 할당)
    private NavMeshAgent agent;     // NavMeshAgent 컴포넌트

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 만약 인스펙터에서 플레이어를 안 넣었으면, 태그로 자동 탐색
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            // 플레이어 위치를 목적지로 설정
            agent.SetDestination(player.position);
        }
    }
}

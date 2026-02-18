using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public float spawnInterval = 2f;
    public int minMonsters = 5;
    public float spawnRadius = 40f;
    public float minDistance = 5f;

    public Transform player;

    // NavMesh에서 바닥 찾는 최대 거리
    public float navMeshSearchRadius = 10f;

    // ✅ 플레이어와 허용되는 높이 차이
    public float maxHeightDifference = 0.3f;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        InvokeRepeating(nameof(CheckAndSpawn), 0f, spawnInterval);
    }

    void CheckAndSpawn()
    {
        int count = GameObject.FindGameObjectsWithTag("Monster").Length;

        if (count < minMonsters)
        {
            int need = minMonsters - count;
            for (int i = 0; i < need; i++)
                SpawnNearPlayer();
        }
    }

    void SpawnNearPlayer()
    {
        if (player == null) return;

        for (int tries = 0; tries < 20; tries++)
        {
            Vector2 circle = Random.insideUnitCircle * spawnRadius;

            Vector3 candidate = new Vector3(
                player.position.x + circle.x,
                player.position.y + 5f,   // 위에서 NavMesh로 떨어뜨림
                player.position.z + circle.y
            );

            // 플레이어와 너무 가까우면 제외
            if (Vector3.Distance(player.position, candidate) < minDistance)
                continue;

            // ✅ NavMesh 위치 찾기
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
            {
                Vector3 spawnPos = hit.position;

                // 거리 체크
                if (Vector3.Distance(player.position, spawnPos) < minDistance)
                    continue;

                // ✅ 높이(Y) 차이 체크 (핵심!)
                float heightDiff = Mathf.Abs(spawnPos.y - player.position.y);
                if (heightDiff > maxHeightDifference)
                    continue;

                GameObject monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
                monster.tag = "Monster";
                return;
            }
        }

        Debug.LogWarning("SpawnNearPlayer: 조건에 맞는 NavMesh 위치를 찾지 못함");
    }
}

using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public float spawnInterval = 2f;
    public int minMonsters = 5;
    public float spawnRadius = 40f;        // 플레이어 주변 최대 소환 범위
    public float minDistance = 5f;         // 플레이어와의 최소 거리(안전 거리)

    public Transform player;

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

        Vector3 spawnPos;

        // ⭐ 플레이어와 너무 가까운 위치는 피해서 생성
        do
        {
            Vector2 circle = Random.insideUnitCircle * spawnRadius;

            spawnPos = new Vector3(
                player.position.x + circle.x,
                player.position.y,
                player.position.z + circle.y
            );

        } while (Vector3.Distance(player.position, spawnPos) < minDistance);
        // 가까우면 다시 랜덤 뽑기

        GameObject monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
        monster.tag = "Monster";
    }
}

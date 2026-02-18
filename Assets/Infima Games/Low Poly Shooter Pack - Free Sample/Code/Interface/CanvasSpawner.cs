using UnityEngine;

namespace InfimaGames.LowPolyShooterPack.Interface
{
    public class CanvasSpawner : MonoBehaviour
    {
        [Header("HUD Prefab (Infima)")]
        [SerializeField]
        private GameObject hudCanvasPrefab;

        private GameObject spawnedHud;

        private void Awake()
        {
            // 플레이어 생성 시 HUD 생성
            if (hudCanvasPrefab == null)
            {
                Debug.LogError("CanvasSpawner: hudCanvasPrefab이 Inspector에 연결되지 않았습니다.");
                return;
            }

            spawnedHud = Instantiate(hudCanvasPrefab);
        }

        private void OnDestroy()
        {
            // 플레이어 삭제 시 HUD도 같이 삭제
            if (spawnedHud != null)
                Destroy(spawnedHud);
        }
    }
}

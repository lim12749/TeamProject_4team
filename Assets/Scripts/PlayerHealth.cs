using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// 플레이어의 체력 및 데미지 처리 담당
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("플레이어 체력 설정")]
        public int maxHealth = 100;     // 최대 체력
        private int currentHealth;      // 현재 체력

        [Header("피격 시 효과")]
        public GameObject hitEffect;    // 피격 이펙트 (선택)
        public AudioClip hitSound;      // 피격 소리 (선택)
        public AudioClip deathSound;    // 사망 소리 (선택)

        private AudioSource audioSource;

        void Start()
        {
            currentHealth = maxHealth;
            audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// 몬스터가 공격할 때 호출됨
        /// </summary>
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            Debug.Log($"플레이어 피해! 현재 체력: {currentHealth}");

            // 피격 효과
            if (hitEffect != null)
                Instantiate(hitEffect, transform.position + Vector3.up, Quaternion.identity);

            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);

            // 체력 0 이하 → 사망 처리
            if (currentHealth <= 0)
                Die();
        }

        private void Die()
        {
            Debug.Log("💀 플레이어 사망");
            if (deathSound != null && audioSource != null)
                audioSource.PlayOneShot(deathSound);

            // 예: 리스폰, 게임오버, 애니메이션 등 구현 가능
            // 현재는 단순히 비활성화
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 체력 회복 함수 (필요 시 사용)
        /// </summary>
        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            Debug.Log($"체력 회복! 현재 체력: {currentHealth}");
        }
    }
}

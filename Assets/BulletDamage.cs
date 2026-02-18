using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public float damage = 20f; // 총알 데미지

    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트에 SimpleFollow 컴포넌트가 있는지 확인
        SimpleFollow enemy = collision.collider.GetComponent<SimpleFollow>();
        if (enemy != null)
        {
            enemy.TakeDamage(Mathf.RoundToInt(damage));  // float -> int 변환
        }

        // 총알은 충돌 시 제거
        Destroy(gameObject);
    }
}

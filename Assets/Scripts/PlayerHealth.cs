using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;

    public GameUI ui;
    bool isDead;

    void Start()
    {
        currentHP = maxHP;
        ui.UpdateHP(currentHP, maxHP);
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHP -= dmg;
        ui.UpdateHP(currentHP, maxHP);

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        GameManager.Instance.GameOver();
    }
}

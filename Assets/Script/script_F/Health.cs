using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log(gameObject.name + " took damage: " + damage);

        if (currentHealth <= 0)
        {
        }
    }

    //void Die()
    //{
    //    Debug.Log(gameObject.name + " died");
    //    //gameObject.SetActive(false); // or Destroy(gameObject);
    //}
}
using UnityEngine;
using System.Collections;

public class attack : MonoBehaviour
{
    public LayerMask mask;
    public float radius = 5;
    public Transform attackp;

    public int damage = 1;

    private void OnDrawGizmosSelected()
    {
        if (attackp == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackp.position, radius);
    }

    public void attacktouch()
    {
        if (attackp == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackp.position, radius, mask);

        foreach (Collider2D obj in hits)
        {
            // ✅ NORMAL ENEMY
            Health health = obj.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            // ✅ BOSS (works even if script is on parent)
            BossScript boss = obj.GetComponentInParent<BossScript>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }

            // ✅ FLASH EFFECT
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                StartCoroutine(FlashRed(sr));
            }
        }
    }

    IEnumerator FlashRed(SpriteRenderer sr)
    {
        Color originalColor = sr.color;

        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);

        if (sr != null)
            sr.color = originalColor;
    }

    private void OnDrawGizmos()
    {
        if (attackp == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackp.position, radius);
    }
}
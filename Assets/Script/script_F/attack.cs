using UnityEngine;

public class attack : MonoBehaviour
{
    public LayerMask mask;
    public float radius = 5;
    public Transform attackp;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackp.position, radius);
    }

    public void attacktouch()
    {
        if (attackp == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackp.position, radius, mask);

        //Debug.Log("Hit " + hits.Length + " objects!");

        foreach (Collider2D obj in hits)
        {
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.red;
            obj.gameObject.SetActive(false);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere (attackp.position, radius);
        Gizmos.color = Color.red;
    }
}
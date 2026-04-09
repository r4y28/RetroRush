using UnityEngine;

public class movesaw : MonoBehaviour
{
    [SerializeField] Transform[] point;
    [SerializeField] float speed = 1f;
    int count = 0;


    private void Update()
    {
        if (Vector3.Distance(transform.position,point[count].position) < 0.1f)
        {
            count++;
        }
        if (count >= point.Length)
        {
            count = 0;
        }
        transform.position = Vector3.MoveTowards(transform.position, point[count].position, speed * Time.deltaTime);
    }
}

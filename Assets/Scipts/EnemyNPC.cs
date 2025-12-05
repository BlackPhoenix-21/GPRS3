using UnityEngine;

public class EnemyNPC : MonoBehaviour
{
    [SerializeField] private float speed = 2f;

    void Start()
    {

    }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }
}

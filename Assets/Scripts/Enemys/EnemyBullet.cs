using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed;
    private Vector3 direction;
    void Start()
    {
        direction = PlayerController.Instance.transform.position - transform.position;
        direction.Normalize();
    }
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerHealthController.Instance.DamagePlayer();
        }
        Destroy(gameObject);
        AudioManager.Instance.PlaySFX(5);
    }
    private void OnBecameInvisible()
    {
        Destroy(gameObject);        
    }
}

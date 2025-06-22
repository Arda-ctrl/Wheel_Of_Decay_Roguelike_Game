using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed;
    private Vector3 direction;
    void Start()
    {
        direction = PlayerController.instance.transform.position - transform.position;
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
            PlayerHealthController.instance.DamagePlayer();
        }
        Destroy(gameObject);
        AudioManager.instance.PlaySFX(5);
    }
    private void OnBecameInvisible()
    {
        Destroy(gameObject);        
    }
}

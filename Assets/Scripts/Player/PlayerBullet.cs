using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] float speed = 7.5f;
    [SerializeField] Rigidbody2D theRB;
    public GameObject impactEffect;
    public int damageToGive = 50;
    void Update()
    {
        theRB.linearVelocity = transform.right * speed;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(gameObject);

        if (other.CompareTag("Enemy")) other.GetComponent<EnemyController>().DamageEnemy(damageToGive);
    }
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}

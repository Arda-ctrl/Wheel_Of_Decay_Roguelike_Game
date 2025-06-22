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

        AudioManager.instance.PlaySFX(4);

        if (other.CompareTag("Enemy")) other.GetComponent<EnemyController>().DamageEnemy(damageToGive);

        if (other.CompareTag("Box")) other.GetComponent<Breakables>().DropItem();
    }
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}

using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] float speed = 7.5f;
    [SerializeField] Rigidbody2D theRB;
    public GameObject impactEffect;
    void Update()
    {
        theRB.linearVelocity = transform.right * speed;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            return;
        Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}

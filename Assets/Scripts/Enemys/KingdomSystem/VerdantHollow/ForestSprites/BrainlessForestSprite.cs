using UnityEngine;
using System.Collections;

public class BrainlessForestSprite : VerdantEnemy
{
    [Header("Brainless Sprite Settings")]
    [SerializeField] private float crossFlightSpeed = 5f;
    [SerializeField] private float bounceForce = 3f;
    [SerializeField] private LayerMask wallLayerMask = -1;
    [SerializeField] private float directionChangeInterval = 2f;
    [SerializeField] private float contactDamage = 15f;
    [SerializeField] private float contactDamageRange = 0.8f;

    [Header("Flight Pattern")]
    [SerializeField] private bool useRandomDirection = false;
    [SerializeField] private float randomDirectionChangeChance = 0.3f;

    private Vector2 currentDirection;
    private float lastDirectionChangeTime;
    private bool isMoving = true;

    protected override void Start()
    {
        base.Start();
        
        // Initialize cross movement pattern
        SetRandomCrossDirection();
        lastDirectionChangeTime = Time.time;
        
        // Set sprite specific stats
        if (enemyData != null)
        {
            // Override base stats for brainless sprite behavior
            enemyData.baseSpeed = crossFlightSpeed;
            enemyData.detectionRange = 0f; // Doesn't actively seek player
            enemyData.attackRange = contactDamageRange;
            enemyData.attackCooldown = 0.5f;
        }
        
        // Disable gravity for flying behavior
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }
    }

    protected override void UpdateAI()
    {
        // Override base AI - brainless sprites don't follow normal behavior
        HandleCrossMovement();
        CheckForPlayerContact();
        
        // Periodically change direction
        if (Time.time >= lastDirectionChangeTime + directionChangeInterval)
        {
            if (useRandomDirection && Random.value < randomDirectionChangeChance)
            {
                SetRandomDirection();
            }
            else
            {
                SetRandomCrossDirection();
            }
            lastDirectionChangeTime = Time.time;
        }
    }

    private void HandleCrossMovement()
    {
        if (!isMoving) return;

        // Move in current direction
        if (rb != null)
        {
            rb.linearVelocity = currentDirection * crossFlightSpeed;
        }

        // Check for wall collisions and bounce
        CheckWallCollisions();
    }

    private void CheckWallCollisions()
    {
        // Raycast in movement direction to detect walls
        RaycastHit2D hit = Physics2D.Raycast(transform.position, currentDirection, 0.5f, wallLayerMask);
        
        if (hit.collider != null)
        {
            // Bounce off wall
            BounceOffWall(hit.normal);
        }
    }

    private void BounceOffWall(Vector2 wallNormal)
    {
        // Reflect the direction vector off the wall
        currentDirection = Vector2.Reflect(currentDirection, wallNormal);
        
        // Add some randomness to prevent getting stuck
        float randomAngle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        currentDirection = new Vector2(
            currentDirection.x * Mathf.Cos(randomAngle) - currentDirection.y * Mathf.Sin(randomAngle),
            currentDirection.x * Mathf.Sin(randomAngle) + currentDirection.y * Mathf.Cos(randomAngle)
        );
        
        currentDirection = currentDirection.normalized;
        
        // Apply bounce force
        if (rb != null)
        {
            rb.AddForce(currentDirection * bounceForce, ForceMode2D.Impulse);
        }
        
        Debug.Log($"🧚 {enemyData.enemyName} bounced off wall!");
    }

    private void SetRandomCrossDirection()
    {
        // Set direction to one of the four cardinal directions (cross pattern)
        int direction = Random.Range(0, 4);
        switch (direction)
        {
            case 0: currentDirection = Vector2.up; break;
            case 1: currentDirection = Vector2.down; break;
            case 2: currentDirection = Vector2.left; break;
            case 3: currentDirection = Vector2.right; break;
        }
    }

    private void SetRandomDirection()
    {
        // Set completely random direction
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        currentDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private void CheckForPlayerContact()
    {
        if (PlayerController.Instance == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        
        if (distanceToPlayer <= contactDamageRange)
        {
            DealContactDamage();
        }
    }

    private void DealContactDamage()
    {
        if (PlayerController.Instance != null && CanAttack())
        {
            var playerHealth = PlayerController.Instance.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.DamagePlayer();
                lastAttackTime = Time.time;
                
                Debug.Log($"🧚 {enemyData.enemyName} dealt contact damage to player!");
                
                // Play contact effect
                if (enemyData.attackSound != null)
                {
                    AudioSource.PlayClipAtPoint(enemyData.attackSound, transform.position);
                }
            }
        }
    }

    protected override void PerformAttack()
    {
        // Brainless sprites don't have traditional attacks, only contact damage
        DealContactDamage();
    }

    protected override void HandlePlayerInRange(float distanceToPlayer)
    {
        // Override - brainless sprites don't actively seek player
        // They just continue their cross movement pattern
    }

    protected override void HandlePlayerOutOfRange()
    {
        // Override - brainless sprites don't care about player range
        // They just continue their cross movement pattern
    }

    protected override void FlipSprite()
    {
        // Update sprite direction based on movement
        if (spriteRenderer != null)
        {
            isFacingRight = currentDirection.x > 0;
            spriteRenderer.flipX = !isFacingRight;
        }
    }

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator != null)
        {
            // Set flying animation parameters
            animator.SetBool("IsFlying", isMoving);
            animator.SetFloat("FlightSpeed", rb.linearVelocity.magnitude);
        }
    }

    // Brainless sprites can't be rooted (they fly)
    protected override void ApplyRootToPlayer()
    {
        // Only poison, no rooting for flying sprites
        if (canPoisonPlayer && Random.value < poisonChance)
        {
            ApplyPoisonToPlayer();
        }
    }

    protected override void OnVerdantDamaged(float damage)
    {
        // When damaged, change direction erratically
        SetRandomDirection();
        
        // Increase speed temporarily when hurt
        StartCoroutine(TemporarySpeedBoost());
    }

    private IEnumerator TemporarySpeedBoost()
    {
        float originalSpeed = crossFlightSpeed;
        crossFlightSpeed *= 1.5f;
        
        yield return new WaitForSeconds(2f);
        
        crossFlightSpeed = originalSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle wall collisions
        if (((1 << collision.gameObject.layer) & wallLayerMask) != 0)
        {
            BounceOffWall(collision.contacts[0].normal);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw contact damage range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, contactDamageRange);
        
        // Draw movement direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, currentDirection * 2f);
    }
}



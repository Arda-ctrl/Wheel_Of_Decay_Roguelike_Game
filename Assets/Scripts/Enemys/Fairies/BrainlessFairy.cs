using UnityEngine;
using System.Collections;

public class BrainlessFairy : BaseFairyController
{
    [Header("Brainless Fairy Settings")]
    [SerializeField] private float diagonalSpeed = 8f;
    [SerializeField] private bool preferDiagonalMovement = true;
    [SerializeField] private float wallBounceMultiplier = 1.2f;
    
    protected override void Start()
    {
        fairyType = FairyType.Brainless;
        
        // Set brainless fairy stats
        if (enemyData != null)
        {
            enemyData.maxHealth = 25f;
            enemyData.baseSpeed = diagonalSpeed;
            enemyData.baseDamage = contactDamage;
            enemyData.detectionRange = 0f; // No detection needed, just flies around
        }
        
        flySpeed = diagonalSpeed;
        contactDamage = 15f;
        canBounceOffWalls = true;
        
        base.Start();
        
        // Start with diagonal movement
        if (preferDiagonalMovement)
        {
            SetDiagonalDirection();
        }
        
        // Ensure the fairy starts moving immediately
        ChangeFairyState(FairyState.Flying);
        
        Debug.Log("Brainless Forest Fairy spawned - flying in diagonal patterns");
    }

    protected override void ChangeFlightDirection()
    {
        if (preferDiagonalMovement)
        {
            SetDiagonalDirection();
        }
        else
        {
            base.ChangeFlightDirection();
        }
    }

    private void SetDiagonalDirection()
    {
        // Choose one of the four diagonal directions
        int diagonalChoice = Random.Range(0, 4);
        
        switch (diagonalChoice)
        {
            case 0: // Up-Right
                currentFlyDirection = new Vector2(1f, 1f).normalized;
                break;
            case 1: // Up-Left
                currentFlyDirection = new Vector2(-1f, 1f).normalized;
                break;
            case 2: // Down-Right
                currentFlyDirection = new Vector2(1f, -1f).normalized;
                break;
            case 3: // Down-Left
                currentFlyDirection = new Vector2(-1f, -1f).normalized;
                break;
        }
        
        Debug.Log($"Brainless Fairy set diagonal direction: {currentFlyDirection}");
    }

    protected override void BounceOffWall(Vector2 wallNormal)
    {
        // Enhanced wall bouncing for brainless fairy
        base.BounceOffWall(wallNormal);
        
        // Apply additional bounce force
        if (rb != null)
        {
            rb.AddForce(currentFlyDirection * wallBounceForce * wallBounceMultiplier, ForceMode2D.Impulse);
        }
        
        // Ensure we maintain diagonal movement after bounce
        if (preferDiagonalMovement)
        {
            StartCoroutine(ReturnToDiagonalMovement());
        }
    }

    private IEnumerator ReturnToDiagonalMovement()
    {
        // Wait a bit after wall bounce, then return to diagonal movement
        yield return new WaitForSeconds(0.5f);
        
        SetDiagonalDirection();
    }

    protected override void HandleFlying()
    {
        // Brainless fairy just flies in straight diagonal lines
        base.HandleFlying();
        
        // Maintain consistent speed
        if (rb != null && rb.linearVelocity.magnitude < flySpeed * 0.8f)
        {
            rb.linearVelocity = currentFlyDirection * flySpeed;
        }
    }

    protected override void DealContactDamage(Collider2D player)
    {
        base.DealContactDamage(player);
        
        // Brainless fairy doesn't change behavior after hitting player
        // Just continues on its path
    }

    protected override void OnFairyDeath()
    {
        Debug.Log("Brainless Forest Fairy died");
        
        // Create simple death effect
        if (enemyData.deathEffect != null)
        {
            GameObject effect = Instantiate(enemyData.deathEffect, transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * 0.8f;
        }
        
        // Play fairy death sound
        if (enemyData.deathSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.deathSound, transform.position);
        }
    }

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator == null) return;
        
        // Brainless fairy only has Idle and Death animations
        // Movement is handled by physics, animation stays in Idle
        // No additional parameters needed
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw preferred diagonal directions
        if (preferDiagonalMovement)
        {
            Gizmos.color = Color.green;
            Vector3 pos = transform.position;
            
            // Draw all four diagonal directions
            Gizmos.DrawRay(pos, new Vector3(1f, 1f, 0f) * 2f);
            Gizmos.DrawRay(pos, new Vector3(-1f, 1f, 0f) * 2f);
            Gizmos.DrawRay(pos, new Vector3(1f, -1f, 0f) * 2f);
            Gizmos.DrawRay(pos, new Vector3(-1f, -1f, 0f) * 2f);
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StationaryRotkinsController : EnemyController
{
    [Header("Stationary Rotkins Settings")]
    [SerializeField] private KingdomEnemyData enemyData;
    [SerializeField] private GameObject rootSpikePrefab;
    [SerializeField] private float rootDamage = 35f;
    [SerializeField] private float mapWideRange = 15f;
    
    [Header("Root Attack Patterns")]
    [SerializeField] private int rootSpikesPerAttack = 3;
    [SerializeField] private float rootSpawnDelay = 0.5f;
    [SerializeField] private float rootWarningTime = 1f;
    [SerializeField] private float rootActiveTime = 2f;
    
    [Header("Attack Timings")]
    [SerializeField] private float lastRootAttackTime;
    [SerializeField] private float lastMultiRootAttackTime;
    [SerializeField] private float multiRootAttackCooldown = 8f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string rootAttackAnimationName = "RootAttack";
    [SerializeField] private string multiRootAnimationName = "MultiRootAttack";
    
    [Header("Visual Components")]
    [SerializeField] private GameObject rootWarningVFX; // Köklerin çıkacağı yeri gösteren warning
    [SerializeField] private GameObject rootAttackVFX; // Actual root spike effect
    [SerializeField] private Transform rootConnectionPoint; // Bu rotkins'ten köklerin çıktığı nokta
    
    private bool isPerformingRootAttack = false;
    private bool isPerformingMultiRootAttack = false;
    private List<Vector3> pendingRootPositions = new List<Vector3>();

    protected override void Start()
    {
        base.Start();
        
        // EnemyData'dan stats'ları al
        if (enemyData != null)
        {
            maxHealth = enemyData.maxHealth;
            baseSpeed = 0f; // Hareket etmiyor
            detectionRange = enemyData.detectionRange;
            stopDistance = 0f;
            currentHealth = maxHealth;
        }
        
        // Animator referansını al
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Bu rotkins hareket etmez - rigidbody'yi kinematic yap
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    protected override void Update()
    {
        // Movement'ı override et - stationary
        // Sadece status effects ve animation update
        UpdateStatusEffects();
        
        // Saldırı kontrolleri
        if (playerTransform != null && !isPerformingRootAttack && !isPerformingMultiRootAttack)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            // Multi-root pattern attack
            if (Time.time - lastMultiRootAttackTime >= multiRootAttackCooldown)
            {
                StartCoroutine(PerformMultiRootAttack());
            }
            // Single root attack at player position
            else if (distanceToPlayer <= mapWideRange && Time.time - lastRootAttackTime >= enemyData.attackCooldown)
            {
                StartCoroutine(PerformSingleRootAttack());
            }
        }
    }

    private void UpdateStatusEffects()
    {
        // Status effect update logic from base class
        List<StatusEffectType> expiredEffects = new List<StatusEffectType>();
        foreach (var effect in activeStatusEffects)
        {
            if (Time.time >= effect.Value)
            {
                expiredEffects.Add(effect.Key);
            }
        }

        foreach (var effect in expiredEffects)
        {
            RemoveStatus(effect);
        }
    }

    private IEnumerator PerformSingleRootAttack()
    {
        isPerformingRootAttack = true;
        lastRootAttackTime = Time.time;
        
        // Root attack animasyonu
        if (animator != null)
        {
            animator.Play(rootAttackAnimationName);
        }
        
        Debug.Log("Stationary Rotkins Single Root Attack!");
        
        if (playerTransform != null)
        {
            Vector3 targetPosition = playerTransform.position;
            
            // Root warning göster
            yield return StartCoroutine(ShowRootWarning(targetPosition));
            
            // Root spike spawn et
            yield return StartCoroutine(SpawnRootSpike(targetPosition));
        }
        
        isPerformingRootAttack = false;
        
        // İdle animasyonuna dön
        if (animator != null)
        {
            animator.Play(idleAnimationName);
        }
    }

    private IEnumerator PerformMultiRootAttack()
    {
        isPerformingMultiRootAttack = true;
        lastMultiRootAttackTime = Time.time;
        
        // Multi root attack animasyonu
        if (animator != null)
        {
            animator.Play(multiRootAnimationName);
        }
        
        Debug.Log("Stationary Rotkins Multi-Root Pattern Attack!");
        
        // Player etrafında random pozisyonlarda multiple root spikes
        if (playerTransform != null)
        {
            Vector3 playerPos = playerTransform.position;
            pendingRootPositions.Clear();
            
            // Player'ın etrafında rastgele pozisyonlar hesapla
            for (int i = 0; i < rootSpikesPerAttack; i++)
            {
                Vector3 randomOffset = Random.insideUnitCircle * 4f; // 4 birim radius
                Vector3 rootPosition = playerPos + randomOffset;
                pendingRootPositions.Add(rootPosition);
            }
            
            // Player'ın current pozisyonunu da ekle
            pendingRootPositions.Add(playerPos);
            
            // Tüm root warning'leri aynı anda göster
            List<Coroutine> warningCoroutines = new List<Coroutine>();
            foreach (var pos in pendingRootPositions)
            {
                warningCoroutines.Add(StartCoroutine(ShowRootWarning(pos)));
            }
            
            // Tüm warning'lerin bitmesini bekle
            foreach (var coroutine in warningCoroutines)
            {
                yield return coroutine;
            }
            
            // Root spike'ları sırayla spawn et
            foreach (var pos in pendingRootPositions)
            {
                StartCoroutine(SpawnRootSpike(pos));
                yield return new WaitForSeconds(rootSpawnDelay);
            }
        }
        
        yield return new WaitForSeconds(1f);
        
        isPerformingMultiRootAttack = false;
        
        // İdle animasyonuna dön
        if (animator != null)
        {
            animator.Play(idleAnimationName);
        }
    }

    private IEnumerator ShowRootWarning(Vector3 position)
    {
        // Warning VFX spawn et
        GameObject warning = null;
        if (rootWarningVFX != null)
        {
            warning = Instantiate(rootWarningVFX, position, Quaternion.identity);
        }
        
        // Warning süresi
        yield return new WaitForSeconds(rootWarningTime);
        
        // Warning'i kaldır
        if (warning != null)
        {
            Destroy(warning);
        }
    }

    private IEnumerator SpawnRootSpike(Vector3 position)
    {
        // Root spike spawn et
        GameObject rootSpike = null;
        if (rootSpikePrefab != null)
        {
            rootSpike = Instantiate(rootSpikePrefab, position, Quaternion.identity);
            
            // Root spike damage component'ini ayarla
            var damageComponent = rootSpike.GetComponent<RootSpikeDamage>();
            if (damageComponent != null)
            {
                damageComponent.Initialize(rootDamage, rootActiveTime);
            }
        }
        
        // Root attack VFX spawn et
        if (rootAttackVFX != null)
        {
            Instantiate(rootAttackVFX, position, Quaternion.identity);
        }
        
        // Bu rotkins'ten target pozisyonuna kök connection efekti
        if (rootConnectionPoint != null)
        {
            StartCoroutine(ShowRootConnection(rootConnectionPoint.position, position));
        }
        
        // Ses efekti çal
        if (enemyData?.attackSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.attackSound, position);
        }
        
        // Root spike'ın aktif kalma süresi
        yield return new WaitForSeconds(rootActiveTime);
        
        // Root spike'ı yok et
        if (rootSpike != null)
        {
            Destroy(rootSpike);
        }
    }

    private IEnumerator ShowRootConnection(Vector3 startPos, Vector3 endPos)
    {
        // Basit line renderer ile kök bağlantısı göster
        GameObject connectionLine = new GameObject("RootConnection");
        LineRenderer lineRenderer = connectionLine.AddComponent<LineRenderer>();
        
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;
        lineRenderer.sortingOrder = 5;
        
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        
        // 0.5 saniye göster sonra yok et
        yield return new WaitForSeconds(0.5f);
        
        Destroy(connectionLine);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Map-wide attack range'ini çiz
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, mapWideRange);
        
        // Root connection point'i çiz
        if (rootConnectionPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(rootConnectionPoint.position, 0.2f);
        }
    }

    public override void TakeDamage(float amount)
    {
        // Güçlü defans (stationary olduğu için)
        float finalDamage = amount;
        if (enemyData != null)
        {
            finalDamage /= enemyData.defenseMultiplier; // 1.3 defense
        }
        
        base.TakeDamage(finalDamage);
        
        // Hasar alma ses efekti
        if (enemyData?.hurtSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.hurtSound, transform.position);
        }
    }

    protected override void Die()
    {
        // Ölüm efekti
        if (enemyData?.deathEffect != null)
        {
            Instantiate(enemyData.deathEffect, transform.position, Quaternion.identity);
        }
        
        // Ölüm ses efekti
        if (enemyData?.deathSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.deathSound, transform.position);
        }
        
        base.Die();
    }
}

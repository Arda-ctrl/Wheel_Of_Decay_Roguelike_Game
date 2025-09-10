using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StationaryRotkinsTrapperController : EnemyController
{
    [Header("Stationary Rotkins Trapper Settings")]
    [SerializeField] private KingdomEnemyData enemyData;
    [SerializeField] private GameObject bindingRootPrefab;
    [SerializeField] private float rootDamage = 30f;
    [SerializeField] private float mapWideRange = 12f;
    
    [Header("Binding Mechanics")]
    [SerializeField] private float bindingDuration = 3f;
    [SerializeField] private float slowAmount = 0.7f; // 70% slow
    [SerializeField] private int bindingRootsPerTrap = 4;
    [SerializeField] private float bindingRadius = 2f;
    
    [Header("Attack Patterns")]
    [SerializeField] private int slowingRootsPerAttack = 2;
    [SerializeField] private float rootSpawnDelay = 0.8f;
    [SerializeField] private float rootWarningTime = 1.2f;
    [SerializeField] private float rootActiveTime = 4f;
    
    [Header("Attack Timings")]
    [SerializeField] private float lastBindingAttackTime;
    [SerializeField] private float lastSlowingAttackTime;
    [SerializeField] private float bindingAttackCooldown = 8f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string bindingAttackAnimationName = "BindingAttack";
    [SerializeField] private string slowingAttackAnimationName = "SlowingAttack";
    
    [Header("Visual Components")]
    [SerializeField] private GameObject bindingWarningVFX;
    [SerializeField] private GameObject slowingWarningVFX;
    [SerializeField] private GameObject bindingRootVFX;
    [SerializeField] private Transform rootConnectionPoint;
    
    private bool isPerformingBindingAttack = false;
    private bool isPerformingSlowingAttack = false;
    private GameObject currentPlayerBinding;

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
        UpdateStatusEffects();
        
        // Saldırı kontrolleri
        if (playerTransform != null && !isPerformingBindingAttack && !isPerformingSlowingAttack)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            // Binding trap attack (high priority)
            if (Time.time - lastBindingAttackTime >= bindingAttackCooldown && distanceToPlayer <= mapWideRange)
            {
                StartCoroutine(PerformBindingTrapAttack());
            }
            // Slowing root attack
            else if (distanceToPlayer <= mapWideRange && Time.time - lastSlowingAttackTime >= enemyData.attackCooldown)
            {
                StartCoroutine(PerformSlowingRootAttack());
            }
        }
    }

    private void UpdateStatusEffects()
    {
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

    private IEnumerator PerformBindingTrapAttack()
    {
        isPerformingBindingAttack = true;
        lastBindingAttackTime = Time.time;
        
        // Binding attack animasyonu
        if (animator != null)
        {
            animator.Play(bindingAttackAnimationName);
        }
        
        Debug.Log("Stationary Rotkins Trapper Binding Attack!");
        
        if (playerTransform != null)
        {
            Vector3 playerPosition = playerTransform.position;
            
            // Binding warning göster
            GameObject bindingWarning = null;
            if (bindingWarningVFX != null)
            {
                bindingWarning = Instantiate(bindingWarningVFX, playerPosition, Quaternion.identity);
            }
            
            // Warning süresi
            yield return new WaitForSeconds(rootWarningTime);
            
            // Warning'i kaldır
            if (bindingWarning != null)
            {
                Destroy(bindingWarning);
            }
            
            // Player'ı etrafa binding roots ile çevrele
            yield return StartCoroutine(CreateBindingTrap(playerPosition));
        }
        
        isPerformingBindingAttack = false;
        
        // İdle animasyonuna dön
        if (animator != null)
        {
            animator.Play(idleAnimationName);
        }
    }

    private IEnumerator CreateBindingTrap(Vector3 centerPosition)
    {
        List<GameObject> bindingRoots = new List<GameObject>();
        
        // Player'ın etrafında daire şeklinde binding roots spawn et
        for (int i = 0; i < bindingRootsPerTrap; i++)
        {
            float angle = (360f / bindingRootsPerTrap) * i;
            Vector3 rootPosition = centerPosition + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * bindingRadius,
                Mathf.Sin(angle * Mathf.Deg2Rad) * bindingRadius,
                0
            );
            
            GameObject bindingRoot = null;
            if (bindingRootPrefab != null)
            {
                bindingRoot = Instantiate(bindingRootPrefab, rootPosition, Quaternion.identity);
                bindingRoots.Add(bindingRoot);
                
                // Binding root component'ini ayarla
                var bindingComponent = bindingRoot.GetComponent<BindingRootTrap>();
                if (bindingComponent != null)
                {
                    bindingComponent.Initialize(rootDamage, bindingDuration, slowAmount);
                }
            }
            
            // VFX spawn et
            if (bindingRootVFX != null)
            {
                Instantiate(bindingRootVFX, rootPosition, Quaternion.identity);
            }
            
            // Root connection göster
            if (rootConnectionPoint != null)
            {
                StartCoroutine(ShowRootConnection(rootConnectionPoint.position, rootPosition, 1f));
            }
            
            yield return new WaitForSeconds(0.2f);
        }
        
        // Ses efekti çal
        if (enemyData?.attackSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.attackSound, centerPosition);
        }
        
        // Binding trap'in aktif kalma süresi
        yield return new WaitForSeconds(bindingDuration + 2f);
        
        // Binding roots'ları temizle
        foreach (var root in bindingRoots)
        {
            if (root != null)
            {
                Destroy(root);
            }
        }
    }

    private IEnumerator PerformSlowingRootAttack()
    {
        isPerformingSlowingAttack = true;
        lastSlowingAttackTime = Time.time;
        
        // Slowing attack animasyonu
        if (animator != null)
        {
            animator.Play(slowingAttackAnimationName);
        }
        
        Debug.Log("Stationary Rotkins Trapper Slowing Root Attack!");
        
        if (playerTransform != null)
        {
            Vector3 playerPos = playerTransform.position;
            
            // Player etrafında slowing roots spawn et
            for (int i = 0; i < slowingRootsPerAttack; i++)
            {
                Vector3 randomOffset = Random.insideUnitCircle * 3f;
                Vector3 rootPosition = playerPos + randomOffset;
                
                // Slowing warning göster
                yield return StartCoroutine(ShowSlowingWarning(rootPosition));
                
                // Slowing root spike spawn et
                yield return StartCoroutine(SpawnSlowingRoot(rootPosition));
                
                yield return new WaitForSeconds(rootSpawnDelay);
            }
        }
        
        isPerformingSlowingAttack = false;
        
        // İdle animasyonuna dön
        if (animator != null)
        {
            animator.Play(idleAnimationName);
        }
    }

    private IEnumerator ShowSlowingWarning(Vector3 position)
    {
        GameObject warning = null;
        if (slowingWarningVFX != null)
        {
            warning = Instantiate(slowingWarningVFX, position, Quaternion.identity);
        }
        
        yield return new WaitForSeconds(rootWarningTime * 0.7f); // Daha kısa warning
        
        if (warning != null)
        {
            Destroy(warning);
        }
    }

    private IEnumerator SpawnSlowingRoot(Vector3 position)
    {
        GameObject slowingRoot = null;
        if (bindingRootPrefab != null)
        {
            slowingRoot = Instantiate(bindingRootPrefab, position, Quaternion.identity);
            
            // Slowing root component'ini ayarla
            var slowingComponent = slowingRoot.GetComponent<BindingRootTrap>();
            if (slowingComponent != null)
            {
                slowingComponent.Initialize(rootDamage * 0.7f, 2f, slowAmount * 0.5f); // Daha hafif etki
            }
        }
        
        // Root connection göster
        if (rootConnectionPoint != null)
        {
            StartCoroutine(ShowRootConnection(rootConnectionPoint.position, position, 0.8f));
        }
        
        // Ses efekti çal
        if (enemyData?.attackSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.attackSound, position);
        }
        
        yield return new WaitForSeconds(rootActiveTime);
        
        if (slowingRoot != null)
        {
            Destroy(slowingRoot);
        }
    }

    private IEnumerator ShowRootConnection(Vector3 startPos, Vector3 endPos, float duration)
    {
        GameObject connectionLine = new GameObject("BindingRootConnection");
        LineRenderer lineRenderer = connectionLine.AddComponent<LineRenderer>();
        
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red; // Kırmızı = tehlikeli binding roots
        lineRenderer.endColor = Color.red;
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.08f;
        lineRenderer.positionCount = 2;
        lineRenderer.sortingOrder = 5;
        
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        
        yield return new WaitForSeconds(duration);
        
        Destroy(connectionLine);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Map-wide attack range'ini çiz
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, mapWideRange);
        
        // Binding radius'u çiz
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, bindingRadius);
        
        // Root connection point'i çiz
        if (rootConnectionPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(rootConnectionPoint.position, 0.2f);
        }
    }

    public override void TakeDamage(float amount)
    {
        // Orta seviye defans
        float finalDamage = amount;
        if (enemyData != null)
        {
            finalDamage /= enemyData.defenseMultiplier; // 1.2 defense
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
        // Aktif binding'leri temizle
        if (currentPlayerBinding != null)
        {
            Destroy(currentPlayerBinding);
        }
        
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

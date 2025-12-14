using UnityEngine;
using System.Collections;

public class MinotaurBoss : MonoBehaviour
{
    [Header("Basic Stat")]
    public float maxHp = 100f;
    public float currentHp;
    public float moveSpeed = 2f;
    public float runSpeed = 12f;
    public float detectionRange = 10f;
    public float meleeRange = 2.5f;

    [Header("Conditions")]
    public float dazedDuration = 3f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    [Header("Reference")]
    public Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spr;
    private Collider2D col;

    private bool isFacingRight = true;
    private bool isAttacking = false;
    private bool isDazed = false;
    private bool isRunning = false;
    private bool isDead = false;

    public bool isSuperArmor = false;

    private int platformLayerMask;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        currentHp = maxHp;

        platformLayerMask = LayerMask.GetMask("Platform");
    }

    void Start()
    {
        if (player == null && GameObject.FindGameObjectWithTag("Player") != null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        anim.Play("idle");
    }

    void Update()
    {
        if (isDead) return;

        // 돌진 중엔 Update 중단 (Idle 덮어쓰기 방지)
        if (isRunning) return;

        if (currentHp <= 0)
        {
            Die();
            return;
        }

        if (isDazed || isAttacking) return;

        if (player == null || HealthSystem.Instance.currentHealth <= 0)
        {
            SetIdle();
            return;
        }

        LookAtPlayer();

        if (Time.time < lastAttackTime + attackCooldown)
        {
            SetIdle();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= meleeRange)
        {
            StopMoving();
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

            if (playerRb != null && Mathf.Abs(playerRb.linearVelocity.y) > 0.5f)
                StartCoroutine(PerformPound());
            else
                StartCoroutine(PerformSwing());
        }
        else if (distance < detectionRange)
        {
            MoveToPlayer();
        }
        else
        {
            SetIdle();
        }
    }

    void SetIdle()
    {
        if (isDead) return;
        rb.linearVelocity = Vector2.zero;
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("idle"))
        {
            anim.Play("idle");
        }
    }

    void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
    }

    void MoveToPlayer()
    {
        if (isDead) return;
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("walk"))
        {
            anim.Play("walk");
        }

        Vector2 target = new Vector2(player.position.x, rb.position.y);
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    // ================= 패턴: 돌진 (Run) ================= //
    public void TriggerRun()
    {
        if (isDead || isDazed || isAttacking || isRunning) return;
        StartCoroutine(PerformRun());
    }

    IEnumerator PerformRun()
    {
        isRunning = true;
        isSuperArmor = true;

        anim.Play("run");

        float dir = isFacingRight ? 1 : -1;
        float timer = 0f;

        while (timer < 4.0f && isRunning && !isDead)
        {
            timer += Time.deltaTime;
            rb.linearVelocity = new Vector2(dir * runSpeed, rb.linearVelocity.y);

            // 가슴 높이에서 Raycast, 바닥은 무시하고 벽만 감지
            Vector2 rayOrigin = transform.position;
            rayOrigin.y += 1.0f; // y축을 1.0 올려서 바닥 감지 방지

            // platformLayerMask를 써서 벽(Platform)만 감지. 파이어볼/플레이어는 무시
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dir, 1.5f, platformLayerMask);

            if (hit.collider != null)
            {
                Debug.Log("벽 감지(Raycast)! 그로기");
                StartCoroutine(PerformStagger());
                yield break;
            }

            yield return null;
        }

        StopRun();
    }

    void StopRun()
    {
        if (isRunning && !isDead)
        {
            isRunning = false;
            isSuperArmor = false;
            rb.linearVelocity = Vector2.zero;
            anim.Play("idle");
            lastAttackTime = Time.time;
        }
    }

    // ================= 패턴: 그로기 ================= //
    IEnumerator PerformStagger()
    {
        if (isDazed || isDead) yield break;

        isDazed = true;
        isRunning = false;
        isSuperArmor = false;
        isAttacking = false;

        rb.linearVelocity = Vector2.zero;
        anim.Play("staggered");

        float bounceDir = isFacingRight ? -1 : 1;
        rb.AddForce(new Vector2(bounceDir * 5f, 5f), ForceMode2D.Impulse);

        spr.color = Color.gray;

        yield return new WaitForSeconds(0.5f);
        if (isDead) yield break;

        anim.Play("stagger-idle");
        yield return new WaitForSeconds(dazedDuration - 0.5f);

        if (isDead) yield break;

        spr.color = Color.white;
        anim.Play("idle");
        isDazed = false;
        lastAttackTime = Time.time;
    }

    // ================= 충돌 감지 ================= //
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead || isRunning) return;

        if (collision.CompareTag("Projectile") || collision.GetComponent<Projectile>() != null)
        {
            rb.linearVelocity = Vector2.zero;
            TriggerRun();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // 돌진 중 벽 충돌은 Raycast가 담당하므로 여기는 플레이어 처리만
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!isDazed && HealthSystem.Instance.currentHealth > 0)
            {
                HealthSystem.Instance.TakeDamage();

                Rigidbody2D prb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (prb)
                {
                    Vector2 dir = (collision.transform.position - transform.position).normalized;
                    prb.AddForce(dir * 5f + Vector2.up * 3f, ForceMode2D.Impulse);
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (isSuperArmor)
        {
            StartCoroutine(FlashRed());
            return;
        }

        if (isDazed) damage *= 2;
        currentHp -= damage;

        rb.linearVelocity = Vector2.zero;
        StartCoroutine(FlashRed());

        if (currentHp <= 0) Die();
    }

    IEnumerator FlashRed()
    {
        spr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (!isDazed && !isDead) spr.color = Color.white;
        else if (isDazed) spr.color = Color.gray;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;

        spr.color = Color.white;
        anim.Play("dying");
        yield return new WaitForSeconds(1.2f);
        anim.Play("dead");

        this.enabled = false;
    }

    // 공격 코루틴 (유지)
    IEnumerator PerformSwing() { isAttacking = true; rb.linearVelocity = Vector2.zero; anim.Play("swing"); yield return new WaitForSeconds(0.4f); CheckAttackHit(); yield return new WaitForSeconds(0.8f); anim.Play("idle"); isAttacking = false; lastAttackTime = Time.time; }
    IEnumerator PerformPound() { isAttacking = true; rb.linearVelocity = Vector2.zero; anim.Play("pound"); yield return new WaitForSeconds(0.5f); CheckAttackHit(); yield return new WaitForSeconds(1.0f); anim.Play("idle"); isAttacking = false; lastAttackTime = Time.time; }
    void CheckAttackHit() { if (HealthSystem.Instance.currentHealth <= 0) return; float dist = Vector2.Distance(transform.position, player.position); if (dist <= meleeRange + 0.8f) HealthSystem.Instance.TakeDamage(); }
    void LookAtPlayer() { if (isAttacking || isRunning || isDazed || isDead) return; if (player.position.x > transform.position.x && !isFacingRight) Flip(); else if (player.position.x < transform.position.x && isFacingRight) Flip(); }
    void Flip() { isFacingRight = !isFacingRight; Vector3 scale = transform.localScale; scale.x *= -1; transform.localScale = scale; }
}
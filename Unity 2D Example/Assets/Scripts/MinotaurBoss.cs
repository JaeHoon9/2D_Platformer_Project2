using UnityEngine;
using System.Collections;

public class MinotaurBoss : MonoBehaviour
{
    [Header("Basic Stat")]
    public float maxHp = 100f;
    public float currentHp;
    public float moveSpeed = 8f;
    public float runSpeed = 10f;
    public float detectionRange = 10f;
    public float meleeRange = 5f;

    [Header("Phase 2 Settings")]
    public float phaseTwoSpeedMultiplier = 1.5f;
    public GameObject shockwaveEffect;

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

    [Header("UI Reference")]
    public GameObject gameClearPanel;

    // 상태 변수들
    private bool isFacingRight = true;
    private bool isAttacking = false;
    private bool isDazed = false;
    private bool isRunning = false;
    private bool isDead = false;

    // 2페이즈 및 특수 기믹 변수
    private bool isPhaseTwo = false;
    private bool isPreparingSmash = false; // 카운터 가능 상태인지 확인
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
        if (isRunning) return; // 돌진 중엔 로직 중단

        // HP 체크 및 페이즈 전환
        if (currentHp <= 0)
        {
            Die();
            return;
        }

        // 페이즈 2 진입 체크 (HP 50% 이하)
        if (!isPhaseTwo && currentHp <= maxHp * 0.5f)
        {
            EnterPhaseTwo();
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

        DecideAction();
    }

    void EnterPhaseTwo()
    {
        isPhaseTwo = true;
        moveSpeed *= phaseTwoSpeedMultiplier; // 속도 증가
        spr.color = new Color(1f, 0.6f, 0.6f); // 붉은 기운 (시각적 피드백)
        Debug.Log("=== Phase 2 Start: Enraged! ===");
    }

    void DecideAction()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        bool isPlayerJumping = playerRb != null && Mathf.Abs(playerRb.linearVelocity.y) > 0.5f;

        // 1. 근접 범위 내 로직
        if (distance <= meleeRange)
        {
            StopMoving();

            if (isPhaseTwo)
            {
                // 2페이즈: 점프 중이면 발구르기, 아니면 콤보 공격
                if (isPlayerJumping)
                    StartCoroutine(PerformStomp());
                else
                    StartCoroutine(PerformSwingCombo());
            }
            else
            {
                // 1페이즈: 점프 중이면 후려치기(Pound), 아니면 휘두르기(Swing)
                if (isPlayerJumping)
                    StartCoroutine(PerformPound(false)); // false = 카운터 불가 (1페이즈는 그냥 공격)
                else
                    StartCoroutine(PerformSwing());
            }
        }
        // 2. 추격 범위 내 로직
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

        string moveAnim = isPhaseTwo ? "run" : "walk";
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(moveAnim))
        {
            anim.Play(moveAnim);
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
        // 2페이즈는 돌진 속도 빨라짐
        float currentRunSpeed = isPhaseTwo ? runSpeed * 1.3f : runSpeed;

        while (timer < 4.0f && isRunning && !isDead)
        {
            timer += Time.deltaTime;
            rb.linearVelocity = new Vector2(dir * currentRunSpeed, rb.linearVelocity.y);

            Vector2 rayOrigin = transform.position;
            rayOrigin.y += 1.0f;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dir, 1.5f, platformLayerMask);

            if (hit.collider != null)
            {
                Debug.Log("벽 충돌! 그로기");
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

    // ================= 패턴: 그로기 (Stagger) ================= //
    IEnumerator PerformStagger()
    {
        // 이미 그로기 상태이거나 죽었으면 실행 안 함
        if (isDead) yield break;

        // 상태 강제 초기화
        isDazed = true;
        isPreparingSmash = false;
        isRunning = false;
        isSuperArmor = false;
        isAttacking = false;

        // 움직임 멈춤
        rb.linearVelocity = Vector2.zero;
        anim.Play("staggered");

        // 튕겨나가는 연출
        float bounceDir = isFacingRight ? -1 : 1;
        rb.AddForce(new Vector2(bounceDir * 5f, 5f), ForceMode2D.Impulse);

        spr.color = Color.gray;

        // 충돌 직후 잠시 대기
        yield return new WaitForSeconds(0.5f);
        if (isDead) yield break;

        anim.Play("stagger-idle");

        // dazedDuration 시간만큼 대기
        yield return new WaitForSeconds(dazedDuration - 0.5f);

        if (isDead) yield break;

        // [상태 복구] 여기서 다시 깨어남
        spr.color = isPhaseTwo ? new Color(1f, 0.6f, 0.6f) : Color.white;
        anim.Play("idle");
        isDazed = false;
        lastAttackTime = Time.time;
    }

    // ================= 충돌 감지 및 피격 ================= //
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        // 1. 근접 공격(MeleeProjectile)일 경우
        if (collision.GetComponent<MeleeProjectile>() != null)
        {
            return;
        }

        // 2. 원거리 공격(파이어볼)일 경우
        if (collision.CompareTag("Projectile") || collision.GetComponent<Projectile>() != null)
        {
            if (isPhaseTwo && isPreparingSmash)
            {
                Debug.Log("카운터 성공! 보스 그로기 상태 진입.");
                StartCoroutine(PerformStagger());
                return;
            }

            // 일반적인 파이어볼 피격 반응 (멀리서 쏘는 것 응징)
            if (!isRunning && !isAttacking)
            {
                rb.linearVelocity = Vector2.zero;

                if (isPhaseTwo)
                {
                    // 2페이즈: 발구르기 -> 돌진 연계
                    StartCoroutine(PerformStompAndCharge());
                }
                else
                {
                    // 1페이즈: 그냥 돌진
                    TriggerRun();
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

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

        // 카운터 상황(isPreparingSmash)일 때는 데미지만 들어가고 로직은 OnTriggerEnter에서 처리

        if (isSuperArmor)
        {
            StartCoroutine(FlashRed());
            currentHp -= damage;
        }
        else
        {
            if (isDazed) damage *= 2; // 그로기 시 데미지 2배
            currentHp -= damage;
            StartCoroutine(FlashRed());
        }

        if (currentHp <= 0) Die();
    }

    IEnumerator FlashRed()
    {
        Color originalColor = isPhaseTwo ? new Color(1f, 0.6f, 0.6f) : Color.white;
        spr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (!isDead)
        {
            if (isDazed) spr.color = Color.gray;
            else spr.color = originalColor;
        }
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

        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(true);
        }

        this.enabled = false;
    }

    // ================= 공격 패턴 코루틴 ================= //

    // 1페이즈 기본 휘두르기
    IEnumerator PerformSwing()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim.Play("swing");
        yield return new WaitForSeconds(1.4f); // 공격 판정 타이밍
        CheckAttackHit(meleeRange + 0.8f);
        yield return new WaitForSeconds(0.8f); // 후딜레이
        anim.Play("idle");
        isAttacking = false;
        lastAttackTime = Time.time;
    }

    // 2페이즈: 휘두르기 콤보 (휘두르기 -> 후려치기)
    IEnumerator PerformSwingCombo()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        // 1타: 휘두르기
        anim.Play("swing");
        yield return new WaitForSeconds(1.4f);
        CheckAttackHit(meleeRange + 0.8f);
        yield return new WaitForSeconds(0.3f); // 콤보 간격

        if (isDazed || isDead) yield break; // 도중 캔슬 체크

        LookAtPlayer(); // 플레이어 방향 다시 봄

        yield return StartCoroutine(PerformPound(true));
    }

    // 후려치기 (Pound / Smash) - canCounter가 true면 카운터 가능 구간 발생
    IEnumerator PerformPound(bool canCounter)
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        anim.Play("pound");

        if (canCounter)
        {
            isPreparingSmash = true;
            Debug.Log("약점 노출! 파이어볼을 쏘세요!");
        }

        yield return new WaitForSeconds(0.8f);

        if (isDazed) yield break;

        isPreparingSmash = false;
        anim.Play("pound");

        yield return new WaitForSeconds(1.5f);
        CheckAttackHit(meleeRange + 1.5f);

        yield return new WaitForSeconds(1.0f); // 후딜레이

        anim.Play("idle");
        isAttacking = false;
        lastAttackTime = Time.time;
    }

    // 2페이즈: 발구르기 (Stomp)
    IEnumerator PerformStomp()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim.Play("stomp");

        yield return new WaitForSeconds(1.5f); // 발 드는 시간

        if (shockwaveEffect != null)
        {
            Instantiate(shockwaveEffect, new Vector3(transform.position.x, transform.position.y - 1f, 0), Quaternion.identity);
        }

        // 근처에 있으면 데미지 및 경직
        CheckAttackHit(meleeRange + 2.0f);

        yield return new WaitForSeconds(0.8f);
        anim.Play("idle");
        isAttacking = false;
        lastAttackTime = Time.time;
    }

    // 2페이즈 반응: 발구르기 후 돌진
    IEnumerator PerformStompAndCharge()
    {
        yield return StartCoroutine(PerformStomp());
        yield return new WaitForSeconds(0.2f);
        LookAtPlayer();
        TriggerRun();
    }

    void CheckAttackHit(float range)
    {
        if (HealthSystem.Instance.currentHealth <= 0) return;
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= range)
        {
            HealthSystem.Instance.TakeDamage();
        }
    }

    void LookAtPlayer()
    {
        if (isAttacking || isRunning || isDazed || isDead) return;
        if (player.position.x > transform.position.x && !isFacingRight) Flip();
        else if (player.position.x < transform.position.x && isFacingRight) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
using UnityEngine;
using UnityEngine.SceneManagement; 

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed;
    public float jumpPower;
    public float jumpCount;
    public float jumpHeight = 5f;
    public float moveSpeed = 8f;
    public bool flippedLeft;
    public bool facingRight;
    

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider2D;
    Animator anim;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDeath;
    public AudioClip audioFinish;
    AudioSource audioSource;

    [Header("근접 공격 설정")]
    public GameObject meleeProjectilePrefab;
    public float attackVerticalOffset = 0.5f;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                audioSource.Play();
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                audioSource.Play();
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                audioSource.Play();
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                audioSource.Play();
                break;
            case "DEATH":
                audioSource.clip = audioDeath;
                audioSource.Play();
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                audioSource.Play();
                break;
        }
    }

    private void Update()
    {
        
        if (Time.timeScale == 0) return;

        if (Input.GetButtonDown("Jump") && jumpCount < 2)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0);
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
            jumpCount++;
        }

        float h = Input.GetAxisRaw("Horizontal");
        rigid.linearVelocity = new Vector2(h * moveSpeed, rigid.linearVelocity.y);

        if (h < 0)
        {
            facingRight = false;
            Flip(false);
            anim.SetBool("isWalking", true);
        }
        else if (h > 0)
        {
            facingRight = true;
            Flip(true);
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            PerformMeleeAttack();
        }
    }

    void Flip(bool facingRight)
    {
        if(flippedLeft && facingRight)
        {
            transform.Rotate(0, -180, 0);
            flippedLeft = false;
        }
        if(!flippedLeft && !facingRight)
        {
            transform.Rotate(0, -180, 0);
            flippedLeft = true;
        }
    }

    void FixedUpdate()
    {
        
        if (Time.timeScale == 0)
        {
            rigid.linearVelocity = Vector2.zero; // ����
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");

        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if (rigid.linearVelocity.x > maxSpeed)
            rigid.linearVelocity = new Vector2(maxSpeed, rigid.linearVelocity.y);
        else if (rigid.linearVelocity.x < -maxSpeed)
            rigid.linearVelocity = new Vector2(-maxSpeed, rigid.linearVelocity.y);

        if (rigid.linearVelocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, Color.yellow);
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.5f)
                {
                    anim.SetBool("isJumping", false);
                    jumpCount = 0;
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) 
        {
            if (rigid.linearVelocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                
                OnAttack(collision.transform);
            }
            else
            {
                
                OnDamaged(collision.transform.position);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.CompareTag("Item"))
        {
            PlaySound("ITEM");
            
        }
        else if (collision.gameObject.CompareTag("Finish"))
        {
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NextStage();
            }
            PlaySound("FINISH");
        }
        else if (collision.gameObject.CompareTag("DeadZone")) 
        {
            if (HealthSystem.Instance != null)
            {
                HealthSystem.Instance.TakeDamage(); 
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayerReposition();
                }
            }
        }
    }

    void OnAttack(Transform enemy)
    {
        PlaySound("ATTACK");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddStagePoint(100); 
        }

        rigid.AddForce(Vector2.up * 8, ForceMode2D.Impulse);
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();

        enemyMove.OnDamaged(20, transform.position);

        if (enemyMove != null)
        {
            enemyMove.OnDamaged(20, transform.position);
        }

    }

    void OnDamaged(Vector2 targetPos)
    {
        
        if (gameObject.layer == LayerMask.NameToLayer("PlayerDamaged")) return;

        PlaySound("DAMAGED");

        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.TakeDamage(); 
        }

        gameObject.layer = LayerMask.NameToLayer("PlayerDamaged"); 
        spriteRenderer.color = new Color(1, 1, 1, 0.4f); 

        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);

        anim.SetTrigger("isDamaged");

        Invoke("OffDamaged", 1); 
    }

    void OffDamaged()
    {
        gameObject.layer = LayerMask.NameToLayer("Player"); 
        spriteRenderer.color = new Color(1, 1, 1, 1); 
    }

    /// 근접 공격 투사체를 발사합니다.
    void PerformMeleeAttack()
    {
        Vector3 spawnPosition = transform.position + new Vector3(0, attackVerticalOffset, 0);

        Instantiate(meleeProjectilePrefab, spawnPosition, Quaternion.identity);

    }

    public void OnDeath() 
    {
        PlaySound("DEATH");
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        spriteRenderer.flipY = true;
        capsuleCollider2D.enabled = false;
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

    }

    public void VelocityZero()
    {
        rigid.linearVelocity = Vector2.zero;
    }

}
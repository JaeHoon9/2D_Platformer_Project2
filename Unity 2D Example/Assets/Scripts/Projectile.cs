using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody2D projectileRb;
    public float speed;
    public int damageToDeal;
    public float projectileLife;
    public float projectileCount;

    public PlayerMove playerMove;
    public bool facingRight;

    void Start()
    {
        projectileCount = projectileLife;
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
            facingRight = playerMove.facingRight;
            if (!facingRight)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    void Update()
    {
        projectileCount -= Time.deltaTime;
        if (projectileCount <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (facingRight)
            projectileRb.linearVelocity = new Vector2(speed, projectileRb.linearVelocity.y);
        else
            projectileRb.linearVelocity = new Vector2(-speed, projectileRb.linearVelocity.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 부딪힌 게 "Trigger(시야 범위)"라면 그냥 무시하고 통과
        if (collision.isTrigger && !collision.CompareTag("Platform")) return;

        if (collision.CompareTag("Enemy"))
        {
            MinotaurBoss boss = collision.GetComponent<MinotaurBoss>();

            if (boss != null)
            {
                // 돌진 중이면 데미지 X, 파괴 O
                if (boss.isSuperArmor)
                {
                    Destroy(gameObject);
                    return;
                }
                boss.TakeDamage(damageToDeal);
                Destroy(gameObject);
            }
            else
            {
                EnemyMove enemy = collision.GetComponent<EnemyMove>();
                if (enemy != null) enemy.OnDamaged(damageToDeal, transform.position);
                Destroy(gameObject);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Platform") || collision.CompareTag("Platform"))
        {
            Destroy(gameObject);
        }
    }
}
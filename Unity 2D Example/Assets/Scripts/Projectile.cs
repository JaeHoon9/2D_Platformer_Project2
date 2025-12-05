using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody2D projectileRb;
    public float speed;
    public int damageToDeal; // ★ 여기에 데미지를 저장할 변수 추가

    public float projectileLife;
    public float projectileCount;

    public PlayerMove playerMove;
    public bool facingRight;

    void Start()
    {
        projectileCount = projectileLife;
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        facingRight = playerMove.facingRight;
        if (!facingRight)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
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
        {
            projectileRb.linearVelocity = new Vector2(speed, projectileRb.linearVelocity.y);
        }
        else
        {
            projectileRb.linearVelocity = new Vector2(-speed, projectileRb.linearVelocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyMove enemy = collision.gameObject.GetComponent<EnemyMove>();
            if (enemy != null)
            {
                // int damageAmount = 30; // ★ 기존의 고정값 30을 지우고
                Vector2 hitPosition = transform.position;
                // ★ public 변수인 damageToDeal을 사용
                enemy.OnDamaged(damageToDeal, hitPosition);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
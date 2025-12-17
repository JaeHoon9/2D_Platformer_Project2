using UnityEngine;
using System.Collections.Generic;

public class MeleeProjectile : MonoBehaviour
{
    [Header("조절 가능한 설정")]
    public float travelDistance = 1.5f;
    public float speed = 10f;
    public int damage = 1;
    public float manaToRestore = 5f;

    // 내부 변수
    private Rigidbody2D rb;
    private Vector2 startPosition;
    private Vector2 attackerPosition;
    private List<Collider2D> alreadyHit;

    private bool facingRight;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        alreadyHit = new List<Collider2D>();
    }

    void Start()
    {
        // 생성된 순간의 위치를 기록
        startPosition = transform.position;

        // 플레이어를 태그로 찾음
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
            Destroy(gameObject);
            return;
        }

        // 넉백을 위해 플레이어 위치 저장
        attackerPosition = player.transform.position;

        PlayerMove playerMove = player.GetComponent<PlayerMove>();

        if (playerMove != null)
        {
            facingRight = playerMove.facingRight;
        }
        else
        {
            Debug.LogError("PlayerMove 스크립트를 찾을 수 없습니다!");
            facingRight = true;
        }

        if (!facingRight)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    void FixedUpdate()
    {
        if (facingRight)
        {
            rb.linearVelocity = new Vector2(speed, 0);
        }
        else
        {
            rb.linearVelocity = new Vector2(-speed, 0);
        }
    }

    void Update()
    {
        if (Vector2.Distance(startPosition, transform.position) >= travelDistance)
        {
            Destroy(gameObject); // 최대 사거리에 도달하면 파괴
        }
    }

    // 트리거 충돌 감지
    void OnTriggerEnter2D(Collider2D other)
    {
        // 이미 맞은 대상이면 무시
        if (alreadyHit.Contains(other)) return;

        bool hitSuccessful = false;

        // 1. 일반 몬스터 체크 (EnemyMove)
        EnemyMove enemy = other.GetComponent<EnemyMove>();
        if (enemy != null)
        {
            enemy.OnDamaged(damage, attackerPosition);
            hitSuccessful = true;
        }

        // 2. 보스 몬스터 체크 (MinotaurBoss)
        MinotaurBoss boss = other.GetComponent<MinotaurBoss>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            hitSuccessful = true;
        }

        // 공격 성공 시 처리 (마나 회복 및 중복 피격 방지)
        if (hitSuccessful)
        {
            if (ManaSystem.Instance != null)
            {
                ManaSystem.Instance.RestoreMana(manaToRestore);
            }
            alreadyHit.Add(other);
        }
    }
}
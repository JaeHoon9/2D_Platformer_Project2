using UnityEngine;
using System.Collections.Generic;

public class MeleeProjectile : MonoBehaviour
{
    [Header("조절 가능한 설정")]
    public float travelDistance = 1.5f;
    public float speed = 10f;
    public int damage = 10;
    public float manaToRestore = 5f;

    // 내부 변수
    private Rigidbody2D rb;
    private Vector2 startPosition;
    private Vector2 attackerPosition;
    private List<Collider2D> alreadyHit;

    private bool facingRight; // ★ 1. 방향을 저장할 변수 추가

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        alreadyHit = new List<Collider2D>();
    }

    void Start()
    {
        // 생성된 순간의 위치를 기록
        startPosition = transform.position;

        // --- ★ 2. 'Projectile.cs'의 로직 적용 (수정됨) ---

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

        // ▼▼▼ 이 부분이 핵심 수정 사항입니다! ▼▼▼

        // 1. 플레이어의 SpriteRenderer 대신 PlayerMove 스크립트를 가져옵니다.
        PlayerMove playerMove = player.GetComponent<PlayerMove>();

        if (playerMove != null)
        {
            // 2. PlayerMove 스크립트의 'facingRight' 변수 값을 그대로 가져옵니다.
            facingRight = playerMove.facingRight;
        }
        else
        {
            // PlayerMove 스크립트를 찾지 못한 경우의 예비 로직
            Debug.LogError("PlayerMove 스크립트를 찾을 수 없습니다!");
            facingRight = true; // (기본값)
        }
        // ▲▲▲ 여기까지 수정 ▲▲▲


        // 왼쪽을 보고 있다면 투사체 자체를 180도 회전
        // (이 로직은 그대로 둡니다. facingRight가 올바르게 설정되었으므로 잘 작동합니다.)
        if (!facingRight)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        // --- ★ 로직 적용 끝 ---
    }

    // ★ 3. FixedUpdate 추가 (속도 처리를 위해)
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
        // 시작 위치로부터 설정한 travelDistance 이상 이동했는지 확인
        if (Vector2.Distance(startPosition, transform.position) >= travelDistance)
        {
            Destroy(gameObject); // 최대 사거리에 도달하면 파괴
        }
    }

    // 트리거 충돌 감지
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy") || alreadyHit.Contains(other))
        {
            return;
        }

        EnemyMove enemy = other.GetComponent<EnemyMove>();
        if (enemy != null)
        {
            // attackerPosition은 Start()에서 이미 설정됨
            enemy.OnDamaged(damage, attackerPosition);

            if (ManaSystem.Instance != null)
            {
                ManaSystem.Instance.RestoreMana(manaToRestore);
            }
            alreadyHit.Add(other);
        }
    }

    // ★ 4. SetAttackerPosition 함수는 이제 필요 없으므로 삭제합니다.
    // public void SetAttackerPosition(Vector2 pos) { ... } // <- 이 함수 삭제
}
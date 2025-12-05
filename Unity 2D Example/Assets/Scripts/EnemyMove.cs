using UnityEngine;
using System.Collections;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spr;
    CapsuleCollider2D cap;

    int nextMove;
    int hp;
    public int maxHp = 30;

    bool isInvincible;
    public float invincibleDur = 0.5f;

    bool isKnockback;
    public float knockbackForce = 3f;
    public float knockbackDur = 0.2f;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spr = GetComponent<SpriteRenderer>();
        cap = GetComponent<CapsuleCollider2D>();

        hp = maxHp;
        Invoke(nameof(Think), 5f);
    }

    void FixedUpdate()
    {
        if (isKnockback) return;

        rigid.linearVelocity = new Vector2(nextMove, rigid.linearVelocity.y);

        Vector2 front = new Vector2(rigid.position.x + nextMove * 0.5f, rigid.position.y);
        RaycastHit2D hit = Physics2D.Raycast(front, Vector3.down, 1f, LayerMask.GetMask("Platform"));
        if (hit.collider == null) Turn();
    }

    void Think()
    {
        nextMove = Random.Range(-1, 2);
        anim.SetInteger("WalkSpeed", nextMove);

        if (nextMove != 0) spr.flipX = nextMove == 1;
        Invoke(nameof(Think), Random.Range(2f, 5f));
    }

    void Turn()
    {
        nextMove = Random.Range(-1, 2);
        anim.SetInteger("WalkSpeed", nextMove);
        if (nextMove != 0) spr.flipX = nextMove == 1;

        CancelInvoke();
        Invoke(nameof(Think), Random.Range(2f, 5f));
    }

    public void OnDamaged(int dmg, Vector2 attackerPos)
    {
        if (isInvincible) return;

        hp -= dmg;
        StartCoroutine(Invincible());

        Vector2 dir = (transform.position.x - attackerPos.x > 0) ? Vector2.right : Vector2.left;
        StartCoroutine(Knockback(dir));

        if (hp <= 0) Die();
    }

    IEnumerator Invincible()
    {
        isInvincible = true;
        spr.color = new Color(1, 1, 1, 0.4f);
        yield return new WaitForSeconds(invincibleDur);
        spr.color = Color.white;
        isInvincible = false;
    }

    IEnumerator Knockback(Vector2 dir)
    {
        isKnockback = true;
        rigid.linearVelocity = Vector2.zero;
        rigid.AddForce((dir + Vector2.up) * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDur);
        isKnockback = false;
    }

    void Die()
    {
        spr.flipY = true;
        cap.enabled = false;
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        Invoke(nameof(Deactivate), 5f);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}

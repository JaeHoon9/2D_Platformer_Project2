using UnityEngine;

// 모든 아이템이 상속받을 기반 클래스
public abstract class Item : MonoBehaviour
{
    public string itemName = "Generic Item"; // 아이템 이름 (디버깅용)

    protected virtual void ApplyEffect(GameObject player)
    {
        Debug.Log($"{itemName} 효과 발동!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 태그 확인
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Item Triggered: {itemName}"); // 디버그 로그 추가
            ApplyEffect(other.gameObject); // 효과 적용
            Destroy(gameObject); // 아이템 사용 후 자신을 파괴
        }
    }

    // 아이템이 맵에 배치될 때 충돌 감지를 위해 Collider2D와 Rigidbody2D 필요
    // Collider2D: Is Trigger를 체크하여 통과할 수 있게 함
    // Rigidbody2D: Is Kinematic을 체크하여 물리적 움직임 없게 함 (트리거 감지에 필요)
    void Reset()
    {
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true; // 통과 가능하게 설정
            // collider.size = new Vector2(0.5f, 0.5f); // 아이템 이미지 크기에 맞춰 조절
            // collider.offset = new Vector2(0, 0); // 위치 조절
        }

        // Rigidbody2D 확인 및 추가
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // 물리적 움직임 없게 함 (Kinematic 설정)
            rb.gravityScale = 0; // 중력 영향 없게 함
        }
    }
}
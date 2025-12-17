using UnityEngine;

public class HealthPotion : Item // Item 클래스 상속
{
    public int healAmount = 1; // 회복할 하트 칸 수

    void Awake()
    {
        itemName = "하트 회복 물약"; // 아이템 이름 설정
    }

    protected override void ApplyEffect(GameObject player)
    {
        base.ApplyEffect(player); // 부모 클래스의 Debug.Log 호출

        // HealthSystem.Instance가 있는지 확인
        if (HealthSystem.Instance != null)
        {
            // 현재 체력이 최대 체력보다 적을 때만 회복
            if (HealthSystem.Instance.currentHealth < HealthSystem.Instance.maxHealth)
            {
                HealthSystem.Instance.HealHealth();
                Debug.Log($"하트 1칸 회복!");
            }
            else
            {
                Debug.Log("체력이 이미 가득 차 있습니다.");
            }
        }
        else
        {
            Debug.LogError("HealthSystem.Instance를 찾을 수 없습니다!");
        }
    }
}
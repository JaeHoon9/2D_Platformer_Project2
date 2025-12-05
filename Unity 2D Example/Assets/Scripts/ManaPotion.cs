using UnityEngine;

public class ManaPotion : Item // Item 클래스 상속
{
    public float manaRestoreAmount = 30f; // 회복할 마나 양

    void Awake()
    {
        itemName = "마나 물약"; // 아이템 이름 설정
    }

    protected override void ApplyEffect(GameObject player)
    {
        base.ApplyEffect(player); // 부모 클래스의 Debug.Log 호출

        // ManaSystem.Instance가 있는지 확인
        if (ManaSystem.Instance != null)
        {
            // 현재 마나가 최대 마나보다 적을 때만 회복
            if (ManaSystem.Instance.manaPoint < ManaSystem.Instance.maxManaPoint)
            {
                ManaSystem.Instance.RestoreMana(manaRestoreAmount);
                Debug.Log($"마나 {manaRestoreAmount} 회복!");
            }
            else
            {
                Debug.Log("마나가 이미 가득 차 있습니다.");
            }
        }
        else
        {
            Debug.LogError("ManaSystem.Instance를 찾을 수 없습니다! 플레이어에 ManaSystem 스크립트가 붙어있나요?");
        }
    }
}
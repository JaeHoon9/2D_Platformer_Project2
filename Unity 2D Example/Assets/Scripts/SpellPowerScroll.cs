using UnityEngine;

public class SpellPowerScroll : Item // Item 클래스 상속
{
    public float spellPowerIncreaseAmount = 5f; // 증가시킬 주문력 양

    void Awake()
    {
        itemName = "주문력 스크롤"; // 아이템 이름 설정
    }

    protected override void ApplyEffect(GameObject player)
    {
        base.ApplyEffect(player); // 부모 클래스의 Debug.Log 호출

        if (ManaSystem.Instance != null)
        {
            ManaSystem.Instance.IncreaseSpellPower(spellPowerIncreaseAmount);
            Debug.Log($"주문력 {spellPowerIncreaseAmount} 증가!");

            if (PlayerInfoManager.Instance != null && PlayerInfoManager.Instance.playerInfoPanel.activeSelf)
            {
                PlayerInfoManager.Instance.UpdatePlayerInfoUI();
            }
        }
        else
        {
            Debug.LogError("ManaSystem.Instance (or PlayerStats.Instance)를 찾을 수 없습니다! 플레이어에 ManaSystem 스크립트가 붙어있나요?");
        }
    }
}
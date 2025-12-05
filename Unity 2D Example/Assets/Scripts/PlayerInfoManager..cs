using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInfoManager : MonoBehaviour
{
    public static PlayerInfoManager Instance; // 싱글톤 인스턴스 추가

    public GameObject playerInfoPanel;
    public TextMeshProUGUI maxManaText;
    public TextMeshProUGUI spellPowerText;

    private bool isPanelOpen = false;

    // Awake에서 싱글톤 초기화
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 필요하다면 씬 전환 시 유지되도록
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (playerInfoPanel != null)
        {
            playerInfoPanel.SetActive(false);
        }
    }

    public void TogglePlayerInfoPanel()
    {
        isPanelOpen = !isPanelOpen;
        playerInfoPanel.SetActive(isPanelOpen);

        if (isPanelOpen)
        {
            UpdatePlayerInfoUI();
        }
    }

    public void UpdatePlayerInfoUI()
    {
        if (ManaSystem.Instance != null)
        {
            if (maxManaText != null)
            {
                maxManaText.text = $"{ManaSystem.Instance.maxManaPoint}";
            }
            if (spellPowerText != null)
            {
                spellPowerText.text = $"{ManaSystem.Instance.spellPower}";
            }
        }
        else
        {
            Debug.LogWarning("ManaSystem.Instance (or PlayerStats.Instance) not found in the scene.");
        }
    }
}
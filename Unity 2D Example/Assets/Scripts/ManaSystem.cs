using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class ManaSystem : MonoBehaviour
{
    public static ManaSystem Instance;

    // 마나 바 UI (Image 컴포넌트)
    public Image currentManaBarFill; // 마나 채움 부분 Image
    public TextMeshProUGUI manaText; // TextMeshPro Text UI 연결

    [Header("Mana Settings")]
    public float manaPoint = 100f; // 현재 마나
    public float maxManaPoint = 100f; // 최대 마나

    [Header("Player Stats")] // 플레이어 능력치 섹션 추가
    public float spellPower = 10f; // 주문력 (기본값 설정)

    // Regenerate Mana & Animation Speed
    public bool Regenerate = true;
    public float manaRegenRate = 0.1f;
    public float manaBarAnimationSpeed = 5f; // 애니메이션 속도 권장 (0.5보다 높게)

    public bool GodMode;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        manaPoint = maxManaPoint;
        UpdateManaUI(true);
    }

    void Update()
    {
        if (Regenerate && manaPoint < maxManaPoint)
        {
            RegenMana();
        }

        if (GodMode)
        {
            RestoreMana(maxManaPoint);
        }

        UpdateManaUI(false);
    }

    private void RegenMana()
    {
        manaPoint += manaRegenRate * Time.deltaTime;
        manaPoint = Mathf.Min(manaPoint, maxManaPoint);
    }

    private void UpdateManaUI(bool instant = false)
    {
        if (currentManaBarFill != null)
        {
            float targetFillAmount = manaPoint / maxManaPoint;

            if (instant)
            {
                currentManaBarFill.fillAmount = targetFillAmount;
            }
            else
            {
                currentManaBarFill.fillAmount = Mathf.Lerp(currentManaBarFill.fillAmount, targetFillAmount, Time.deltaTime * manaBarAnimationSpeed);
            }
        }
        else
        {
            Debug.LogWarning("currentManaBarFill Image is not assigned in ManaSystem!");
        }

        if (manaText != null)
        {
            manaText.text = $"{Mathf.RoundToInt(manaPoint)} / {Mathf.RoundToInt(maxManaPoint)}";
        }
        else
        {
            Debug.LogWarning("manaText (TextMeshProUGUI) is not assigned in ManaSystem!");
        }
    }

    public bool UseMana(float Mana)
    {
        if (manaPoint >= Mana)
        {
            manaPoint -= Mana;
            if (manaPoint < 0) manaPoint = 0;
            UpdateManaUI(false);
            Debug.Log($"마나 {Mana} 사용. 남은 마나: {manaPoint}");
            return true;
        }
        else
        {
            Debug.LogWarning($"마나 부족! {Mana} 필요하지만 현재 {manaPoint} 밖에 없습니다.");
            return false;
        }
    }

    public void RestoreMana(float Mana)
    {
        manaPoint += Mana;
        if (manaPoint > maxManaPoint) manaPoint = maxManaPoint;
        UpdateManaUI(false);
        Debug.Log($"마나 {Mana} 회복. 현재 마나: {manaPoint}");
    }

    public void SetMaxMana(float max)
    {
        maxManaPoint += (int)(maxManaPoint * max / 100);
        UpdateManaUI(true);
    }

    // 주문력 관련 함수
    public void IncreaseSpellPower(float amount)
    {
        spellPower += amount;
    }
}
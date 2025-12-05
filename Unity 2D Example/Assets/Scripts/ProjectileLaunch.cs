using UnityEngine;
using UnityEngine.UI;

public class ProjectileLaunch : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform launchPoint;

    public float shootTime;
    public float shootCounter;

    private PlayerMove player;
    private ManaSystem mana;

    [Header("Charging System")]
    public float fullChargeDuration = 2.0f;

    [Header("Normal Shot")]
    public int normalManaCost = 30;
    public float normalSpeed = 10f;
    public int normalDamage = 30;           // ★ 일반 샷 데미지 변수 추가

    [Header("Charged Shot")]
    public int chargedManaCost = 40;
    public float chargedSpeed = 15f;
    public float chargedScale = 3.0f;
    public int chargedDamage = 100;         // ★ 차징 샷 데미지 변수 추가 (원하는 값으로)

    public Slider chargingBar;
    public Vector3 chargeBarOffset;

    private bool isCharging = false;
    private float currentChargeTime = 0f;

    // ... (Start, Update, UpdateChargingBar, ResetCharging 함수는 수정 없음) ...

    void Start()
    {
        shootCounter = shootTime;
        player = GetComponent<PlayerMove>();
        mana = ManaSystem.Instance;

        if (chargingBar != null)
        {
            chargingBar.minValue = 0;
            chargingBar.maxValue = fullChargeDuration;
            chargingBar.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (shootCounter > 0)
        {
            shootCounter -= Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(1) && shootCounter <= 0 && mana.manaPoint >= normalManaCost)
        {
            isCharging = true;
            currentChargeTime = 0f;

            if (chargingBar != null)
            {
                chargingBar.gameObject.SetActive(true);
                UpdateChargingBar();
            }
        }

        if (isCharging && Input.GetMouseButton(1))
        {
            if (chargingBar != null)
            {
                UpdateChargingBar();
            }
        }

        if (Input.GetMouseButtonUp(1) && isCharging)
        {
            Launch();
        }

        if (isCharging && Input.GetKeyDown(KeyCode.Escape))
        {
            ResetCharging();
        }
    }

    void UpdateChargingBar()
    {
        currentChargeTime += Time.deltaTime;
        chargingBar.value = Mathf.Min(currentChargeTime, fullChargeDuration);
        Vector3 worldPos = transform.position + chargeBarOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        chargingBar.transform.position = screenPos;
    }

    // [★수정된 발사 함수★]
    void Launch()
    {
        GameObject newProjectile = Instantiate(projectilePrefab, launchPoint.position, Quaternion.identity);
        // 생성된 파이어볼의 Projectile 스크립트를 가져옴
        Projectile projectileScript = newProjectile.GetComponent<Projectile>();

        // 1. 차징샷
        if (currentChargeTime >= fullChargeDuration && mana.manaPoint >= chargedManaCost)
        {
            newProjectile.transform.localScale *= chargedScale;
            projectileScript.speed = chargedSpeed;
            projectileScript.damageToDeal = chargedDamage; // ★ 차징 데미지 전달

            mana.manaPoint -= chargedManaCost;
        }
        // 2. 일반샷
        else
        {
            projectileScript.speed = normalSpeed;
            projectileScript.damageToDeal = normalDamage; // ★ 일반 데미지 전달

            mana.manaPoint -= normalManaCost;
        }

        // 공통 로직
        shootCounter = shootTime;
        player.PlaySound("ATTACK");
        ResetCharging();
    }

    void ResetCharging()
    {
        isCharging = false;
        if (chargingBar != null)
        {
            chargingBar.gameObject.SetActive(false);
        }
    }
}
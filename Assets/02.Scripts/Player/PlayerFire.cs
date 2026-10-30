using UnityEngine;

[RequireComponent(typeof(PlayerStat))]
public class PlayerFire : MonoBehaviour
{
    [Header("발사 쿨다운")]
    public float FireTimer = 0f;

    [Header("투사체")]
    public GameObject BulletPrefab;

    [Header("공격 속도")]
    public bool IsAttackSpeedBuff;

    public bool IsAttackSpeedDebuff;

    [Header("관통")]
    public bool IsPierceBuff;

    [Header("치명타")]
    public bool IsCritBuff;

    private Player player;
    private PlayerStat stat;

    private void Awake()
    {
        player = GetComponent<Player>();
        stat = GetComponent<PlayerStat>();
    }

    public void Update()
    {
        // 기절 중에는 발사 불가
        if (player != null && player.IsStunned)
        {
            return;
        }

        bool attackSlowed = IsAttackSpeedDebuff || (player != null && player.IsAttackSlowed);
        float attackSpeed = (IsAttackSpeedBuff ? stat.AttackSpeedBuffRate : 1) *
                            (attackSlowed ? stat.AttackSpeedDebuffRate : 1) *
                            stat.AttackSpeedBonus;
        FireTimer += Time.deltaTime * attackSpeed;
        if (FireTimer >= stat.FireRate)
        {
            Fire();
            FireTimer -= stat.FireRate;
        }
    }

    public void Fire()
    {
        var normalMonsters = FindObjectsByType<NormalMonster>(FindObjectsSortMode.None);
        var dashMonsters = FindObjectsByType<DashMonster>(FindObjectsSortMode.None);
        var debuffMonsters = FindObjectsByType<DebuffMonster>(FindObjectsSortMode.None);

        float minDistance = float.MaxValue;
        var targetPos = Vector3.zero;

        foreach (var monster in normalMonsters)
        {
            if (Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                           Mathf.Pow(monster.transform.position.z - transform.position.z, 2)) < minDistance)
            {
                minDistance = Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                                         Mathf.Pow(monster.transform.position.z - transform.position.z, 2));
                targetPos = monster.transform.position;
            }
        }

        foreach (var monster in dashMonsters)
        {
            if (Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                           Mathf.Pow(monster.transform.position.z - transform.position.z, 2)) < minDistance)
            {
                minDistance = Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                                         Mathf.Pow(monster.transform.position.z - transform.position.z, 2));
                targetPos = monster.transform.position;
            }
        }

        foreach (var monster in debuffMonsters)
        {
            if (Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                           Mathf.Pow(monster.transform.position.z - transform.position.z, 2)) < minDistance)
            {
                minDistance = Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                                         Mathf.Pow(monster.transform.position.z - transform.position.z, 2));
                targetPos = monster.transform.position;
            }
        }

        Vector3 targetDirection = Vector3.zero;
        targetDirection = minDistance == float.MaxValue ? transform.forward : targetPos - transform.position;
        targetDirection.y = 0;
        targetDirection.Normalize();

        float critTrial = Random.Range(0f, 1f);

        GameObject gameObject = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        gameObject.GetComponent<PlayerBullet>().MoveSpeed = stat.BulletSpeed;
        gameObject.GetComponent<PlayerBullet>().Direction = targetDirection;
        gameObject.GetComponent<PlayerBullet>().LifeTime = stat.BulletLifeTime;
        gameObject.GetComponent<PlayerBullet>().Damage =
            stat.Damage * (IsCritBuff ? (stat.CritChance >= critTrial ? stat.CritMultiplier : 1) : 1);
        gameObject.GetComponent<PlayerBullet>().RemainPierce = IsPierceBuff ? stat.PierceCount : 1;
    }

    // 증강: 공격 속도 +rate
    public void UpgradeAttackSpeed(float rate)
    {
        stat.UpgradeAttackSpeed(rate);
    }

    // 증강: 공격력 +rate (기본값 대비 누적)
    public void UpgradeDamage(float rate)
    {
        stat.UpgradeDamage(rate);
    }

    public void ResetStats()
    {
        stat.ResetAttackBonus();
        FireTimer = 0f;
    }
}
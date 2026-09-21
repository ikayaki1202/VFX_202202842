using UnityEngine;

public enum FireMode { Single, Auto }

public class Usable_Firearm : MonoBehaviour, IUsable, IUsableInit
{
    [Header("발사 모드")]
    public FireMode Mode = FireMode.Single;
    public bool AllowModeSwitch = false;

    [Header("연사 설정 (Auto 모드일 때만 사용)")]
    public float FireRate = 0.12f;

    [Header("발사체 설정")]
    public Projectile_Bullet BulletPrefab;
    public float BulletSpeed = 60f;

    [Header("이펙트 연결")]
    public GameObjectEvent OnFired;
    public GameObjectEvent OnFireModeChanged;

    private Grabbable_Handheld handheld;
    private IPlayerGrabInput grabInput;
    private float nextFireTime;

    private void Awake()
    {
        handheld = GetComponent<Grabbable_Handheld>();
    }

    public void OnEquipped(GameObject user) => grabInput = user.GetComponentInChildren<IPlayerGrabInput>();
    public void OnUnequipped(GameObject user) => grabInput = null;

    private void Update()
    {
        if (AllowModeSwitch && grabInput != null && grabInput.SwitchFireModePressed)
        {
            Mode = Mode == FireMode.Single ? FireMode.Auto : FireMode.Single;
            OnFireModeChanged?.Invoke(gameObject);
        }

        if (Mode == FireMode.Auto && handheld.IsPressing && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + FireRate;
            Fire(gameObject);
        }
    }

    public void OnUsePressed(GameObject user)
    {
        if (Mode == FireMode.Single)
        {
            Fire(user);
        }
        else
        {
            nextFireTime = Time.time;
            Fire(user);
        }
    }

    public void OnUseReleased(GameObject user) { }

    private void Fire(GameObject user)
    {
        Vector3 origin = handheld.MuzzlePoint.position;
        Vector3 direction = handheld.MuzzlePoint.forward;

        if (BulletPrefab != null)
        {
            Projectile_Bullet bullet = Instantiate(BulletPrefab);
            bullet.Launch(user, origin, direction, BulletSpeed);
        }

        Debug.Log($"[Usable_Firearm] {gameObject.name} 발사! Mode={Mode}");
        OnFired?.Invoke(user);
    }
}
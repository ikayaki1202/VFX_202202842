using UnityEngine;
using System.Collections;

public class Usable_MeleeSwing : MonoBehaviour, IUsable
{
    [Header("타격 판정 연결")]
    public GrabbablePart_MeleeHitbox Hitbox;

    [Header("스윙 설정")]
    public float SwingDuration = 0.3f;
    public float SwingCooldown = 0.15f; // 스윙 끝난 뒤 다음 스윙까지 최소 간격

    [Header("이펙트 연결")]
    public GameObjectEvent OnSwingStarted;

    private Grabbable_Handheld handheld;
    private Coroutine swingRoutine;
    private float nextSwingTime;

    private void Awake()
    {
        handheld = GetComponent<Grabbable_Handheld>();
    }

    private void Update()
    {
        // 홀드 중이고, 스윙 중이 아니고, 쿨다운이 끝났으면 자동으로 다음 스윙 시작
        if (handheld.IsPressing && swingRoutine == null && Time.time >= nextSwingTime)
        {
            swingRoutine = StartCoroutine(SwingCoroutine(gameObject));
        }
    }

    public void OnUsePressed(GameObject user)
    {
        if (swingRoutine != null) return;
        swingRoutine = StartCoroutine(SwingCoroutine(user));
    }

    public void OnUseReleased(GameObject user) { }

    private IEnumerator SwingCoroutine(GameObject user)
    {
        OnSwingStarted?.Invoke(user);
        Hitbox?.Arm();

        yield return new WaitForSeconds(SwingDuration);

        Hitbox?.Disarm();
        swingRoutine = null;
        nextSwingTime = Time.time + SwingCooldown;
    }
}
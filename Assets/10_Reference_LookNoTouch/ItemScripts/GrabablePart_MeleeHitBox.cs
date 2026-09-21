using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class GrabbablePart_MeleeHitbox : MonoBehaviour
{
    public LayerMask HitMask = ~0;

    private bool isArmed;
    private readonly HashSet<Collider> hitThisSwing = new();

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public void Arm()
    {
        isArmed = true;
        hitThisSwing.Clear(); // 새 스윙 시작, 타격 기록 초기화
    }

    public void Disarm()
    {
        isArmed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isArmed) return;
        if (((1 << other.gameObject.layer) & HitMask) == 0) return;
        if (!hitThisSwing.Add(other)) return; // 이번 스윙에 이미 맞은 대상이면 무시

        // TODO: IDamageable 생기면 여기서 데미지 적용
        Debug.Log($"[{name}] 타격: {other.name}");
    }
}
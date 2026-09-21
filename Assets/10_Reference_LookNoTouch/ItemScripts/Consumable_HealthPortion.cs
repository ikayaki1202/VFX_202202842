using UnityEngine;

public class Consumable_HealthPotion : MonoBehaviour
{
    [Header("회복 설정")]
    public float HealAmount = 25f;

    // Usable_Single.OnUsed 이벤트에 이 메서드를 인스펙터에서 연결
    public void Consume(GameObject user)
    {
        // TODO: 체력 시스템(IDamageable) 생기면 여기서 회복 적용
        Debug.Log($"[Consumable_HealthPotion] {user.name}이(가) {gameObject.name} 마심, 회복량: {HealAmount}");

        // 마신 뒤엔 사라져야 하니, GrabHolder에서 분리 후 파괴
        Destroy(gameObject);
    }
}
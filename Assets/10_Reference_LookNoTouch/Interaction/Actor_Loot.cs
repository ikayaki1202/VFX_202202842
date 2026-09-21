using UnityEngine;

/// <summary>
/// 이 컴포넌트가 붙어있으면 "인벤토리로 편입 가능한 대상"이라는 뜻이면서,
/// 동시에 실제 루팅 실행(인벤토리 추가 + 자기 자신 정리)도 담당한다.
///
/// 두 가지 방식으로 호출될 수 있다:
///  1) 바로 루팅: Trigger_OnInteract.Interacted 이벤트에 Loot를 인스펙터로 연결 (클릭 한 번)
///  2) Grab 후 확인: Player_GrabBehavior가 HandlingObject에서 이 컴포넌트를 찾아 코드로 직접 호출
/// </summary>
public class Actor_Loot : MonoBehaviour
{
    public SO_ItemData ItemData;
    public int Quantity = 1;

    public void Loot(GameObject sender)
    {
        var inventory = sender.GetComponentInParent<Player_Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning($"[{nameof(Actor_Loot)}] {sender.name}에서 Player_Inventory를 찾을 수 없습니다.");
            return;
        }

        inventory.AddItem(ItemData, Quantity);
        Debug.Log($"[{nameof(Actor_Loot)}] {ItemData?.ItemName} x{Quantity} 획득!");

        gameObject.SetActive(false); // 필요하면 Destroy(gameObject)로 교체 가능
    }
}
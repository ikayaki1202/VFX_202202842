using UnityEngine;

public class Usable_Reload : MonoBehaviour, IUsable
{
    [Header("탄약 설정")]
    public int AmmoAmount = 30;

    [Header("이펙트 연결")]
    public GameObjectEvent OnReloaded;
    public GameObjectEvent OnReloadFailed; // 재장전할 무기가 없거나 이미 꽉 찬 경우

    public void OnUsePressed(GameObject user)
    {
        // 플레이어가 현재 들고 있는 "다른" 아이템을 찾아야 함
        var playerState = user.GetComponentInParent<Player_State>();
        if (playerState == null || playerState.HandlingObject == null)
        {
            OnReloadFailed?.Invoke(user);
            return;
        }

        // TODO: 총기류에 "재장전 가능"을 나타내는 인터페이스 필요 (아래 참고)
        Debug.Log($"[Usable_Reload] {gameObject.name}으로 재장전 시도, 양: {AmmoAmount}");
        OnReloaded?.Invoke(user);
    }

    public void OnUseReleased(GameObject user) { }
}
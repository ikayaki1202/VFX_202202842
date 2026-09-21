using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 특정 태그를 가진 오브젝트와 Trigger Collider가 충돌하면 이벤트를 발동한다.
/// Player_MoveBehavior의 사다리/절벽 감지와 별개로, "이 공간에 들어오면 뭔가 하고 싶다"는
/// 범용적인 트리거가 필요할 때 쓴다 (예: 함정, 체크포인트, 컷씬 시작 지점).
/// </summary>
public class Trigger_OnTriggerTag : MonoBehaviour
{
    [Tooltip("이 태그를 가진 오브젝트만 반응한다. 비워두면 태그 상관없이 전부 반응한다.")]
    public string RequiredTag;

    public GameObjectEvent OnEnter;
    public GameObjectEvent OnExit;

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(RequiredTag) && !other.CompareTag(RequiredTag)) return;
        OnEnter?.Invoke(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(RequiredTag) && !other.CompareTag(RequiredTag)) return;
        OnExit?.Invoke(other.gameObject);
    }
}
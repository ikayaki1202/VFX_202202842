using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 프리팹(이펙트 등)의 인스턴스를 생성하는 액터.
/// 트리거의 이벤트에 연결해서 쓴다. 목록에 넣은 프리팹은 한 번에 모두 생성된다.
/// - InstantiateHere(): 이 오브젝트의 위치에 생성 (예: Trigger_OnInteract에 연결)
/// - InstantiateAtObject(GameObject): 넘겨받은 오브젝트의 위치에 생성
///   (예: Trigger_OnTriggerTag.OnEnter에 Dynamic GameObject로 연결하면, 들어온 총알의 위치에 생성)
/// </summary>
public class Actor_Instantiate : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        [Tooltip("생성할 프리팹")]
        public GameObject prefab;

        [Tooltip("생성 후 이 시간(초)이 지나면 삭제한다. 0이면 삭제하지 않는다.")]
        [Min(0f)] public float lifetime = 2f;

        [Tooltip("켜면 위쪽(Y축)이 맞은 표면의 바깥 방향을 향한다. " +
                 "끄면 월드 기준(위가 위)으로 생성한다. 위로 타오르는 불꽃은 끈다.")]
        public bool alignToSurface = false;
    }

    [Tooltip("한 번에 함께 생성할 프리팹 목록")]
    public List<Entry> prefabs = new List<Entry>();

    [Header("표면에 생성")]
    [Tooltip("지정하면 생성 위치를 이 콜라이더의 표면으로 옮긴다. " +
             "총알 감지용 Trigger 콜라이더가 아니라 표적 본체의 콜라이더를 넣는다.")]
    public Collider surfaceCollider;

    [Tooltip("총알이 표적을 뚫고 지나간 뒤에 감지되어도, 총알이 날아온 방향으로 거슬러 올라가 " +
             "처음 들어온 표면(앞면)을 찾는다. 찾지 못하면 가장 가까운 표면 점을 쓴다.")]
    [Min(0f)] public float traceBackDistance = 5f;

    [Header("기타")]
    [Tooltip("켜면 생성된 인스턴스를 이 오브젝트의 자식으로 붙인다. 움직이는 표적에 쓴다.")]
    public bool attachToThis = false;

    /// <summary>이 오브젝트의 위치에 생성</summary>
    public void InstantiateHere()
    {
        InstantiateAt(transform.position, Vector3.up);
    }

    /// <summary>넘겨받은 오브젝트의 위치에 생성 (Dynamic GameObject로 연결)</summary>
    public void InstantiateAtObject(GameObject source)
    {
        if (source == null) return;

        Vector3 position = source.transform.position;
        Vector3 normal = Vector3.up;

        if (surfaceCollider != null)
        {
            if (!TryTraceEntryPoint(source, out position, out normal))
            {
                position = surfaceCollider.ClosestPoint(source.transform.position);
                normal = (position - surfaceCollider.bounds.center).normalized;
            }
        }

        InstantiateAt(position, normal);
    }

    /// <summary>지정한 위치에 생성 (스크립트에서 호출)</summary>
    public void InstantiateAtPosition(Vector3 position)
    {
        InstantiateAt(position, Vector3.up);
    }

    // 총알의 진행 방향을 거슬러 올라간 지점에서 진행 방향으로 레이를 쏴,
    // 표적 표면에 처음 닿는 점(앞면)과 그 면의 방향을 구한다.
    bool TryTraceEntryPoint(GameObject source, out Vector3 point, out Vector3 normal)
    {
        point = source.transform.position;
        normal = Vector3.up;

        Vector3 direction = GetTravelDirection(source);
        if (direction == Vector3.zero || traceBackDistance <= 0f) return false;

        Vector3 origin = source.transform.position - direction * traceBackDistance;
        Ray ray = new Ray(origin, direction);

        if (surfaceCollider.Raycast(ray, out RaycastHit hit, traceBackDistance * 2f))
        {
            point = hit.point;
            normal = hit.normal;
            return true;
        }
        return false;
    }

    // Rigidbody가 있으면 속도 방향, 없으면 오브젝트의 앞(Z축) 방향
    static Vector3 GetTravelDirection(GameObject source)
    {
        Rigidbody rb = source.GetComponent<Rigidbody>();
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.0001f)
            return rb.linearVelocity.normalized;

        return source.transform.forward;
    }

    void InstantiateAt(Vector3 position, Vector3 normal)
    {
        Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, normal);
        Transform parent = attachToThis ? transform : null;

        foreach (Entry e in prefabs)
        {
            if (e == null || e.prefab == null) continue;

            Quaternion rotation = e.alignToSurface ? surfaceRotation : Quaternion.identity;
            GameObject instance = Instantiate(e.prefab, position, rotation, parent);
            if (e.lifetime > 0f) Destroy(instance, e.lifetime);
        }
    }
}

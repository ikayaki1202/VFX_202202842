using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Usable_Beam : MonoBehaviour, IUsable
{
    [Header("빔 설정")]
    public float MaxDistance = 30f;
    public LayerMask HitMask = ~0;
    public float BeamWidth = 0.03f;

    [Header("이펙트 연결")]
    public GameObjectEvent OnBeamStarted;
    public GameObjectEvent OnBeamEnded;
    public GameObjectEvent OnBeamHit; // 신규: 빔이 매프레임 무언가에 맞을 때마다 발동

    private Grabbable_Handheld handheld;
    private LineRenderer beamRenderer;
    private bool isBeaming;

    private void Awake()
    {
        handheld = GetComponent<Grabbable_Handheld>();
        beamRenderer = GetComponent<LineRenderer>();

        beamRenderer.startWidth = BeamWidth;
        beamRenderer.endWidth = BeamWidth;
        beamRenderer.enabled = false; // 시작 시 항상 꺼진 상태 보장
    }

    public void OnUsePressed(GameObject user)
    {
        isBeaming = true;
        beamRenderer.enabled = true;
        OnBeamStarted?.Invoke(user);
    }

    public void OnUseReleased(GameObject user)
    {
        isBeaming = false;
        beamRenderer.enabled = false;
        OnBeamEnded?.Invoke(user);
    }

    private void Update()
    {
        if (!isBeaming) return;

        Vector3 origin = handheld.MuzzlePoint.position;
        Vector3 direction = handheld.MuzzlePoint.forward;
        Vector3 endPoint = origin + direction * MaxDistance;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, MaxDistance, HitMask))
        {
            endPoint = hit.point;

            Debug.Log($"[{name}] 빔 명중: {hit.collider.gameObject.name}");

            // TODO: hit.collider에서 IDamageable 찾아 초당 데미지(DPS) 적용
            OnBeamHit?.Invoke(hit.collider.gameObject);
        }

        beamRenderer.SetPosition(0, origin);
        beamRenderer.SetPosition(1, endPoint);
    }
}
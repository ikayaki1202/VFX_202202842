using UnityEngine;

public class Player_PointBehavior : MonoBehaviour
{
    private IPlayerPointInput input;
    private Player_Config playerConfig;
    private Player_State playerState;

    [Header("Settings")]
    public LayerMask InteractableLayer = ~0;
    public float MaxDistance = 20f;
    public float LineWidth = 0.01f;

    [Header("Visuals")]
    public GameObject HitPointMarker;
    private LineRenderer lineRenderer;

    private RaycastHit? currentHit;
    private IHoverable lastHover;
    private IInteractable activeInteractTarget; // OnInteractionStart를 받은 대상, End까지 유지

    private void Awake()
    {
        input = GetComponent<IPlayerPointInput>();
        playerConfig = GetComponentInParent<Player_Config>();
        playerState = GetComponentInParent<Player_State>();

        if (playerState != null)
            playerState.OnInteractionStateChanged += HandleInteractionStateChanged;
    }

    private void OnDestroy()
    {
        if (playerState != null)
            playerState.OnInteractionStateChanged -= HandleInteractionStateChanged;
    }

    private void Start()
    {
        if (input == null)
        {
            Debug.LogWarning($"[{gameObject.name}] IPlayerPointInput 컴포넌트를 찾을 수 없습니다.");
        }

        lineRenderer = playerConfig.PointingHand.GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = LineWidth;
            lineRenderer.endWidth = LineWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }

        if (HitPointMarker != null) HitPointMarker.SetActive(false);
    }

    private void HandleInteractionStateChanged(PlayerInteractionState newState)
    {
        if (newState != PlayerInteractionState.Idle)
        {
            CleanUpVisuals();
            enabled = false;
        }
        else
        {
            enabled = true;
        }
    }

    private void Update()
    {
        if (input == null) return;
        HandlePointing();
    }

    private void HandlePointing()
    {
        if (!input.IsPressing)
        {
            if (input.Released) CleanUpVisuals(); // 내부에서 activeInteractTarget에게 End를 보냄
            return;
        }

        Transform origin = playerConfig.PointingHand;
        Vector3 startPos = origin.position;
        Vector3 direction = origin.forward;

        currentHit = CastRay(startPos, direction);
        DrawLine(startPos, direction, currentHit);
        UpdateHover(currentHit);

        if (input.Pressed && currentHit.HasValue)
        {
            ExecuteInteractStart(currentHit.Value);
        }
    }

    private void UpdateHover(RaycastHit? hit)
    {
        if (!hit.HasValue)
        {
            if (lastHover != null)
            {
                lastHover.OnHoverExit(gameObject);
                lastHover = null;
            }
            if (HitPointMarker != null) HitPointMarker.SetActive(false);
            return;
        }

        IInteractable currentInteractable = null;
        IHoverable current = null;
        hit.Value.transform.TryGetComponent(out currentInteractable);
        hit.Value.transform.TryGetComponent(out current);

        if (current != lastHover)
        {
            current?.OnHoverEnter(gameObject);
            lastHover?.OnHoverExit(gameObject);
            lastHover = current;
        }

        if (HitPointMarker != null)
        {
            bool showPointer = currentInteractable != null;
            HitPointMarker.SetActive(showPointer);
            if (showPointer)
            {
                HitPointMarker.transform.position = hit.Value.point + hit.Value.normal * 0.01f;
                HitPointMarker.transform.rotation = Quaternion.LookRotation(hit.Value.normal);
            }
        }
    }

    private void ExecuteInteractStart(RaycastHit hit)
    {
        if (hit.transform.TryGetComponent<IInteractable>(out var interactable))
        {
            activeInteractTarget = interactable;
            activeInteractTarget.OnInteractionStart(gameObject);
        }
    }

    private void DrawLine(Vector3 start, Vector3 dir, RaycastHit? hit)
    {
        if (lineRenderer == null) return;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, hit.HasValue ? hit.Value.point : start + dir * MaxDistance);
    }

    public RaycastHit? GetRayHit()
    {
        Transform origin = playerConfig.PointingHand;
        return CastRay(origin.position, origin.forward);
    }

    private RaycastHit? CastRay(Vector3 start, Vector3 dir)
    {
        if (Physics.Raycast(start, dir, out RaycastHit hit, MaxDistance, InteractableLayer, QueryTriggerInteraction.Ignore))
            return hit;
        return null;
    }

    private void CleanUpVisuals()
    {
        currentHit = null;

        if (activeInteractTarget != null)
        {
            activeInteractTarget.OnInteractionEnd(gameObject);
            activeInteractTarget = null;
        }

        lastHover?.OnHoverExit(gameObject);
        lastHover = null;

        if (lineRenderer != null) lineRenderer.enabled = false;
        if (HitPointMarker != null) HitPointMarker.SetActive(false);
    }
}
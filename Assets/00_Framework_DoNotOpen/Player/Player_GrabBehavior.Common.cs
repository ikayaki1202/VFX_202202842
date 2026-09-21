using UnityEngine;

public partial class Player_GrabBehavior
{
    [Header("Visuals (원격 그랩 중 표시 - Point와 같은 오브젝트를 연결해야 함)")]
    public GameObject HitPointMarker;
    private LineRenderer lineRenderer;

    private IGrabbable heldGrabbable;
    private Rigidbody heldRigidbody;
    private Transform heldTransform;
    private Collider heldCollider;

    private Vector3 previousHandPosition;
    private Vector3 handVelocity;

    private void InitVisuals()
    {
        lineRenderer = playerConfig.PointingHand.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = playerConfig.PlatformConfigData.AimLineWidth;
            lineRenderer.endWidth = playerConfig.PlatformConfigData.AimLineWidth;
            lineRenderer.positionCount = 2;
        }

        if (HitPointMarker == null)
            Debug.LogWarning($"[{nameof(Player_GrabBehavior)}] HitPointMarker가 연결되지 않았습니다.");
    }

    private void InitHandVelocityTracking()
    {
        previousHandPosition = playerConfig.PointingHand.position;
    }

    private void TrackHandVelocity()
    {
        Vector3 currentPos = playerConfig.PointingHand.position;
        handVelocity = (currentPos - previousHandPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        previousHandPosition = currentPos;
    }

    // ---------- 공통 마무리 (Pull/Touch → 손에 장착) ----------

    private void FinalizeGrab(Transform target, IGrabbable grabbable, GrabSource source)
    {
        isFlyingToHand = false;
        currentSource = source;

        heldGrabbable = grabbable;
        heldTransform = target;

        target.SetParent(playerConfig.GrabHolder);
        target.localPosition = grabbable.HoldOffset;
        target.localRotation = Quaternion.identity;

        heldGrabbable.OnGrab(gameObject);

        playerState.SetHandlingObject(target.gameObject);
        playerState.SetInteractionState(PlayerInteractionState.Grabing);
    }

    // ---------- Drop ----------
    private void DoDrop()
    {
        if (playerState.HandlingObject == null) return;

        playerState.HandlingObject.transform.SetParent(null);
        heldGrabbable?.OnRelease(gameObject);

        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = false;

            var config = playerConfig.PlatformConfigData;
            Vector3 throwVelocity;

            if (config.UseAimedThrow)
            {
                // Desktop: 마우스 가속도 대신 조준 방향으로 고정 속도 — 정밀 조준 가능
                Vector3 aimDirection = playerConfig.PlayerCamera.forward;
                throwVelocity = aimDirection * config.AimedThrowSpeed;
            }
            else
            {
                // XR: 기존 방식 그대로 — 실제 팔 스윙 속도 반영
                float multiplier = heldGrabbable?.ThrowForceMultiplier ?? 1f;
                throwVelocity = Vector3.ClampMagnitude(handVelocity * multiplier, config.MaxThrowSpeed);
            }

            heldRigidbody.linearVelocity = throwVelocity;

            if (heldTransform.TryGetComponent<IThrowableHandler>(out var throwHandler))
            {
                throwHandler.OnThrown(gameObject, throwVelocity);
            }
        }

        ClearHeldState();
    }
    // private void DoDrop()
    // {
    //     if (playerState.HandlingObject == null) return;

    //     playerState.HandlingObject.transform.SetParent(null);
    //     heldGrabbable?.OnRelease(gameObject);

    //     if (heldRigidbody != null)
    //     {
    //         heldRigidbody.isKinematic = false;

    //         float multiplier = heldGrabbable?.ThrowForceMultiplier ?? 1f;
    //         Vector3 throwVelocity = Vector3.ClampMagnitude(handVelocity * multiplier, playerConfig.PlatformConfigData.MaxThrowSpeed);
    //         heldRigidbody.linearVelocity = throwVelocity;

    //         // 신규: 투척형 핸들러가 있으면 던짐 통지
    //         if (heldTransform.TryGetComponent<IThrowableHandler>(out var throwHandler))
    //         {
    //             throwHandler.OnThrown(gameObject, throwVelocity);
    //         }
    //     }

    //     ClearHeldState();
    // }

    private void ClearHeldState()
    {
        // 마우스를 누른 채로 Drop/취소/루팅된 경우, Update()의 Released 체크가
        // 이미 지나간 상태라 OnInteractionEnd가 누락될 수 있다. 여기서 보정한다.
        if (pointInput != null && pointInput.IsPressing && playerState.HandlingObject != null
            && playerState.HandlingObject.TryGetComponent<IInteractable>(out var stillHeld))
        {
            stillHeld.OnInteractionEnd(gameObject);
        }

        if (heldCollider != null && playerCharacterController != null)
        {
            Physics.IgnoreCollision(heldCollider, playerCharacterController, false);
        }

        heldGrabbable = null;
        heldRigidbody = null;
        heldTransform = null;
        heldCollider = null;

        currentSource = GrabSource.None;
        playerState.SetHandlingObject(null);
        playerState.SetInteractionState(PlayerInteractionState.Idle);
    }

    // ---------- Visuals ----------

    private void DrawGrabVisuals(Vector3 targetPoint, Vector3 normal)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, playerConfig.PointingHand.position);
            lineRenderer.SetPosition(1, targetPoint);
        }

        if (HitPointMarker != null)
        {
            HitPointMarker.SetActive(true);
            HitPointMarker.transform.position = targetPoint + normal * 0.01f;
            HitPointMarker.transform.rotation = Quaternion.LookRotation(normal);
        }
    }

    private void HideGrabVisuals()
    {
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (HitPointMarker != null) HitPointMarker.SetActive(false);
    }

    // ---------- Loot ----------

    private void TryLootHeldObject()
    {
        var held = playerState.HandlingObject;
        if (held == null) return;

        if (currentSource == GrabSource.Quickslot)
        {
            // 이미 인벤토리에 소유권이 있는 아이템 — 다시 루팅하지 않고 손에서만 내려놓음
            UnequipCurrent();
            return;
        }

        if (held.TryGetComponent<Actor_Loot>(out var lootable))
        {
            lootable.Loot(gameObject);
            ClearHeldState();
        }
    }
}
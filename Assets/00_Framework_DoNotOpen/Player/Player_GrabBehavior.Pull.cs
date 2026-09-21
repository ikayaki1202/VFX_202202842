using UnityEngine;

public partial class Player_GrabBehavior
{
    private bool isFlyingToHand;
    private Vector3 pullLocalHitOffset;
    private Vector3 pullLocalHitNormal;

    private void HandlePullInput()
    {
        if (isFlyingToHand)
        {
            UpdateFlight();

            if (input.DistancePullReleased)
            {
                AbortFlight();
            }
            return;
        }

        if (currentSource == GrabSource.Pull)
        {
            return;
        }

        bool mouseHeld = pointInput != null && pointInput.IsPressing;

        if (mouseHeld && input.DistancePullPressed && currentSource == GrabSource.None)
        {
            TryDistancePull();
        }
    }

    private void TryDistancePull()
    {
        Transform origin = playerConfig.PointingHand;

        if (!Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, playerConfig.PlatformConfigData.GrabMaxDistance, GrabbableLayer))
            return;

        if (!hit.transform.TryGetComponent<IGrabbable>(out var grabbable)) return;

        BeginFlight(hit.transform, grabbable, hit.point, hit.normal);
    }

    private void BeginFlight(Transform target, IGrabbable grabbable, Vector3 hitPoint, Vector3 hitNormal)
    {
        heldGrabbable = grabbable;
        heldTransform = target;
        pullLocalHitOffset = Quaternion.Inverse(target.rotation) * (hitPoint - target.position);
        pullLocalHitNormal = Quaternion.Inverse(target.rotation) * hitNormal;

        if (target.TryGetComponent<Rigidbody>(out heldRigidbody))
        {
            heldRigidbody.isKinematic = true;
        }

        if (target.TryGetComponent<Collider>(out heldCollider) && playerCharacterController != null)
        {
            Physics.IgnoreCollision(heldCollider, playerCharacterController, true);
        }

        isFlyingToHand = true;

        currentSource = GrabSource.Pull;
        playerState.SetHandlingObject(target.gameObject);
        playerState.SetInteractionState(PlayerInteractionState.Grabing);
    }

    private void UpdateFlight()
    {
        if (heldTransform == null)
        {
            isFlyingToHand = false;
            HideGrabVisuals();
            return;
        }

        Vector3 targetPos = GetTargetWorldPosition();
        heldTransform.position = Vector3.MoveTowards(heldTransform.position, targetPos, playerConfig.PlatformConfigData.PullFlySpeed * Time.deltaTime);

        Vector3 hitPointWorld = heldTransform.position + heldTransform.rotation * pullLocalHitOffset;
        Vector3 hitNormalWorld = heldTransform.rotation * pullLocalHitNormal;
        DrawGrabVisuals(hitPointWorld, hitNormalWorld);

        if (Vector3.Distance(heldTransform.position, targetPos) <= playerConfig.PlatformConfigData.PullArrivalThreshold)
        {
            HideGrabVisuals();
            FinalizeGrab(heldTransform, heldGrabbable, GrabSource.Pull);
        }
    }

    private void AbortFlight()
    {
        isFlyingToHand = false;
        HideGrabVisuals();
        if (heldRigidbody != null) heldRigidbody.isKinematic = false;
        ClearHeldState();
    }

    private Vector3 GetTargetWorldPosition()
    {
        Vector3 offset = heldGrabbable?.HoldOffset ?? Vector3.zero;

        return playerConfig.GrabHolder.position
             + playerConfig.GrabHolder.right * offset.x
             + playerConfig.GrabHolder.up * offset.y
             + playerConfig.GrabHolder.forward * offset.z;
    }
}
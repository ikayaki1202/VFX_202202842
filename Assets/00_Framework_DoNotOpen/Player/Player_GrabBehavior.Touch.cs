using UnityEngine;

public partial class Player_GrabBehavior
{
    [Header("Touch Grab")]
    public Player_GrabZone GrabZone;

    private Collider currentTouchCandidate;

    private void InitTouch()
    {
        if (GrabZone != null)
        {
            GrabZone.TouchEntered += HandleTouchEntered;
            GrabZone.TouchExited += HandleTouchExited;
        }
        else
        {
            Debug.LogWarning($"[{nameof(Player_GrabBehavior)}] GrabZone이 연결되지 않아 Touch Grab이 동작하지 않습니다.");
        }
    }

    private void TeardownTouch()
    {
        if (GrabZone != null)
        {
            GrabZone.TouchEntered -= HandleTouchEntered;
            GrabZone.TouchExited -= HandleTouchExited;
        }
    }

    private void HandleTouchInput()
    {
        if (input.TouchGrabPressed && currentTouchCandidate != null && currentSource == GrabSource.None && !isFlyingToHand)
        {
            TryTouchGrab(currentTouchCandidate);
        }
    }

    private void HandleTouchEntered(Collider other) => currentTouchCandidate = other;

    private void HandleTouchExited(Collider other)
    {
        if (currentTouchCandidate == other)
            currentTouchCandidate = null;
    }

    private void TryTouchGrab(Collider target)
    {
        if (!target.TryGetComponent<IGrabbable>(out var grabbable)) return;

        if (target.TryGetComponent<Rigidbody>(out heldRigidbody))
        {
            heldRigidbody.isKinematic = true;
        }

        if (target.TryGetComponent<Collider>(out heldCollider) && playerCharacterController != null)
        {
            Physics.IgnoreCollision(heldCollider, playerCharacterController, true);
        }

        FinalizeGrab(target.transform, grabbable, GrabSource.Touch);
    }
}
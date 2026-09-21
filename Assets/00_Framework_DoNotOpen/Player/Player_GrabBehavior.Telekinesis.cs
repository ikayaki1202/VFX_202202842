using UnityEngine;

public partial class Player_GrabBehavior
{
    private Transform telekinesisTarget;
    private Rigidbody telekinesisRigidbody;
    private Collider telekinesisCollider;
    private Vector3 telekinesisRelativePosition;
    private Quaternion telekinesisRelativeRotation;
    private Vector3 telekinesisLocalHitOffset;
    private Vector3 telekinesisLocalHitNormal;

    private void HandleTelekinesisInput()
    {
        if (currentSource == GrabSource.Telekinesis)
        {
            UpdateTelekinesis();

            if (input.DistanceGrabReleased)
            {
                EndTelekinesis();
            }
            return;
        }

        bool mouseHeld = pointInput != null && pointInput.IsPressing;

        if (mouseHeld && input.DistanceGrabPressed && currentSource == GrabSource.None)
        {
            TryBeginTelekinesis();
        }
    }

    private void TryBeginTelekinesis()
    {
        Transform origin = playerConfig.PointingHand;

        if (!Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, playerConfig.PlatformConfigData.GrabMaxDistance, GrabbableLayer))
            return;

        if (!hit.transform.TryGetComponent<IGrabbable>(out _)) return;

        telekinesisTarget = hit.transform;
        telekinesisRelativePosition = Quaternion.Inverse(origin.rotation) * (telekinesisTarget.position - origin.position);
        telekinesisRelativeRotation = Quaternion.Inverse(origin.rotation) * telekinesisTarget.rotation;
        telekinesisLocalHitOffset = Quaternion.Inverse(telekinesisTarget.rotation) * (hit.point - telekinesisTarget.position);
        telekinesisLocalHitNormal = Quaternion.Inverse(telekinesisTarget.rotation) * hit.normal;

        if (telekinesisTarget.TryGetComponent<Rigidbody>(out telekinesisRigidbody))
        {
            telekinesisRigidbody.isKinematic = true;
        }

        if (telekinesisTarget.TryGetComponent<Collider>(out telekinesisCollider) && playerCharacterController != null)
        {
            Physics.IgnoreCollision(telekinesisCollider, playerCharacterController, true);
        }

        currentSource = GrabSource.Telekinesis;
        playerState.SetHandlingObject(telekinesisTarget.gameObject);
        playerState.SetInteractionState(PlayerInteractionState.Grabing);
    }

    private void UpdateTelekinesis()
    {
        if (telekinesisTarget == null)
        {
            EndTelekinesis();
            return;
        }

        Transform origin = playerConfig.PointingHand;
        Vector3 targetPos = origin.position + origin.rotation * telekinesisRelativePosition;
        Quaternion targetRot = origin.rotation * telekinesisRelativeRotation;

        Vector3 nextPos;
        Quaternion nextRot;

        if (telekinesisRigidbody != null)
        {
            nextPos = Vector3.MoveTowards(telekinesisRigidbody.position, targetPos, playerConfig.PlatformConfigData.TelekinesisFollowSpeed * Time.deltaTime);
            nextRot = Quaternion.RotateTowards(telekinesisRigidbody.rotation, targetRot, playerConfig.PlatformConfigData.TelekinesisRotationSpeed * Time.deltaTime);
            telekinesisRigidbody.MovePosition(nextPos);
            telekinesisRigidbody.MoveRotation(nextRot);
        }
        else
        {
            nextPos = Vector3.MoveTowards(telekinesisTarget.position, targetPos, playerConfig.PlatformConfigData.TelekinesisFollowSpeed * Time.deltaTime);
            nextRot = Quaternion.RotateTowards(telekinesisTarget.rotation, targetRot, playerConfig.PlatformConfigData.TelekinesisRotationSpeed * Time.deltaTime);
            telekinesisTarget.position = nextPos;
            telekinesisTarget.rotation = nextRot;
        }

        Vector3 hitPointWorld = nextPos + nextRot * telekinesisLocalHitOffset;
        Vector3 hitNormalWorld = nextRot * telekinesisLocalHitNormal;
        DrawGrabVisuals(hitPointWorld, hitNormalWorld);
    }

    private void EndTelekinesis()
    {
        HideGrabVisuals();

        if (telekinesisCollider != null && playerCharacterController != null)
        {
            Physics.IgnoreCollision(telekinesisCollider, playerCharacterController, false);
        }

        if (telekinesisRigidbody != null)
        {
            telekinesisRigidbody.isKinematic = false;
        }

        telekinesisTarget = null;
        telekinesisRigidbody = null;
        telekinesisCollider = null;

        currentSource = GrabSource.None;
        playerState.SetHandlingObject(null);
        playerState.SetInteractionState(PlayerInteractionState.Idle);
    }
}
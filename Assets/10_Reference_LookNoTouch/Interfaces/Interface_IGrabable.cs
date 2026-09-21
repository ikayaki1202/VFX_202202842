using UnityEngine;

public interface IGrabbable
{
    Vector3 HoldOffset { get; }
    float ThrowForceMultiplier { get; }

    void OnGrab(GameObject sender);
    void OnRelease(GameObject sender);
}
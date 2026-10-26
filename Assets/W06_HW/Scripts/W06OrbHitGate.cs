using UnityEngine;

namespace W06Homework
{
    /// <summary>
    /// The larger trigger catches fast projectiles; only rays crossing the visible
    /// sphere continue to the Inspector-wired surface, burst and telemetry events.
    /// </summary>
    public sealed class W06OrbHitGate : MonoBehaviour
    {
        public Collider surfaceCollider;
        public GameObjectEvent OnConfirmedHit = new GameObjectEvent();
        [Min(.1f)] public float traceBackDistance = 5f;

        public void TryHit(GameObject source)
        {
            if (!source || !surfaceCollider) return;
            var body = source.GetComponent<Rigidbody>();
            Vector3 direction = body && body.linearVelocity.sqrMagnitude > .0001f
                ? body.linearVelocity.normalized : source.transform.forward;
            if (direction.sqrMagnitude < .0001f) return;

            var ray = new Ray(source.transform.position - direction * traceBackDistance, direction);
            if (!surfaceCollider.Raycast(ray, out _, traceBackDistance * 2f)) return;
            OnConfirmedHit?.Invoke(source);
        }
    }
}

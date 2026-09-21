using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Grabbable_Grenade : MonoBehaviour, IGrabbable, IThrowableHandler
{
    [Header("Grab Settings")]
    [SerializeField] private Vector3 holdOffset;
    [SerializeField] private float throwForceMultiplier = 1.2f;

    [Header("폭발 설정")]
    public float ExplosionRadius = 5f;

    [Header("이펙트 연결")]
    public GameObjectEvent OnExploded;

    private bool isThrown;

    public Vector3 HoldOffset => holdOffset;
    public float ThrowForceMultiplier => throwForceMultiplier;

    public void OnGrab(GameObject sender) => isThrown = false;
    public void OnRelease(GameObject sender) { }

    public void OnThrown(GameObject thrower, Vector3 velocity)
    {
        isThrown = true; // 손에 쥔 채 몸에 부딪혀도 안 터지게, 던진 뒤부터만 반응
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isThrown) return;
        Explode();
    }

    private void Explode()
    {
        // TODO: ExplosionRadius 내 IDamageable에게 데미지
        Debug.Log($"[{name}] 폭발! 반경: {ExplosionRadius}");
        OnExploded?.Invoke(gameObject);
        Destroy(gameObject);
    }
}
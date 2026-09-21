using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Projectile_Bullet : MonoBehaviour
{
    [Header("데미지 설정")]
    public float Damage = 10f;

    [Header("수명/정리")]
    public float MaxLifetime = 5f;
    public float MaxDistance = 100f;

    [Header("이펙트 연결")]
    public GameObjectEvent OnHit;

    private Vector3 startPosition;
    private float spawnTime;
    private GameObject shooter;

    private void Awake()
    {
        // 고속 이동 시 관통(터널링) 방지
        GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void Launch(GameObject sender, Vector3 origin, Vector3 direction, float speed)
    {
        shooter = sender;
        startPosition = origin;
        spawnTime = Time.time;

        transform.position = origin;
        transform.rotation = Quaternion.LookRotation(direction);

        GetComponent<Rigidbody>().linearVelocity = direction.normalized * speed;
    }

    private void Update()
    {
        if (Time.time - spawnTime >= MaxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (Vector3.Distance(startPosition, transform.position) >= MaxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 발사자 본인과의 충돌 무시 (Physics.IgnoreCollision과 별개의 안전장치)
        if (collision.gameObject == shooter) return;

        // TODO: collision.gameObject에서 IDamageable 찾아 Damage 적용
        Debug.Log($"[{name}] 명중: {collision.gameObject.name}");

        OnHit?.Invoke(collision.gameObject);
        Destroy(gameObject);
    }
}
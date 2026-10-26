using UnityEngine;

namespace W06Homework
{
    public sealed class W06TargetTelemetry : MonoBehaviour
    {
        public string materialName;
        public int hitCount;
        public void RegisterHit(GameObject projectile)
        {
            hitCount++;
            Debug.Log("[W06_HIT] " + materialName + " #" + hitCount + " projectile=" + projectile.name);
        }
    }
}

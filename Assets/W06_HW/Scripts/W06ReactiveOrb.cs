using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace W06Homework
{
    public enum OrbKind { Glass, Water }

    /// <summary>Surface response connected to Trigger_OnTriggerTag.OnEnter in the Inspector.</summary>
    public sealed class W06ReactiveOrb : MonoBehaviour
    {
        public OrbKind kind;
        public Renderer surfaceRenderer;
        public Material crackMaterial;
        [Range(1, 8)] public int maxCrackImpacts = 5;
        [Range(.001f, .012f)] public float crackWidth = .0048f;
        [Range(.2f, 2f)] public float impactStrength = 1f;
        public int hitCount { get; private set; }

        readonly Queue<GameObject> marks = new Queue<GameObject>();
        MaterialPropertyBlock properties;
        Material ownedMaterial;
        static readonly int ImpactTime = Shader.PropertyToID("_ImpactTime");
        static readonly int ImpactPoint = Shader.PropertyToID("_ImpactPoint");
        static readonly int ImpactStrength = Shader.PropertyToID("_ImpactStrength");

        void Awake()
        {
            if (!surfaceRenderer) surfaceRenderer = GetComponent<Renderer>();
            properties = new MaterialPropertyBlock();
            ResetSurface();
        }

        public void ReceiveHit(GameObject projectile)
        {
            if (!projectile) return;
            var collider = GetComponent<Collider>();
            Vector3 point = projectile.transform.position;
            var body = projectile.GetComponent<Rigidbody>();
            Vector3 direction = body && body.linearVelocity.sqrMagnitude > .001f
                ? body.linearVelocity.normalized : projectile.transform.forward;
            if (collider && collider.Raycast(new Ray(point - direction * 5f, direction), out var hit, 10f))
                point = hit.point;
            else if (collider) point = collider.ClosestPoint(point);
            Vector3 local = transform.InverseTransformPoint(point).normalized;
            if (local.sqrMagnitude < .5f) local = -transform.InverseTransformDirection(direction).normalized;

            hitCount++;
            if (kind == OrbKind.Glass) AddCrack(local);
            if (surfaceRenderer)
            {
                if (properties == null) properties = new MaterialPropertyBlock();
                surfaceRenderer.GetPropertyBlock(properties);
                properties.SetFloat(ImpactTime, Time.time);
                properties.SetVector(ImpactPoint, new Vector4(local.x, local.y, local.z, 0));
                properties.SetFloat(ImpactStrength, impactStrength);
                surfaceRenderer.SetPropertyBlock(properties);
            }
            Debug.Log("[W06_ORB] " + kind + " hit=" + hitCount + " surface=" + point.ToString("F3"));
        }

        public void ResetSurface()
        {
            while (marks.Count > 0) { var mark = marks.Dequeue(); if (mark) Destroy(mark); }
            hitCount = 0;
            if (!surfaceRenderer) return;
            if (properties == null) properties = new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(properties);
            properties.SetFloat(ImpactTime, -1000f);
            properties.SetFloat(ImpactStrength, 0);
            surfaceRenderer.SetPropertyBlock(properties);
        }

        void AddCrack(Vector3 origin)
        {
            if (!crackMaterial)
            {
                var shader = Shader.Find("W06/CrackSurface");
                if (!shader) { Debug.LogError("W06/CrackSurface shader is missing.", this); return; }
                ownedMaterial = ownedMaterial ? ownedMaterial : new Material(shader);
                crackMaterial = ownedMaterial;
            }
            while (marks.Count >= maxCrackImpacts) { var oldest = marks.Dequeue(); if (oldest) Destroy(oldest); }
            var mark = new GameObject("Glass surface fracture " + hitCount);
            mark.transform.SetParent(transform, false);
            var mesh = new Mesh { name = "Geodesic branching fracture" };
            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var triangles = new List<int>();
            var random = new System.Random(1439 + hitCount * 191);
            Vector3 right = Vector3.Cross(origin, Mathf.Abs(origin.y) < .9f ? Vector3.up : Vector3.right).normalized;
            Vector3 up = Vector3.Cross(origin, right).normalized;
            float rotation = (float)random.NextDouble() * Mathf.PI * 2;
            for (int ray = 0; ray < 6; ray++)
            {
                float angle = rotation + ray * Mathf.PI / 3f + Signed(random) * .23f;
                Vector3 tangent = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)).normalized;
                float length = ray < 3 ? 1.55f + (float)random.NextDouble() * .38f : .72f + (float)random.NextDouble() * .48f;
                var path = MakePath(origin, tangent, length, 22, random);
                AppendRibbon(path, crackWidth * (ray < 3 ? 1f : .7f), vertices, colors, triangles);
                for (int branch = 0; branch < 2; branch++)
                {
                    int index = 6 + branch * 6 + random.Next(0, 3);
                    Vector3 branchOrigin = path[index];
                    Vector3 along = Vector3.ProjectOnPlane(path[index + 1] - path[index - 1], branchOrigin).normalized;
                    Vector3 side = Vector3.Cross(branchOrigin, along);
                    Vector3 branchDirection = (along * .7f + side * (branch == 0 ? .72f : -.72f)).normalized;
                    var branchPath = MakePath(branchOrigin, branchDirection, .25f + (float)random.NextDouble() * .38f, 10, random);
                    AppendRibbon(branchPath, crackWidth * .43f, vertices, colors, triangles);
                }
            }
            mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            mark.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = mark.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = crackMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            mark.AddComponent<W06FractureMeshLifetime>().mesh = mesh;
            marks.Enqueue(mark);
        }

        static List<Vector3> MakePath(Vector3 origin, Vector3 tangent, float length, int segments, System.Random random)
        {
            var points = new List<Vector3>(segments + 1);
            Vector3 sideways = Vector3.Cross(origin, tangent).normalized;
            for (int i = 0; i <= segments; i++)
            {
                float angle = length * i / segments;
                float jitter = i == 0 ? 0 : Signed(random) * .028f;
                points.Add((origin * Mathf.Cos(angle) + tangent * Mathf.Sin(angle) + sideways * jitter).normalized);
            }
            return points;
        }

        static void AppendRibbon(List<Vector3> path, float width, List<Vector3> vertices, List<Color> colors, List<int> triangles)
        {
            int first = vertices.Count;
            float[] lanes = { -1f, -.4f, .42f, .75f, 1f };
            Color[] ink = { new Color(.68f, .95f, 1f, .12f), new Color(.02f, .095f, .12f, .98f),
                new Color(.01f, .055f, .075f, .98f), new Color(.88f, 1f, 1f, 1f), new Color(.7f, .98f, 1f, .08f) };
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 radial = path[i];
                Vector3 tangent = path[Mathf.Min(i + 1, path.Count - 1)] - path[Mathf.Max(i - 1, 0)];
                Vector3 side = Vector3.Cross(radial, tangent).normalized;
                float taper = Mathf.Lerp(1f, .16f, Mathf.Pow((float)i / (path.Count - 1), 2));
                for (int lane = 0; lane < lanes.Length; lane++)
                {
                    vertices.Add((radial * .5035f + side * lanes[lane] * width * taper).normalized * .5035f);
                    colors.Add(ink[lane]);
                }
                if (i == 0) continue;
                for (int lane = 0; lane < lanes.Length - 1; lane++)
                {
                    int a = first + (i - 1) * lanes.Length + lane;
                    int b = a + lanes.Length;
                    triangles.Add(a); triangles.Add(a + 1); triangles.Add(b);
                    triangles.Add(a + 1); triangles.Add(b + 1); triangles.Add(b);
                }
            }
        }

        static float Signed(System.Random random) => (float)random.NextDouble() * 2f - 1f;
        void OnDestroy() { if (ownedMaterial) Destroy(ownedMaterial); }
    }

    public sealed class W06FractureMeshLifetime : MonoBehaviour
    {
        public Mesh mesh;
        void OnDestroy() { if (mesh) Destroy(mesh); }
    }
}

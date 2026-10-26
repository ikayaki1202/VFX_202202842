using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace W06Homework
{
    /// <summary>One-shot, mesh-particle impact prefab spawned by the course Actor_Instantiate.</summary>
    public sealed class W06OrbBurst : MonoBehaviour
    {
        public OrbKind kind;
        public Material particleMaterial;
        [Range(8, 80)] public int particleCount = 36;
        public float lifetimeMin = .65f;
        public float lifetimeMax = 1.35f;
        public float speedMin = 2.2f;
        public float speedMax = 5f;
        public float sizeMin = .055f;
        public float sizeMax = .18f;
        Mesh particleMesh;

        void Start()
        {
            bool water = kind == OrbKind.Water;
            var particles = gameObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = .1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 96;
            main.startSpeed = 0;
            main.startLifetime = lifetimeMax;
            main.startSize3D = true;
            main.startRotation3D = true;
            main.gravityModifier = water ? .7f : .85f;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            var emission = particles.emission; emission.enabled = false;
            var shape = particles.shape; shape.enabled = false;
            var velocity = particles.limitVelocityOverLifetime;
            velocity.enabled = true; velocity.limit = 8; velocity.dampen = .08f;
            var color = particles.colorOverLifetime; color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, .55f), new GradientAlphaKey(0, 1) });
            color.color = gradient;
            var size = particles.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0, .75f), new Keyframe(.12f, 1f), new Keyframe(.75f, 1f), new Keyframe(1, .3f)));
            var rotation = particles.rotationOverLifetime; rotation.enabled = true;
            rotation.separateAxes = true;
            rotation.x = new ParticleSystem.MinMaxCurve(-7, 7);
            rotation.y = new ParticleSystem.MinMaxCurve(-10, 10);
            rotation.z = new ParticleSystem.MinMaxCurve(-12, 12);
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            particleMesh = water ? MakeDroplet() : MakeShard();
            renderer.mesh = particleMesh;
            renderer.sharedMaterial = particleMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.alignment = ParticleSystemRenderSpace.World;
            particles.Play();

            // Actor_Instantiate maps the prefab's Y axis to the collision normal.
            for (int i = 0; i < particleCount; i++)
            {
                float angle = i * 2.399963f + Random.Range(-.15f, .15f);
                float spread = Random.Range(.24f, .95f);
                Vector3 normal = transform.up;
                Vector3 radial = transform.right * Mathf.Cos(angle) + transform.forward * Mathf.Sin(angle);
                Vector3 direction = (normal * Random.Range(.5f, 1.1f) + radial * spread).normalized;
                float scale = Random.Range(sizeMin, sizeMax);
                if (water && i % 5 == 0) scale *= 1.35f;
                var emit = new ParticleSystem.EmitParams
                {
                    position = transform.position + normal * .045f + radial * Random.Range(0, .07f),
                    velocity = direction * Random.Range(speedMin, speedMax) + Vector3.up * (water ? .8f : .45f),
                    startLifetime = Random.Range(lifetimeMin, lifetimeMax),
                    startSize3D = water ? new Vector3(scale * .78f, scale * 1.25f, scale * .78f) : new Vector3(scale, scale * Random.Range(1.1f, 2.1f), scale),
                    rotation3D = Random.insideUnitSphere * 180f,
                    startColor = water ? Color.Lerp(new Color(.42f, .88f, 1f, .92f), new Color(.9f, 1f, 1f, 1f), Random.value)
                        : Color.Lerp(new Color(.49f, .83f, .95f, .9f), Color.white, Random.value)
                };
                particles.Emit(emit, 1);
            }
        }

        static Mesh MakeShard()
        {
            Vector3[] corners = { new Vector3(-.48f, -.45f, -.055f), new Vector3(.42f, -.3f, -.04f), new Vector3(.12f, .55f, -.025f),
                new Vector3(-.48f, -.45f, .055f), new Vector3(.42f, -.3f, .04f), new Vector3(.12f, .55f, .025f) };
            int[] faces = { 0, 2, 1, 3, 4, 5, 0, 1, 4, 0, 4, 3, 1, 2, 5, 1, 5, 4, 2, 0, 3, 2, 3, 5 };
            var vertices = new Vector3[faces.Length]; var triangles = new int[faces.Length]; var uv = new Vector2[faces.Length];
            for (int i = 0; i < faces.Length; i++) { vertices[i] = corners[faces[i]]; triangles[i] = i; uv[i] = new Vector2(vertices[i].x + .5f, vertices[i].y + .5f); }
            var mesh = new Mesh { name = "Angular glass splinter", vertices = vertices, triangles = triangles, uv = uv };
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        static Mesh MakeDroplet()
        {
            const int rings = 9, segments = 12;
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            for (int y = 0; y <= rings; y++)
            {
                float v = (float)y / rings; float theta = v * Mathf.PI;
                float taper = Mathf.Lerp(.86f, 1.05f, v);
                for (int x = 0; x <= segments; x++)
                {
                    float u = (float)x / segments; float phi = u * Mathf.PI * 2;
                    vertices.Add(new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi) * taper, Mathf.Cos(theta), Mathf.Sin(theta) * Mathf.Sin(phi) * taper) * .5f);
                    uv.Add(new Vector2(u, v));
                    if (y == 0 || x == 0) continue;
                    int a = y * (segments + 1) + x; int b = a - segments - 1;
                    triangles.Add(a); triangles.Add(b - 1); triangles.Add(b);
                    triangles.Add(a); triangles.Add(a - 1); triangles.Add(b - 1);
                }
            }
            var mesh = new Mesh { name = "Rounded water droplet" };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        void OnDestroy() { if (particleMesh) Destroy(particleMesh); }
    }
}

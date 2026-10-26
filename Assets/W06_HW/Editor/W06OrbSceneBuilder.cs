using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace W06Homework
{
    // All native materials, meshes, prefabs and scene changes are saved through Unity APIs.
    public static class W06OrbSceneBuilder
    {
        const string Root = "Assets/W06_HW/";

        [MenuItem("W06/Build Glass and Water Orbs")]
        public static void BuildMenu() => Debug.Log(Apply());

        public static string Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
            var session = UnityEngine.Object.FindFirstObjectByType<W06LabSession>();
            if (!session) throw new InvalidOperationException("Open W06_HW.unity first.");
            foreach (string folder in new[] { "Shaders", "Materials", "Prefabs", "Meshes" })
                if (!AssetDatabase.IsValidFolder(Root + folder)) AssetDatabase.CreateFolder(Root.TrimEnd('/'), folder);
            var glassShader = Shader.Find("W06/Glass Orb");
            var waterShader = Shader.Find("W06/Water Orb");
            if (!glassShader || !waterShader) throw new InvalidOperationException("Import the two orb shaders first.");
            var glass = Material("Mat_GlassOrb_HW", glassShader);
            glass.SetColor("_BaseColor", new Color(.64f, .88f, .96f, 1));
            glass.SetFloat("_Opacity", .23f);
            glass.SetFloat("_Smoothness", .96f);
            var water = Material("Mat_WaterOrb_HW", waterShader);
            water.SetColor("_BaseColor", new Color(.035f, .48f, .73f, 1));
            water.SetFloat("_Opacity", .46f);
            water.SetFloat("_Smoothness", .96f);
            water.SetFloat("_ImpactTime", -100f);
            var crack = Material("Mat_GlassCrack_HW", Shader.Find("W06/CrackSurface"));
            var glassParticle = ParticleMaterial("Mat_GlassShard_HW", new Color(.63f, .87f, .94f, .85f), .94f);
            var waterParticle = ParticleMaterial("Mat_WaterDrop_HW", new Color(.06f, .57f, .85f, .88f), 1f);
            var mesh = SphereMesh();
            var burstPrefabs = new[] { Burst(OrbKind.Glass, glassParticle), Burst(OrbKind.Water, waterParticle) };

            // Replace only the homework targets and stands; professor map roots remain untouched.
            foreach (var target in session.targets) if (target) UnityEngine.Object.DestroyImmediate(target.gameObject);
            var stand = GameObject.Find("W06 Courtyard Target Stands");
            if (stand) UnityEngine.Object.DestroyImmediate(stand);
            stand = new GameObject("W06 Courtyard Target Stands");
            stand.transform.position = session.zoneOrigin;
            var targets = new List<Transform>();
            var font = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset")
                .Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>).First(f => f);
            var dark = Material("Mat_OrbStand_HW", Shader.Find("Universal Render Pipeline/Lit"));
            dark.SetColor("_BaseColor", new Color(.035f, .065f, .085f));
            dark.SetFloat("_Metallic", .6f); dark.SetFloat("_Smoothness", .55f);
            var targetNames = new[] { "Glass", "Water" };
            var mats = new[] { glass, water };
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -2.6f : 2.6f;
                var accent = Material("Mat_OrbAccent_" + i, Shader.Find("Universal Render Pipeline/Lit"));
                Color c = i == 0 ? new Color(.58f, .86f, .96f) : new Color(.04f, .7f, .96f);
                accent.SetColor("_BaseColor", c); accent.EnableKeyword("_EMISSION"); accent.SetColor("_EmissionColor", c * 1.5f);
                Box(stand.transform, "Bay " + i + " plinth", new Vector3(x, .35f, 5.8f), new Vector3(3.05f, .7f, 2.6f), dark);
                Box(stand.transform, "Bay " + i + " trim", new Vector3(x, .72f, 5.8f), new Vector3(2.96f, .025f, 2.51f), accent);
                var label = new GameObject(i == 0 ? "01 / GLASS" : "02 / WATER").AddComponent<TextMeshPro>();
                label.transform.SetParent(stand.transform, false); label.transform.localPosition = new Vector3(x, .37f, 4.48f);
                label.font = font; label.fontSize = 2.5f; label.color = c; label.alignment = TextAlignmentOptions.Center;
                label.rectTransform.sizeDelta = new Vector2(3, .5f); label.text = label.name;

                var go = new GameObject("Target_" + targetNames[i] + "_HW", typeof(MeshFilter), typeof(MeshRenderer), typeof(SphereCollider));
                go.transform.position = session.zoneOrigin + new Vector3(x, 2.12f, 5.8f);
                go.transform.localScale = Vector3.one * 2.25f;
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = mats[i];
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                var surface = go.GetComponent<SphereCollider>(); surface.radius = .5f;
                var reaction = go.AddComponent<W06ReactiveOrb>(); reaction.kind = i == 0 ? OrbKind.Glass : OrbKind.Water;
                reaction.surfaceRenderer = renderer; reaction.crackMaterial = crack;
                var actor = go.AddComponent<Actor_Instantiate>(); actor.surfaceCollider = surface; actor.attachToThis = false;
                actor.prefabs = new List<Actor_Instantiate.Entry> { new Actor_Instantiate.Entry { prefab = burstPrefabs[i], lifetime = 3, alignToSurface = true } };
                var meter = go.AddComponent<W06TargetTelemetry>(); meter.materialName = targetNames[i];
                var sense = new GameObject("Bullet detection"); sense.transform.SetParent(go.transform, false);
                var collider = sense.AddComponent<SphereCollider>(); collider.radius = .67f; collider.isTrigger = true;
                var trigger = sense.AddComponent<Trigger_OnTriggerTag>(); trigger.RequiredTag = "Bullet";
                trigger.OnEnter = new GameObjectEvent(); trigger.OnExit = new GameObjectEvent();
                var gate = go.AddComponent<W06OrbHitGate>(); gate.surfaceCollider = surface; gate.OnConfirmedHit = new GameObjectEvent();
                UnityEventTools.AddPersistentListener(trigger.OnEnter, gate.TryHit);
                UnityEventTools.AddPersistentListener(gate.OnConfirmedHit, reaction.ReceiveHit);
                UnityEventTools.AddPersistentListener(gate.OnConfirmedHit, actor.InstantiateAtObject);
                UnityEventTools.AddPersistentListener(gate.OnConfirmedHit, meter.RegisterHit);
                targets.Add(go.transform);
            }
            session.targets = targets.ToArray();
            session.player.transform.SetPositionAndRotation(session.zoneOrigin + new Vector3(-2.6f, 1, -1.8f), Quaternion.identity);
            session.statusLabel.text = "TRACE / GLASS & WATER";
            var hud = session.statusLabel.canvas.transform;
            hud.Find("Controls").GetComponent<TMP_Text>().text = "WASD  MOVE     MOUSE  LOOK     CLICK  FIRE     R  RESET     F6  SHOWCASE     ESC  CURSOR";
            foreach (var camera in session.player.GetComponentsInChildren<Camera>(true).Append(session.showcaseCamera))
            {
                var data = camera.GetUniversalAdditionalCameraData();
                data.requiresColorOption = CameraOverrideOption.On;
                data.requiresDepthOption = CameraOverrideOption.On;
            }
            W06OriginalMapAdapter.FixDirectFire();
            EditorUtility.SetDirty(glass); EditorUtility.SetDirty(water); EditorUtility.SetDirty(crack); EditorUtility.SetDirty(dark);
            EditorSceneManager.MarkSceneDirty(session.gameObject.scene);
            EditorSceneManager.SaveScene(session.gameObject.scene);
            AssetDatabase.SaveAssets();
            return "Glass/water orbs saved in the original courtyard; 2 Inspector-wired targets, high-resolution sphere mesh, refraction cameras enabled.";
        }

        static Material Material(string name, Shader shader)
        {
            var path = Root + "Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
            mat.shader = shader; EditorUtility.SetDirty(mat); return mat;
        }

        static Material ParticleMaterial(string name, Color color, float smoothness)
        {
            var mat = Material(name, Shader.Find("W06/Impact Particle"));
            bool glass = name.Contains("Glass");
            mat.SetColor("_BaseColor", color); mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Opacity", glass ? .48f : .72f);
            mat.SetFloat("_Refraction", glass ? .003f : .004f); mat.SetFloat("_Reflection", .7f);
            mat.renderQueue = 3100;
            EditorUtility.SetDirty(mat); return mat;
        }

        static GameObject Burst(OrbKind kind, Material material)
        {
            var root = new GameObject("FX_HW_" + kind);
            var burst = root.AddComponent<W06OrbBurst>(); burst.kind = kind; burst.particleMaterial = material;
            burst.particleCount = kind == OrbKind.Glass ? 36 : 44;
            burst.sizeMin = kind == OrbKind.Glass ? .055f : .07f;
            burst.sizeMax = kind == OrbKind.Glass ? .23f : .2f;
            burst.speedMin = 2.3f; burst.speedMax = kind == OrbKind.Glass ? 5.4f : 4.7f;
            burst.lifetimeMin = .75f; burst.lifetimeMax = 1.5f;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Root + "Prefabs/FX_HW_" + kind + ".prefab");
            UnityEngine.Object.DestroyImmediate(root); return prefab;
        }

        static void Box(Transform parent, string name, Vector3 p, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = p; go.transform.localScale = size; go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        static Mesh SphereMesh()
        {
            const string path = Root + "Meshes/OrbSphere_HW.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (existing) return existing;
            const int longitude = 96, latitude = 64;
            var vertices = new Vector3[(longitude + 1) * (latitude + 1)]; var normals = new Vector3[vertices.Length]; var uv = new Vector2[vertices.Length];
            var triangles = new int[longitude * latitude * 6]; int at = 0;
            for (int y = 0; y <= latitude; y++) for (int x = 0; x <= longitude; x++)
            {
                float theta = Mathf.PI * y / latitude, phi = 2 * Mathf.PI * x / longitude;
                int n = y * (longitude + 1) + x;
                normals[n] = new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Cos(theta), Mathf.Sin(theta) * Mathf.Sin(phi));
                vertices[n] = normals[n] * .5f; uv[n] = new Vector2((float)x / longitude, (float)y / latitude);
                if (x == longitude || y == latitude) continue;
                int b = n + longitude + 1;
                triangles[at++] = n; triangles[at++] = n + 1; triangles[at++] = b;
                triangles[at++] = n + 1; triangles[at++] = b + 1; triangles[at++] = b;
            }
            var mesh = new Mesh { name = "OrbSphere_96x64", vertices = vertices, normals = normals, uv = uv, triangles = triangles };
            mesh.RecalculateTangents(); mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1.8f);
            AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
    }
}

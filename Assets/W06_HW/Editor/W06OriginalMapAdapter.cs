using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace W06Homework
{
    // Merge through Unity's scene API so prefab links, terrain and lighting survive intact.
    public static class W06OriginalMapAdapter
    {
        const string Source = "Assets/W06_TraceAndMark/W06_TraceAndMark_Demo.unity";
        const string Destination = "Assets/W06_HW/W06_HW.unity";
        public static readonly Vector3 Courtyard = new Vector3(-75, .0006103516f, 310);

        public static string Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
            var homework = SceneManager.GetSceneByPath(Destination);
            if (!homework.IsValid()) homework = EditorSceneManager.OpenScene(Destination);
            var roots = homework.GetRootGameObjects();
            if (roots.Any(g => g.name == "Industrial_Zone_Canal")) return "Original map already present.";
            var source = SceneManager.GetSceneByPath(Source);
            if (!source.IsValid()) source = EditorSceneManager.OpenScene(Source, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(source);

            // Only class demonstration objects are removed from the destination copy.
            foreach (var go in source.GetRootGameObjects())
                if (go.name == "Shooter" || go.name.StartsWith("HitEffect_"))
                    UnityEngine.Object.DestroyImmediate(go);
            var sourceMap = source.GetRootGameObjects();
            var originalTransforms = sourceMap.SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .ToDictionary(t => t, t => t.localToWorldMatrix);

            var session = roots.Select(g => g.GetComponent<W06LabSession>()).First(s => s);
            Vector3 offset = Courtyard - session.zoneOrigin;
            var stand = roots.First(g => g.name.StartsWith("East Material Lab"));
            foreach (Transform child in stand.transform.Cast<Transform>().ToArray())
            {
                bool keep = child.name.Contains("plinth") || child.name == "Target mount"
                    || child.name == "01 / METAL" || child.name == "02 / WOOD" || child.name == "03 / CONCRETE";
                if (!keep) UnityEngine.Object.DestroyImmediate(child.gameObject);
                else if (child.name.Contains(" / "))
                {
                    child.localPosition = new Vector3(child.localPosition.x, .4f, 4.68f);
                    child.GetComponent<TMPro.TMP_Text>().fontSize = 2.5f;
                }
            }
            stand.name = "W06 Courtyard Target Stands";
            stand.transform.position = Courtyard;
            foreach (var target in session.targets) target.position += offset;
            session.player.transform.position = Courtyard + new Vector3(0, 1, -5);
            session.zoneOrigin = Courtyard;
            session.showcaseCamera.transform.position = Courtyard + new Vector3(-1.3f, 3.3f, -5.3f);
            session.showcaseCamera.transform.LookAt(Courtyard + new Vector3(0, 2, 6));
            session.showcaseCamera.gameObject.SetActive(false);
            foreach (var camera in session.player.GetComponentsInChildren<Camera>(true).Append(session.showcaseCamera))
            {
                camera.clearFlags = CameraClearFlags.Skybox;
                camera.farClipPlane = 700;
            }

            foreach (var go in roots)
            {
                bool keep = go == stand || go == session.gameObject || go == session.player
                    || go == session.showcaseCamera.gameObject || go.name == "Lab HUD"
                    || session.targets.Any(t => t.gameObject == go);
                if (keep) SceneManager.MoveGameObjectToScene(go, source);
                else UnityEngine.Object.DestroyImmediate(go);
            }
            int changed = originalTransforms.Count(pair => pair.Key.localToWorldMatrix != pair.Value);
            if (changed != 0) throw new InvalidOperationException("An original map transform changed.");
            EditorSceneManager.CloseScene(homework, true);
            EditorSceneManager.SaveScene(source, Destination);
            FinishPresentation();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Destination, true) };
            AssetDatabase.SaveAssets();
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.LookAt(Courtyard + new Vector3(0, 2, 5), Quaternion.Euler(12, 0, 0), 15);
            return "Original map copied: " + sourceMap.Length + " roots, " + originalTransforms.Count
                + " unchanged transforms. Homework courtyard: " + Courtyard + ". Source file was not saved.";
        }

        public static string FinishPresentation()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
            var session = UnityEngine.Object.FindFirstObjectByType<W06LabSession>();
            foreach (var target in session.targets)
                target.GetComponentsInChildren<SphereCollider>().First(c => c.isTrigger).radius = .85f;
            var hud = session.statusLabel.canvas.transform;
            if (!hud.Find("Controls backing"))
            {
                var go = new GameObject("Controls backing", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                go.transform.SetParent(hud, false);
                go.transform.SetAsFirstSibling();
                var rt = (RectTransform)go.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(.5f, 0); rt.sizeDelta = new Vector2(0, 85);
                var backing = go.GetComponent<UnityEngine.UI.Image>();
                backing.color = new Color(.025f, .045f, .06f, .78f); backing.raycastTarget = false;
            }
            hud.Find("Controls").GetComponent<TMPro.TMP_Text>().color = new Color(.87f, .92f, .95f);
            EditorSceneManager.MarkSceneDirty(session.gameObject.scene);
            EditorSceneManager.SaveScene(session.gameObject.scene);
            return "Controls readable on the courtyard ground; trigger shell widened for fast bullets.";
        }
    }
}

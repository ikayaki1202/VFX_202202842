using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace W06Homework
{
    public sealed class W06LabSession : MonoBehaviour
    {
        public GameObject player;
        public SO_ItemData rifle;
        public Projectile_Bullet projectile;
        public Camera showcaseCamera;
        public Transform[] targets;
        public TMP_Text statusLabel;
        public Vector3 zoneOrigin = new Vector3(-75, 0, 310);
        public bool demoComplete;
        public bool demoRunning;
        public int shotsFired;
        Coroutine demo;

        IEnumerator Start()
        {
            Application.runInBackground = true;
            yield return null;
            yield return new WaitForSeconds(.2f);
            if (player && rifle)
            {
                player.GetComponentInChildren<Player_Inventory>()?.AddItem(rifle);
                player.GetComponentInChildren<Player_GrabBehavior>()?.Equip(rifle);
            }
            SetLabel("TRACE / GLASS & WATER");
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.f6Key.wasPressedThisFrame)
            {
                if (demoRunning) EndShowcase(); else BeginShowcase();
            }
            if (keyboard.rKey.wasPressedThisFrame) ClearMarks();
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (demoRunning) EndShowcase();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void ClearMarks()
        {
            foreach (var orb in FindObjectsByType<W06ReactiveOrb>(FindObjectsSortMode.None)) orb.ResetSurface();
            foreach (var decal in FindObjectsByType<UnityEngine.Rendering.Universal.DecalProjector>(FindObjectsSortMode.None))
                if (decal.transform.root.name.StartsWith("Decal_HW_")) Destroy(decal.transform.root.gameObject);
        }

        public void BeginShowcase()
        {
            if (demoRunning || !showcaseCamera || targets == null || targets.Length == 0 || !projectile) return;
            ClearMarks();
            demoComplete = false;
            shotsFired = 0;
            demoRunning = true;
            player.SetActive(false);
            showcaseCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
            demo = StartCoroutine(Showcase());
        }

        public void EndShowcase()
        {
            if (demo != null) StopCoroutine(demo);
            demoRunning = false;
            showcaseCamera.gameObject.SetActive(false);
            player.SetActive(true);
            SetLabel("TRACE / GLASS & WATER");
        }

        IEnumerator Showcase()
        {
            SetLabel("TRACE / GLASS & WATER     •     MATERIAL STUDY 06");
            showcaseCamera.fieldOfView = 49;
            yield return MoveCamera(zoneOrigin + new Vector3(-1.3f, 3.3f, -5.3f), zoneOrigin + new Vector3(0, 2, 6), 2.5f, true);
            for (int i = 0; i < targets.Length; i++)
            {
                SetLabel(targets[i].name.Contains("Glass") ? "01 / GLASS   —   FRACTURE & SHARDS" : "02 / WATER   —   RIPPLE & SPLASH");
                Vector3 center = targets[i].position;
                Vector3 offset = new Vector3(i == 1 ? .65f : -.65f, .25f, -4.4f);
                yield return MoveCamera(center + offset, center, 1.0f, false);
                yield return new WaitForSeconds(.4f);
                for (int n = 0; n < 5; n++)
                {
                    Vector3 aim = center + new Vector3((n % 3 - 1) * .17f, ((n * 2) % 3 - 1) * .14f, -.15f);
                    FireAt(aim);
                    yield return new WaitForSeconds(1.35f);
                }
                yield return new WaitForSeconds(1.0f);
            }
            SetLabel("GLASS KEEPS THE FRACTURE. WATER RETURNS TO STILLNESS.");
            yield return MoveCamera(zoneOrigin + new Vector3(0, 3.1f, -4.9f), zoneOrigin + new Vector3(0, 2, 6), 2.0f, false);
            yield return new WaitForSeconds(2);
            demoComplete = true;
            Debug.Log("[W06_DEMO_COMPLETE] shots=" + shotsFired);
        }

        IEnumerator MoveCamera(Vector3 end, Vector3 look, float duration, bool snap)
        {
            Vector3 start = showcaseCamera.transform.position;
            Quaternion from = showcaseCamera.transform.rotation;
            Quaternion to = Quaternion.LookRotation(look - end);
            if (snap) { showcaseCamera.transform.SetPositionAndRotation(end, to); yield return new WaitForSeconds(duration); yield break; }
            float t = 0;
            while (t < duration)
            {
                float s = Mathf.SmoothStep(0, 1, t / duration);
                showcaseCamera.transform.SetPositionAndRotation(Vector3.Lerp(start, end, s), Quaternion.Slerp(from, to, s));
                t += Time.deltaTime;
                yield return null;
            }
            showcaseCamera.transform.SetPositionAndRotation(end, to);
        }

        public void FireAt(Vector3 point)
        {
            var cam = showcaseCamera.gameObject.activeInHierarchy ? showcaseCamera : (Camera.main ? Camera.main : player.GetComponentInChildren<Camera>());
            Vector3 origin = cam.transform.position + cam.transform.right * .28f - cam.transform.up * .16f + cam.transform.forward * .4f;
            var bullet = Instantiate(projectile);
            bullet.name = "Projectile_HW_Showcase";
            bullet.Launch(gameObject, origin, (point - origin).normalized, 32f);
            shotsFired++;
        }

        void SetLabel(string value) { if (statusLabel) statusLabel.text = value; }
    }
}

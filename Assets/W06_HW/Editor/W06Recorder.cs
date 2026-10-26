#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace W06Homework
{
    /// <summary>Captures the homework's complete Game View showcase with Unity Recorder.</summary>
    [InitializeOnLoad]
    public static class W06Recorder
    {
        static RecorderController controller;
        static RecorderControllerSettings settings;
        static MovieRecorderSettings movie;
        static W06LabSession session;
        static string state = "Idle";
        static string output;
        static string error;
        static int startFrame;
        static int recordedFrames;
        static int shots;
        static bool complete;
        static double startedAt;

        static W06Recorder()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;
        }

        public static void Start()
        {
            if (!EditorApplication.isPlaying || !Application.isPlaying)
                throw new InvalidOperationException("[W06 Recorder] Start must be called after the homework scene enters Play mode.");
            if (controller != null)
                throw new InvalidOperationException("[W06 Recorder] A recording session already exists. Call Stop before starting another.");
            if (Time.timeScale <= 0f)
                throw new InvalidOperationException("[W06 Recorder] Time.timeScale must be greater than zero for the showcase to advance.");

            var sessions = UnityEngine.Object.FindObjectsByType<W06LabSession>(FindObjectsSortMode.None);
            if (sessions.Length != 1)
                throw new InvalidOperationException("[W06 Recorder] Expected one active W06LabSession; found " + sessions.Length + ".");
            session = sessions[0];
            if (session.demoRunning)
                throw new InvalidOperationException("[W06 Recorder] The showcase is already running. End it before recording from the beginning.");
            if (session.player == null || session.showcaseCamera == null || session.projectile == null || session.targets == null
                || session.targets.Length != 3 || session.targets.Any(t => t == null))
                throw new InvalidOperationException("[W06 Recorder] The lab requires its player, showcase camera, projectile, and three target references.");

            var directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Deliverables"));
            Directory.CreateDirectory(directory);
            var outputBase = Path.Combine(directory, "W06_202202842").Replace('\\', '/');
            output = outputBase + ".mp4";
            error = null;
            complete = false;
            shots = 0;
            recordedFrames = 0;
            startFrame = Time.frameCount;
            startedAt = EditorApplication.timeSinceStartup;
            state = "Preparing";

            settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            settings.name = "W06 Showcase Controller";
            settings.hideFlags = HideFlags.HideAndDontSave;
            settings.FrameRate = 30f;
            settings.FrameRatePlayback = FrameRatePlayback.Constant;
            settings.CapFrameRate = true;
            settings.SetRecordModeToManual();

            movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movie.name = "W06_202202842";
            movie.hideFlags = HideFlags.HideAndDontSave;
            movie.Enabled = true;
            movie.OutputFile = outputBase;
            movie.EncoderSettings = new CoreEncoderSettings
            {
                Codec = CoreEncoderSettings.OutputCodec.MP4,
                EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
                EncodingProfile = CoreEncoderSettings.H264EncodingProfile.High,
                GopSize = 30
            };
            movie.ImageInputSettings = new GameViewInputSettings
            {
                OutputWidth = 1920,
                OutputHeight = 1080,
                FlipFinalOutput = false
            };
            // The recording camera replaces the player camera during the showcase.
            // Only request an audio stream when that camera has a listener and the scene has a source.
            var listener = session.showcaseCamera.GetComponent<AudioListener>();
            movie.CaptureAudio = listener != null && listener.enabled
                && UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None)
                    .Any(source => source.enabled && source.gameObject.activeInHierarchy && source.clip != null);
            settings.AddRecorderSettings(movie);
            controller = new RecorderController(settings);

            try
            {
                controller.PrepareRecording();
                if (!controller.StartRecording())
                    throw new InvalidOperationException("[W06 Recorder] Unity Recorder refused to start. Inspect the Console for encoder or Game View errors.");
                session.BeginShowcase();
                if (!session.demoRunning)
                    throw new InvalidOperationException("[W06 Recorder] BeginShowcase did not enter the running state.");
                state = "Recording";
                EditorApplication.update -= Tick;
                EditorApplication.update += Tick;
                Debug.Log("[W06_RECORDER_STARTED] " + output + " | 1920x1080 | 30fps | Game View with HUD | audio=" + movie.CaptureAudio);
            }
            catch (Exception exception)
            {
                error = exception.ToString();
                state = "Failed";
                Finish(false, true);
                throw;
            }
        }

        public static void Stop()
        {
            Finish(session != null && session.demoComplete, false);
        }

        public static string Status()
        {
            var active = controller != null && controller.IsRecording();
            var currentShots = session != null ? session.shotsFired : shots;
            var frames = active ? Math.Max(0, Time.frameCount - startFrame) : recordedFrames;
            return JsonUtility.ToJson(new RecorderStatus
            {
                state = state,
                recording = active,
                demoComplete = complete || session != null && session.demoComplete,
                shotsFired = currentShots,
                frameCount = frames,
                nominalDurationSeconds = frames / 30f,
                wallSeconds = controller == null ? 0 : EditorApplication.timeSinceStartup - startedAt,
                output = output,
                fileExists = !string.IsNullOrEmpty(output) && File.Exists(output),
                fileBytes = !string.IsNullOrEmpty(output) && File.Exists(output) ? new FileInfo(output).Length : 0,
                error = error
            });
        }

        static void Tick()
        {
            if (controller == null) { EditorApplication.update -= Tick; return; }
            if (!EditorApplication.isPlaying) { Finish(false, false); return; }
            if (session == null)
            {
                error = "The W06LabSession was destroyed before the recording finished.";
                Debug.LogError("[W06 Recorder] " + error);
                Finish(false, true);
                return;
            }
            shots = session.shotsFired;
            if (session.demoComplete)
            {
                Finish(true, false);
                return;
            }
            if (!controller.IsRecording())
            {
                error = "Unity Recorder stopped before the showcase completed.";
                Debug.LogError("[W06 Recorder] " + error);
                Finish(false, true);
            }
        }

        static void Finish(bool success, bool failed)
        {
            EditorApplication.update -= Tick;
            if (controller == null) return;
            if (session != null) shots = session.shotsFired;
            recordedFrames = Application.isPlaying ? Math.Max(0, Time.frameCount - startFrame) : recordedFrames;
            controller.StopRecording();
            controller = null;
            complete = success;
            state = failed ? "Failed" : success ? "Completed" : "Stopped";
            if (movie != null) UnityEngine.Object.DestroyImmediate(movie);
            if (settings != null) UnityEngine.Object.DestroyImmediate(settings);
            movie = null;
            settings = null;
            session = null;

            if (success && (string.IsNullOrEmpty(output) || !File.Exists(output) || new FileInfo(output).Length == 0))
            {
                state = "Failed";
                complete = false;
                error = "The showcase completed but Unity Recorder produced no nonempty MP4 at " + output;
                Debug.LogError("[W06 Recorder] " + error);
                throw new IOException(error);
            }
            Debug.Log("[W06_RECORDER_" + state.ToUpperInvariant() + "] shots=" + shots + " | " + output);
        }

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode && controller != null)
                Finish(session != null && session.demoComplete, false);
        }

        static void BeforeAssemblyReload()
        {
            if (controller == null) return;
            error = "Script assemblies reloaded during recording; this capture was interrupted.";
            Debug.LogError("[W06 Recorder] " + error);
            Finish(false, true);
        }

        [Serializable]
        sealed class RecorderStatus
        {
            public string state;
            public bool recording;
            public bool demoComplete;
            public int shotsFired;
            public int frameCount;
            public float nominalDurationSeconds;
            public double wallSeconds;
            public string output;
            public bool fileExists;
            public long fileBytes;
            public string error;
        }
    }
}
#endif

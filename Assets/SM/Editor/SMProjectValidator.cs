// Validates SM project wiring: scenes, MapCatalog, NetworkManager+FishySteamworks,
// Surf layer, StartZone, and Player prefab requirements.
// Menu: SM/Run Project Health Check
// Logs a PASS/WARN/FAIL summary and writes Assets/SM/ValidationReport.txt

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

// FishNet types (safe if FishNet present)
using FishNet.Managing;
using FishNet.Component.Transforming;

namespace SM.EditorTools
{
    public static class SMProjectValidator
    {
        private const string ReportPath = "Assets/SM/ValidationReport.txt";

        private enum Sev { Pass, Warn, Fail }
        private struct Issue { public Sev sev; public string msg; }

        [MenuItem("SM/Run Project Health Check")]
        public static void Run()
        {
            var issues = new List<Issue>();
            void PASS(string m) => issues.Add(new Issue { sev = Sev.Pass, msg = m });
            void WARN(string m) => issues.Add(new Issue { sev = Sev.Warn, msg = m });
            void FAIL(string m) => issues.Add(new Issue { sev = Sev.Fail, msg = m });

            Debug.Log("[SM Validator] Starting checks…");

            // ----- Build settings scenes -----
            var buildScenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToList();
            string Name(string path) => System.IO.Path.GetFileNameWithoutExtension(path);
            bool HasScene(string n) => buildScenes.Any(p => Name(p).Equals(n, StringComparison.OrdinalIgnoreCase));
            string GetScenePathByName(string n) => buildScenes.FirstOrDefault(p => Name(p).Equals(n, StringComparison.OrdinalIgnoreCase));

            string bootName = "00_Boot";
            string menuName = "MainMenu";
            string lobbyName = "Lobby";

            if (HasScene(bootName)) PASS($"Build Settings contains Boot scene: {bootName}");
            else WARN($"Build Settings missing Boot scene named '{bootName}'");

            if (HasScene(menuName)) PASS($"Build Settings contains MainMenu scene: {menuName}");
            else FAIL($"Build Settings missing MainMenu scene named '{menuName}'");

            if (HasScene(lobbyName)) PASS($"Build Settings contains Lobby scene: {lobbyName}");
            else FAIL($"Build Settings missing Lobby scene named '{lobbyName}'");

            // ----- MapCatalog -----
            var catalogGuids = AssetDatabase.FindAssets("t:MapCatalog");
            SM.Net.MapCatalog catalog = null;
            if (catalogGuids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(catalogGuids[0]);
                catalog = AssetDatabase.LoadAssetAtPath<SM.Net.MapCatalog>(path);
                PASS($"Found MapCatalog at {path}");
                if (catalog.Maps == null || catalog.Maps.Length == 0)
                {
                    FAIL("MapCatalog has no Maps defined.");
                }
                else
                {
                    foreach (var m in catalog.Maps)
                    {
                        if (string.IsNullOrWhiteSpace(m.SceneName))
                            WARN("A MapEntry has empty SceneName.");
                        else if (!HasScene(m.SceneName))
                            WARN($"Map '{m.DisplayName}' scene '{m.SceneName}' is not in Build Settings.");
                        else
                            PASS($"Map '{m.DisplayName}' OK (scene '{m.SceneName}' in Build Settings).");
                    }
                }
            }
            else
            {
                FAIL("No MapCatalog asset found (Create → SM → Map Catalog).");
            }

            // ----- Surf layer exists -----
            var layers = InternalEditorUtility.layers;
            if (layers.Contains("Surf")) PASS("Layer 'Surf' exists.");
            else FAIL("Layer 'Surf' not found. Create a layer named 'Surf' and put surf ramps on it.");

            // keep track of currently open scenes for restoration
            var originallyOpen = Enumerable.Range(0, EditorSceneManager.sceneCount)
                                           .Select(EditorSceneManager.GetSceneAt).ToList();
            var openedForCheck = new List<Scene>();

            void OpenAdditiveIfPresent(string n)
            {
                var p = GetScenePathByName(n);
                if (!string.IsNullOrEmpty(p))
                {
                    var s = EditorSceneManager.OpenScene(p, OpenSceneMode.Additive);
                    openedForCheck.Add(s);
                }
            }

            // ----- Inspect Boot scene wiring -----
            if (HasScene(bootName))
            {
                OpenAdditiveIfPresent(bootName);
                var boot = openedForCheck.LastOrDefault();

                var steamBootstrap = UnityEngine.Object.FindFirstObjectByType<SM.Steam.SteamBootstrap>();
                if (steamBootstrap) PASS("SteamBootstrap present in Boot.");
                else FAIL("SteamBootstrap missing in Boot.");

                var nm = UnityEngine.Object.FindFirstObjectByType<NetworkManager>();
                if (nm != null) PASS("NetworkManager present in Boot.");
                else FAIL("NetworkManager missing in Boot.");

                if (nm != null)
                {
                    var transport = nm.TransportManager != null ? nm.TransportManager.Transport : null;
                    string tn = transport != null ? transport.GetType().FullName : "(none)";
                    if (tn.Contains("FishySteamworks")) PASS($"Transport is FishySteamworks ({tn}).");
                    else FAIL($"Transport is not FishySteamworks (found {tn}). Set FishySteamworks on NetworkManager.");

                    try
                    {
                        ushort currentTick = nm.TimeManager != null ? nm.TimeManager.TickRate : (ushort)0;
                        if (currentTick == 60) PASS("TickRate = 60.");
                        else WARN($"TickRate is {currentTick}; spec suggests 60.");
                    }
                    catch (Exception ex)
                    {
                        WARN($"Could not read TimeManager.TickRate: {ex.Message}");
                    }
                }

                var bootLoader = UnityEngine.Object.FindFirstObjectByType<SM.Net.BootLoader>();
                if (bootLoader) PASS("BootLoader present in Boot (loads MainMenu).");
                else WARN("BootLoader not found in Boot (ensure it loads MainMenu).");
            }

            // ----- Inspect first gameplay map for StartZone -----
            if (catalog != null && catalog.Maps != null && catalog.Maps.Length > 0)
            {
                var firstMap = catalog.Maps[0];
                if (HasScene(firstMap.SceneName))
                {
                    OpenAdditiveIfPresent(firstMap.SceneName);
                    var szAll = UnityEngine.Object.FindObjectsByType<SM.Net.StartZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (szAll != null && szAll.Length > 0) PASS($"StartZone present in map '{firstMap.SceneName}'.");
                    else FAIL($"No StartZone found in map '{firstMap.SceneName}'. Add a StartZone.");
                }
            }

            // ----- Player prefab existence & components -----
            var motorPrefabGuids = AssetDatabase.FindAssets("t:Prefab SurfBhopMotor");
            if (motorPrefabGuids.Length == 0)
            {
                WARN("No prefab with SurfBhopMotor found (searching t:Prefab SurfBhopMotor). Ensure your Player prefab exists.");
            }
            else
            {
                var anyOk = false;
                foreach (var g in motorPrefabGuids)
                {
                    var p = AssetDatabase.GUIDToAssetPath(g);
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                    if (go == null) continue;

                    bool hasNO = go.GetComponent<FishNet.Object.NetworkObject>() != null;
                    bool hasNT = go.GetComponent<NetworkTransform>() != null;
                    bool hasCC = go.GetComponent<CharacterController>() != null;
                    string msg = $"Player prefab '{Path.GetFileName(p)}': " +
                                 $"{(hasNO ? "NetworkObject OK" : "NO MISSING")}, " +
                                 $"{(hasNT ? "NetworkTransform OK" : "NetworkTransform MISSING")}, " +
                                 $"{(hasCC ? "CharacterController OK" : "CharacterController MISSING")}";
                    if (hasNO && hasNT && hasCC) { PASS(msg); anyOk = true; }
                    else WARN(msg);

                    // Optional: nudge on send interval
                    if (hasNT)
                    {
                        var nt = go.GetComponent<NetworkTransform>();
                        // Some versions expose a send interval property; we just notify.
                        WARN($"Check NetworkTransform send interval on '{go.name}' → set to ~1/60s for 60Hz sends.");
                    }
                }
                if (!anyOk) FAIL("No Player prefab met all required components (NetworkObject + NetworkTransform + CharacterController).");
            }

            // ----- Cleanup: close scenes we opened -----
            foreach (var s in openedForCheck)
            {
                EditorSceneManager.CloseScene(s, true);
            }

            // Restore original scenes: leave them as-is (we opened Additive, so no harm).
            // ----- Summary -----
            var pass = issues.Count(i => i.sev == Sev.Pass);
            var warn = issues.Count(i => i.sev == Sev.Warn);
            var fail = issues.Count(i => i.sev == Sev.Fail);

            var summary = $"[SM Validator] Done. PASS:{pass}  WARN:{warn}  FAIL:{fail}";
            Debug.Log(summary);

            // Write report file
            var lines = new List<string> { summary };
            lines.AddRange(issues.Select(i => $"{i.sev.ToString().ToUpper(),-5}  {i.msg}"));
            File.WriteAllLines(ReportPath, lines);
            AssetDatabase.ImportAsset(ReportPath);

            if (fail > 0) EditorUtility.DisplayDialog("SM Health Check", $"{summary}\n\nSee Console and {ReportPath}.", "OK");
            else EditorUtility.DisplayDialog("SM Health Check", $"{summary}\n\nLooks good! See {ReportPath} for details.", "Nice");
        }
    }
}
#endif
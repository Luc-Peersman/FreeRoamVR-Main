using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace FreeRoamVR.EditorTools
{
    // Applies the same fixes as clicking "Fix"/"Fix All" in Project Settings > XR Plug-in
    // Management > Project Validation, via Unity's/Meta's own internal fix code, so the
    // OpenXR Package Settings asset gets regenerated/edited by the real APIs instead of by hand.
    internal static class ClaudeProjectFixer
    {
        [MenuItem("Tools/Fix Project Validation Issues")]
        public static async void FixEverything()
        {
            try
            {
                Debug.Log("[ProjectFixer] Starting...");

                RegenerateOpenXRSettingsIfNeeded();

                var groups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android };
                foreach (var group in groups)
                {
                    // Run twice: enabling "API Layers" can unlock a newly-valid follow-up task
                    // (registering the Meta XR Operator layer) that only appears once enabled.
                    await OVRProjectSetup.FixAllAsync(group);
                    await OVRProjectSetup.FixAllAsync(group);
                }

                // Skip the interactive "restart the editor" dialog the built-in Standalone
                // Graphics API fix shows, and just set it directly.
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
                Debug.Log("[ProjectFixer] Set Standalone Graphics API to Direct3D11.");

                FixStaleApiVersions(BuildTargetGroup.Standalone);
                FixStaleApiVersions(BuildTargetGroup.Android);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[ProjectFixer] Done. Re-open Project Settings > XR Plug-in Management > Project Validation to confirm 0 issues remain.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProjectFixer] Failed: {e}");
            }
        }

        private static void RegenerateOpenXRSettingsIfNeeded()
        {
            var type = Type.GetType("UnityEditor.XR.OpenXR.OpenXRProjectValidation, Unity.XR.OpenXR.Editor");
            if (type == null)
            {
                Debug.LogWarning("[ProjectFixer] Could not find OpenXRProjectValidation type - skipping duplicate-settings check.");
                return;
            }

            var hasNoDuplicates = type.GetMethod("AssetHasNoDuplicates", BindingFlags.NonPublic | BindingFlags.Static);
            if (hasNoDuplicates != null && (bool)hasNoDuplicates.Invoke(null, null))
            {
                Debug.Log("[ProjectFixer] OpenXR Package Settings asset has no duplicates, skipping regeneration.");
                return;
            }

            var regenerate = type.GetMethod("RegenerateXRPackageSettingsAsset", BindingFlags.NonPublic | BindingFlags.Static);
            if (regenerate == null)
            {
                Debug.LogWarning("[ProjectFixer] Could not find RegenerateXRPackageSettingsAsset method.");
                return;
            }

            regenerate.Invoke(null, null);
            Debug.Log("[ProjectFixer] Regenerated the OpenXR Package Settings asset (removed duplicate entries).");
        }

        private static void FixStaleApiVersions(BuildTargetGroup group)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            if (settings == null)
            {
                return;
            }

            var field = typeof(OpenXRFeature).GetField("targetOpenXRApiVersion", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogWarning("[ProjectFixer] Could not find targetOpenXRApiVersion field.");
                return;
            }

            foreach (var feature in settings.GetFeatures())
            {
                if (feature == null)
                {
                    continue;
                }

                var raw = (string)field.GetValue(feature);
                if (string.IsNullOrEmpty(raw))
                {
                    continue;
                }

                var parts = raw.Split('.');
                if (parts.Length != 3 || !int.TryParse(parts[0], out var major) || !int.TryParse(parts[2], out var patch))
                {
                    continue;
                }

                // Package currently ships OpenXRApiVersion.Current = 1.1.54.
                if (major == 1 && patch < 54)
                {
                    field.SetValue(feature, "1.1.54");
                    EditorUtility.SetDirty(feature);
                    Debug.Log($"[ProjectFixer] Bumped targetOpenXRApiVersion on '{feature.name}' ({group}) from {raw} to 1.1.54.");
                }
            }
        }
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using System.IO;
using UnityEditor.AddressableAssets;

namespace Project.Editor.Build
{
    /// <summary>
    /// Complete build automation for production deployment
    /// Полная автоматизация сборки для производственного развертывания
    /// </summary>
    public static class ProductionBuildPipeline
    {
        private const string BUILD_VERSION = "1.0.0";
        private const string WEBGL_BUILD_PATH = "Builds/WebGL";
        private const string ANDROID_BUILD_PATH = "Builds/Android";
        
        [MenuItem("Build Pipeline/Build All Platforms (Production)")]
        public static void BuildAllPlatforms()
        {
            Debug.Log("[BuildPipeline] Starting complete production build...");
            
            try
            {
                // Step 1: Prepare build environment
                PrepareForBuild();
                
                // Step 2: Build Addressables
                BuildAddressablesForProduction();
                
                // Step 3: Build WebGL
                BuildWebGLProduction();
                
                // Step 4: Build Android
                BuildAndroidProduction();
                
                // Step 5: Generate build report
                GenerateBuildReport();
                
                Debug.Log("[BuildPipeline] All builds completed successfully!");
                EditorUtility.DisplayDialog("Build Complete", 
                    "All platform builds completed successfully!\nCheck the Builds/ folder for outputs.", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuildPipeline] Build failed: {ex.Message}");
                EditorUtility.DisplayDialog("Build Failed", $"Build process failed: {ex.Message}", "OK");
            }
        }
        
        [MenuItem("Build Pipeline/Build WebGL Only")]
        public static void BuildWebGLOnly()
        {
            try
            {
                PrepareForBuild();
                BuildAddressablesForProduction();
                BuildWebGLProduction();
                Debug.Log("[BuildPipeline] WebGL build completed!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuildPipeline] WebGL build failed: {ex.Message}");
            }
        }
        
        [MenuItem("Build Pipeline/Build Android Only")]
        public static void BuildAndroidOnly()
        {
            try
            {
                PrepareForBuild();
                BuildAddressablesForProduction();
                BuildAndroidProduction();
                Debug.Log("[BuildPipeline] Android build completed!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuildPipeline] Android build failed: {ex.Message}");
            }
        }
        
        private static void PrepareForBuild()
        {
            Debug.Log("[BuildPipeline] Preparing build environment...");
            
            // Create build directories
            Directory.CreateDirectory(WEBGL_BUILD_PATH);
            Directory.CreateDirectory(ANDROID_BUILD_PATH);
            
            // Set build version
            PlayerSettings.bundleVersion = BUILD_VERSION;
            PlayerSettings.Android.bundleVersionCode = GetVersionCode();
            
            Debug.Log($"[BuildPipeline] Build version set to: {BUILD_VERSION}");
        }
        
        private static void BuildAddressablesForProduction()
        {
            Debug.Log("[BuildPipeline] Building Addressables for production...");
            
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                throw new System.Exception("Addressable Asset Settings not found!");
            }
            
            // Set production profile
            var productionProfileId = settings.profileSettings.GetProfileId("Production");
            if (!string.IsNullOrEmpty(productionProfileId))
            {
                settings.activeProfileId = productionProfileId;
            }
            
            // Clean previous content
            AddressableAssetSettings.CleanPlayerContent();
            
            // Build addressable content
            AddressableAssetSettings.BuildPlayerContent();
            
            Debug.Log("[BuildPipeline] Addressables build completed");
        }
        
        private static void BuildWebGLProduction()
        {
            Debug.Log("[BuildPipeline] Building WebGL...");
            
            // Set WebGL as target platform
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            
            // Configure WebGL settings
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.debugSymbols = false;
            
            // Build scenes
            string[] scenes = GetEnabledScenes();
            
            // Build options
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = WEBGL_BUILD_PATH,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            
            var report = BuildPipeline.BuildPlayer(buildOptions);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildPipeline] WebGL build succeeded: {report.summary.totalSize} bytes");
            }
            else
            {
                throw new System.Exception($"WebGL build failed: {report.summary.result}");
            }
        }
        
        private static void BuildAndroidProduction()
        {
            Debug.Log("[BuildPipeline] Building Android...");
            
            // Set Android as target platform
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            
            // Configure Android settings
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            EditorUserBuildSettings.buildAppBundle = true;
            
            // Build scenes
            string[] scenes = GetEnabledScenes();
            
            // Build options
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(ANDROID_BUILD_PATH, $"TestTaskAddressables_v{BUILD_VERSION}.aab"),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            
            var report = BuildPipeline.BuildPlayer(buildOptions);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildPipeline] Android build succeeded: {report.summary.totalSize} bytes");
            }
            else
            {
                throw new System.Exception($"Android build failed: {report.summary.result}");
            }
        }
        
        private static string[] GetEnabledScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }
            
            return scenes.ToArray();
        }
        
        private static void GenerateBuildReport()
        {
            Debug.Log("[BuildPipeline] Generating build report...");
            
            var report = new System.Text.StringBuilder();
            report.AppendLine("# Build Report");
            report.AppendLine($"**Build Date**: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"**Unity Version**: {Application.unityVersion}");
            report.AppendLine($"**Build Version**: {BUILD_VERSION}");
            report.AppendLine();
            
            // WebGL info
            if (Directory.Exists(WEBGL_BUILD_PATH))
            {
                var webglSize = GetDirectorySize(WEBGL_BUILD_PATH);
                report.AppendLine($"**WebGL Build Size**: {webglSize / (1024f * 1024f):F1} MB");
            }
            
            // Android info
            if (Directory.Exists(ANDROID_BUILD_PATH))
            {
                var androidSize = GetDirectorySize(ANDROID_BUILD_PATH);
                report.AppendLine($"**Android Build Size**: {androidSize / (1024f * 1024f):F1} MB");
            }
            
            // Addressables info
            var addressablesPath = "ServerData";
            if (Directory.Exists(addressablesPath))
            {
                var addressablesSize = GetDirectorySize(addressablesPath);
                report.AppendLine($"**Addressables Size**: {addressablesSize / (1024f * 1024f):F1} MB");
            }
            
            var reportPath = "BUILD_REPORT.md";
            File.WriteAllText(reportPath, report.ToString());
            
            Debug.Log($"[BuildPipeline] Build report saved to: {reportPath}");
        }
        
        private static long GetDirectorySize(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return 0;
                
            long size = 0;
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            
            foreach (var file in files)
            {
                size += new FileInfo(file).Length;
            }
            
            return size;
        }
        
        private static int GetVersionCode()
        {
            // Generate version code from version string
            var parts = BUILD_VERSION.Split('.');
            if (parts.Length >= 3)
            {
                if (int.TryParse(parts[0], out int major) && 
                    int.TryParse(parts[1], out int minor) && 
                    int.TryParse(parts[2], out int patch))
                {
                    return major * 10000 + minor * 100 + patch;
                }
            }
            return 1;
        }
        
        [MenuItem("Build Pipeline/Clean All Builds")]
        public static void CleanAllBuilds()
        {
            if (Directory.Exists("Builds"))
            {
                Directory.Delete("Builds", true);
                Debug.Log("[BuildPipeline] All builds cleaned");
            }
            
            AddressableAssetSettings.CleanPlayerContent();
            Debug.Log("[BuildPipeline] Addressables cleaned");
        }
        
        [MenuItem("Build Pipeline/Open Build Folder")]
        public static void OpenBuildFolder()
        {
            if (Directory.Exists("Builds"))
            {
                EditorUtility.RevealInFinder("Builds");
            }
            else
            {
                Debug.LogWarning("[BuildPipeline] Builds folder not found");
            }
        }
    }
}

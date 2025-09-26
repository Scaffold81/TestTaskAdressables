using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using Project.Core.Config.Addressable;

namespace Project.Editor.Addressable
{
    /// <summary>
    /// Editor window for configuring Addressable Groups automatically
    /// Окно редактора для автоматической настройки групп Addressables
    /// </summary>
    public class AddressableGroupsSetupWindow : EditorWindow
    {
        private AddressableGroupsConfig _groupsConfig;
        private Vector2 _scrollPosition;
        private bool _showAdvancedSettings = false;
        
        [MenuItem("Addressables/Groups Setup Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<AddressableGroupsSetupWindow>("Addressable Groups Setup");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            DrawConfigSelection();
            
            if (_groupsConfig != null)
            {
                DrawGroupsOverview();
                DrawSetupButtons();
                DrawAdvancedSettings();
            }
            
            DrawFooter();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            var style = new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            EditorGUILayout.LabelField("Addressable Groups Setup", style);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.HelpBox(
                "This tool helps configure Addressable Groups based on your project configuration. " +
                "Make sure you have AddressableGroupsConfig asset created first.",
                MessageType.Info);
                
            EditorGUILayout.Space(10);
        }
        
        private void DrawConfigSelection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Groups Config:", GUILayout.Width(100));
            
            var newConfig = EditorGUILayout.ObjectField(_groupsConfig, typeof(AddressableGroupsConfig), false) as AddressableGroupsConfig;
            if (newConfig != _groupsConfig)
            {
                _groupsConfig = newConfig;
            }
            
            if (GUILayout.Button("Create New", GUILayout.Width(80)))
            {
                CreateNewGroupsConfig();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
        }
        
        private void DrawGroupsOverview()
        {
            EditorGUILayout.LabelField("Groups Overview", EditorStyles.boldLabel);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));
            
            var groups = _groupsConfig.GetAllGroups();
            
            foreach (var group in groups)
            {
                DrawGroupInfo(group);
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(10);
        }
        
        private void DrawGroupInfo(AddressableGroupInfo group)
        {
            EditorGUILayout.BeginVertical("box");
            
            // Group name and status
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(group.groupName, EditorStyles.boldLabel);
            
            var statusColor = group.includeInBuild ? Color.green : new Color(1f, 0.5f, 0f); // orange
            var statusText = group.includeInBuild ? "LOCAL" : "REMOTE";
            
            var originalColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField(statusText, EditorStyles.miniButton, GUILayout.Width(60));
            GUI.color = originalColor;
            
            EditorGUILayout.EndHorizontal();
            
            // Group details
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Bundle Mode: {group.GetBundleModeDisplayName()}", EditorStyles.miniLabel, GUILayout.Width(180));
            EditorGUILayout.LabelField($"Compression: {group.GetCompressionDisplayName()}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            // Asset keys
            if (group.assetKeys != null && group.assetKeys.Length > 0)
            {
                EditorGUILayout.LabelField($"Assets: {group.assetKeys.Length} keys", EditorStyles.miniLabel);
                
                EditorGUI.indentLevel++;
                foreach (var key in group.assetKeys)
                {
                    EditorGUILayout.LabelField($"• {key}", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawSetupButtons()
        {
            EditorGUILayout.LabelField("Setup Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create Groups", GUILayout.Height(30)))
            {
                CreateAddressableGroups();
            }
            
            if (GUILayout.Button("Setup Profiles", GUILayout.Height(30)))
            {
                SetupAddressableProfiles();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Validate Setup", GUILayout.Height(25)))
            {
                ValidateGroupsSetup();
            }
            
            if (GUILayout.Button("Open Addressables", GUILayout.Height(25)))
            {
                EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
        }
        
        private void DrawAdvancedSettings()
        {
            _showAdvancedSettings = EditorGUILayout.Foldout(_showAdvancedSettings, "Advanced Settings");
            
            if (_showAdvancedSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);
                
                // WebGL Settings
                EditorGUILayout.LabelField("WebGL:", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Max Concurrent Requests: {_groupsConfig.webGLSettings.maxConcurrentWebRequests}");
                EditorGUILayout.LabelField($"Catalog Timeout: {_groupsConfig.webGLSettings.catalogDownloadTimeout}s");
                EditorGUI.indentLevel--;
                
                // Android Settings  
                EditorGUILayout.LabelField("Android:", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Use Asset Database: {_groupsConfig.androidSettings.useAssetDatabase}");
                EditorGUILayout.LabelField($"Simulate Groups: {_groupsConfig.androidSettings.simulateGroups}");
                EditorGUI.indentLevel--;
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawFooter()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Tip: After setting up groups, you can add assets to them by selecting assets " +
                "and using 'Make Addressable' or drag-and-drop in Addressables Groups window.",
                MessageType.Info);
        }
        
        private void CreateNewGroupsConfig()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create AddressableGroupsConfig",
                "AddressableGroupsConfig",
                "asset",
                "Create new Addressable Groups configuration");
                
            if (!string.IsNullOrEmpty(path))
            {
                var config = CreateInstance<AddressableGroupsConfig>();
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                _groupsConfig = config;
                
                Debug.Log($"[AddressableGroupsSetup] Created new groups config at: {path}");
            }
        }
        
        private void CreateAddressableGroups()
        {
            if (_groupsConfig == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a Groups Config first!", "OK");
                return;
            }
            
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not get Addressable Asset Settings!", "OK");
                return;
            }
            
            var groups = _groupsConfig.GetAllGroups();
            int createdCount = 0;
            
            foreach (var groupInfo in groups)
            {
                var existingGroup = settings.FindGroup(groupInfo.groupName);
                if (existingGroup == null)
                {
                    CreateGroup(settings, groupInfo);
                    createdCount++;
                }
                else
                {
                    Debug.Log($"[AddressableGroupsSetup] Group '{groupInfo.groupName}' already exists, skipping...");
                }
            }
            
            if (createdCount > 0)
            {
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Success", $"Created {createdCount} Addressable Groups!", "OK");
                Debug.Log($"[AddressableGroupsSetup] Successfully created {createdCount} groups");
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "All groups already exist!", "OK");
            }
        }
        
        private void CreateGroup(AddressableAssetSettings settings, AddressableGroupInfo groupInfo)
        {
            var group = settings.CreateGroup(groupInfo.groupName, false, false, true, null);
            
            // Configure BundledAssetGroupSchema
            var bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (bundledSchema != null)
            {
                // Set bundle mode
                bundledSchema.BundleMode = groupInfo.bundleMode switch
                {
                    BundlePackingMode.PackTogether => BundledAssetGroupSchema.BundlePackingMode.PackTogether,
                    BundlePackingMode.PackSeparately => BundledAssetGroupSchema.BundlePackingMode.PackSeparately,
                    BundlePackingMode.PackTogetherByLabel => BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel,
                    _ => BundledAssetGroupSchema.BundlePackingMode.PackTogether
                };
                
                // Set compression
                bundledSchema.Compression = groupInfo.compression switch
                {
                    BundleCompressionMode.None => BundledAssetGroupSchema.BundleCompressionMode.Uncompressed,
                    BundleCompressionMode.LZ4 => BundledAssetGroupSchema.BundleCompressionMode.LZ4,
                    BundleCompressionMode.LZMA => BundledAssetGroupSchema.BundleCompressionMode.LZMA,
                    _ => BundledAssetGroupSchema.BundleCompressionMode.LZ4
                };
                
                // Set include in build
                bundledSchema.IncludeInBuild = groupInfo.includeInBuild;
                
                Debug.Log($"[AddressableGroupsSetup] Created group '{groupInfo.groupName}' " +
                         $"(Mode: {groupInfo.GetBundleModeDisplayName()}, " +
                         $"Compression: {groupInfo.GetCompressionDisplayName()}, " +
                         $"Local: {groupInfo.includeInBuild})");
            }
        }
        
        private void SetupAddressableProfiles()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Error", "Addressable Asset Settings not found!", "OK");
                return;
            }
            
            // Create profiles based on our configuration
            CreateProfile(settings, "Development", "http://localhost:8080/[BuildTarget]");
            CreateProfile(settings, "Staging", "https://staging.yourcdn.com/[BuildTarget]");
            CreateProfile(settings, "Production", "https://cdn.yourprod.com/[BuildTarget]");
            
            EditorUtility.DisplayDialog("Success", "Addressable Profiles configured!", "OK");
            Debug.Log("[AddressableGroupsSetup] Addressable profiles setup completed");
        }
        
        private void CreateProfile(AddressableAssetSettings settings, string profileName, string remoteLoadPath)
        {
            var profileId = settings.profileSettings.GetProfileId(profileName);
            if (string.IsNullOrEmpty(profileId))
            {
                profileId = settings.profileSettings.AddProfile(profileName, settings.profileSettings.GetProfileId("Default"));
                Debug.Log($"[AddressableGroupsSetup] Created profile: {profileName}");
            }
            
            // Set RemoteLoadPath variable
            var remoteLoadPathId = settings.profileSettings.GetProfileDataByName("RemoteLoadPath").Id;
            if (!string.IsNullOrEmpty(remoteLoadPathId))
            {
                settings.profileSettings.SetValue(profileId, remoteLoadPathId, remoteLoadPath);
            }
            
            // Set BuildPath (same for all)
            var buildPathId = settings.profileSettings.GetProfileDataByName("BuildPath").Id;
            if (!string.IsNullOrEmpty(buildPathId))
            {
                settings.profileSettings.SetValue(profileId, buildPathId, "ServerData/[BuildTarget]");
            }
        }
        
        private void ValidateGroupsSetup()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Error", "Addressable Asset Settings not found!", "OK");
                return;
            }
            
            var groups = _groupsConfig.GetAllGroups();
            var validationResults = new System.Text.StringBuilder();
            validationResults.AppendLine("Groups Validation Results:\n");
            
            int validGroups = 0;
            
            foreach (var groupInfo in groups)
            {
                var group = settings.FindGroup(groupInfo.groupName);
                if (group != null)
                {
                    validationResults.AppendLine($"✓ {groupInfo.groupName} - EXISTS");
                    validGroups++;
                }
                else
                {
                    validationResults.AppendLine($"✗ {groupInfo.groupName} - MISSING");
                }
            }
            
            validationResults.AppendLine($"\nSummary: {validGroups}/{groups.Length} groups found");
            
            Debug.Log($"[AddressableGroupsSetup] Validation completed: {validGroups}/{groups.Length} groups found");
            EditorUtility.DisplayDialog("Validation Results", validationResults.ToString(), "OK");
        }
    }
}

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Project.Editor
{
    /// <summary>
    /// Editor utility for Addressables setup automation
    /// Утилита редактора для автоматизации настройки Addressables
    /// </summary>
    public static class AddressableSetupHelper
    {
        /// <summary>
        /// Menu item to create demo content / Пункт меню для создания демо контента
        /// </summary>
        [MenuItem("Tools/Addressables/Create Demo Content")]
        public static void CreateDemoContent()
        {
            // Create demo sprites / Создать демо спрайты
            CreateDemoSprites();
            
            // Create demo prefabs / Создать демо префабы  
            CreateDemoPrefabs();
            
            Debug.Log("[AddressableSetupHelper] Demo content created! Please assign Addressable addresses manually.");
        }
        
        /// <summary>
        /// Menu item to validate setup / Пункт меню для проверки настройки
        /// </summary>
        [MenuItem("Tools/Addressables/Validate Setup")]
        public static void ValidateSetup()
        {
            bool isValid = true;
            
            // Check if Addressables is initialized / Проверить инициализацию Addressables
            if (!UnityEngine.AddressableAssets.Addressables.RuntimePath.Contains("aa"))
            {
                Debug.LogWarning("[AddressableSetupHelper] Addressables not properly initialized!");
                isValid = false;
            }
            
            // Check folder structure / Проверить структуру папок
            string[] requiredFolders = {
                "Assets/AddressableAssets/Core",
                "Assets/AddressableAssets/Characters", 
                "Assets/AddressableAssets/Environment",
                "Assets/AddressableAssets/Effects",
                "Assets/AddressableAssets/Levels"
            };
            
            foreach (var folder in requiredFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    Debug.LogWarning($"[AddressableSetupHelper] Missing folder: {folder}");
                    isValid = false;
                }
            }
            
            if (isValid)
            {
                Debug.Log("[AddressableSetupHelper] ✅ Setup validation passed!");
            }
            else
            {
                Debug.LogError("[AddressableSetupHelper] ❌ Setup validation failed! Check warnings above.");
            }
        }
        
        /// <summary>
        /// Create demo sprites for testing / Создать демо спрайты для тестирования
        /// </summary>
        private static void CreateDemoSprites()
        {
            // Create simple colored textures for testing / Создать простые цветные текстуры для тестирования
            CreateColoredTexture("Assets/AddressableAssets/Core/ui_main_button.png", Color.blue);
            CreateColoredTexture("Assets/AddressableAssets/Effects/explosion_particle.png", Color.red);
        }
        
        /// <summary>
        /// Create demo prefabs / Создать демо префабы
        /// </summary>
        private static void CreateDemoPrefabs()
        {
            // Create simple test prefab / Создать простой тестовый префаб
            GameObject testObject = new GameObject("TestCharacter");
            testObject.AddComponent<MeshRenderer>();
            testObject.AddComponent<BoxCollider>();
            
            // Save as prefab / Сохранить как префаб
            PrefabUtility.SaveAsPrefabAsset(testObject, "Assets/AddressableAssets/Characters/characters_test_prefab.prefab");
            
            // Cleanup / Очистка
            Object.DestroyImmediate(testObject);
        }
        
        /// <summary>
        /// Create colored texture for testing / Создать цветную текстуру для тестирования
        /// </summary>
        private static void CreateColoredTexture(string path, Color color)
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Convert to PNG bytes / Конвертировать в байты PNG
            byte[] pngData = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngData);
            
            // Cleanup / Очистка
            Object.DestroyImmediate(texture);
            
            // Refresh AssetDatabase / Обновить AssetDatabase
            AssetDatabase.Refresh();
        }
    }
}
#endif
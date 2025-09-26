using UnityEngine;

namespace Project.Testing.Addressable
{
    /// <summary>
    /// Simple test prefab for demonstrating Addressable loading
    /// Простой тестовый префаб для демонстрации загрузки Addressables
    /// </summary>
    public class AddressableTestPrefab : MonoBehaviour
    {
        [Header("Test Prefab Info")]
        [SerializeField] private string prefabName = "Test Prefab";
        [SerializeField] private string description = "This is a test prefab for Addressable system";
        [SerializeField] private Color prefabColor = Color.white;
        
        [Header("Demo Components")]
        [SerializeField] private bool rotateOnStart = true;
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private bool logOnStart = true;
        
        private void Start()
        {
            if (logOnStart)
            {
                Debug.Log($"[AddressableTestPrefab] {prefabName} loaded successfully! Description: {description}");
            }
            
            // Apply color to renderer if available
            var renderer = GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = prefabColor;
            }
        }
        
        private void Update()
        {
            if (rotateOnStart)
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            }
        }
        
        /// <summary>
        /// Public method for testing interaction
        /// Публичный метод для тестирования взаимодействия
        /// </summary>
        public void OnTestInteraction()
        {
            Debug.Log($"[AddressableTestPrefab] {prefabName} interaction triggered!");
            
            // Change color randomly
            prefabColor = new Color(Random.value, Random.value, Random.value, 1f);
            
            var renderer = GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = prefabColor;
            }
        }
        
        /// <summary>
        /// Get prefab information for testing
        /// Получить информацию о префабе для тестирования
        /// </summary>
        public string GetPrefabInfo()
        {
            return $"Name: {prefabName}, Description: {description}, Color: {prefabColor}";
        }
    }
}

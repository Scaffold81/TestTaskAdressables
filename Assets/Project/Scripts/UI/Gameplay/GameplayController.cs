using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using System;
using System.Collections.Generic;
using Project.Core.Services.Addressable;
using Game.Services;

namespace Project.UI.Gameplay
{
    /// <summary>
    /// Gameplay demo controller for testing Addressable asset loading
    /// Контроллер демо геймплея для тестирования загрузки Addressable ассетов
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        [Header("Control Buttons")]
        [SerializeField] private Button loadSpriteButton;
        [SerializeField] private Button loadPrefabButton;
        [SerializeField] private Button loadCharacterButton;
        [SerializeField] private Button unloadAllButton;
        [SerializeField] private Button backToMenuButton;
        
        [Header("Progress Display")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Spawn Points")]
        [SerializeField] private Transform spriteSpawnPoint;
        [SerializeField] private Transform prefabSpawnPoint;
        [SerializeField] private Transform characterSpawnPoint;
        
        [Inject] private IAddressableService _addressableService;
        [Inject] private IGameFactory _gameFactory;
        [Inject] private ISceneManagerService _sceneManager;
        
        private readonly List<GameObject> _instantiatedObjects = new List<GameObject>();
        private bool _isLoading = false;
        
        private void Start()
        {
            SetupButtons();
            UpdateStatus("Ready");
        }
        
        private void SetupButtons()
        {
            if (loadSpriteButton != null)
                loadSpriteButton.onClick.AddListener(() => LoadSpriteAsync().Forget());
            
            if (loadPrefabButton != null)
                loadPrefabButton.onClick.AddListener(() => LoadPrefabAsync().Forget());
            
            if (loadCharacterButton != null)
                loadCharacterButton.onClick.AddListener(() => LoadCharacterAsync().Forget());
            
            if (unloadAllButton != null)
                unloadAllButton.onClick.AddListener(UnloadAllAssets);
            
            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(() => BackToMenuAsync().Forget());
        }
        
        private async UniTask LoadSpriteAsync()
        {
            if (_isLoading || spriteSpawnPoint == null) return;
            
            _isLoading = true;
            UpdateButtonsInteractability();
            
            try
            {
                UpdateStatus("Loading sprite...");
                UpdateProgress(0f);
                
                var sprite = await _addressableService.LoadAssetAsync<Sprite>("ui_main_button");
                UpdateProgress(0.5f);
                
                if (sprite != null)
                {
                    var spriteObj = new GameObject("LoadedSprite");
                    spriteObj.transform.SetParent(spriteSpawnPoint);
                    spriteObj.transform.localPosition = Vector3.zero;
                    
                    var renderer = spriteObj.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.sortingOrder = 100;
                    spriteObj.transform.localScale = Vector3.one * 2f;
                    
                    _instantiatedObjects.Add(spriteObj);
                    UpdateProgress(1f);
                    UpdateStatus("Sprite loaded!");
                }
                else
                {
                    UpdateStatus("Failed to load sprite");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                UpdateButtonsInteractability();
            }
        }
        
        private async UniTask LoadPrefabAsync()
        {
            if (_isLoading || prefabSpawnPoint == null) return;
            
            _isLoading = true;
            UpdateButtonsInteractability();
            
            try
            {
                UpdateStatus("Loading prefab...");
                UpdateProgress(0f);
                
                var prefab = await _gameFactory.InstantiateAsync("characters_test_prefab", prefabSpawnPoint);
                UpdateProgress(0.7f);
                
                if (prefab != null)
                {
                    prefab.transform.localPosition = Vector3.zero;
                    _instantiatedObjects.Add(prefab);
                    UpdateProgress(1f);
                    UpdateStatus($"Prefab loaded: {prefab.name}");
                }
                else
                {
                    CreatePlaceholderCube(prefabSpawnPoint, Color.green);
                    UpdateStatus("Placeholder created");
                }
            }
            catch (Exception)
            {
                CreatePlaceholderCube(prefabSpawnPoint, Color.green);
                UpdateStatus("Placeholder created");
            }
            finally
            {
                _isLoading = false;
                UpdateButtonsInteractability();
            }
        }
        
        private async UniTask LoadCharacterAsync()
        {
            if (_isLoading || characterSpawnPoint == null) return;
            
            _isLoading = true;
            UpdateButtonsInteractability();
            
            try
            {
                UpdateStatus("Loading character...");
                UpdateProgress(0f);
                
                var character = await _gameFactory.InstantiateAsync("character_player", characterSpawnPoint);
                UpdateProgress(0.7f);
                
                if (character != null)
                {
                    character.transform.localPosition = Vector3.zero;
                    _instantiatedObjects.Add(character);
                    UpdateProgress(1f);
                    UpdateStatus($"Character loaded: {character.name}");
                }
                else
                {
                    CreatePlaceholderCube(characterSpawnPoint, Color.blue);
                    UpdateStatus("Placeholder created");
                }
            }
            catch (Exception)
            {
                CreatePlaceholderCube(characterSpawnPoint, Color.blue);
                UpdateStatus("Placeholder created");
            }
            finally
            {
                _isLoading = false;
                UpdateButtonsInteractability();
            }
        }
        
        private void UnloadAllAssets()
        {
            try
            {
                UpdateStatus("Unloading all assets...");
                
                foreach (var obj in _instantiatedObjects)
                {
                    if (obj != null)
                    {
                        if (_gameFactory != null)
                            _gameFactory.Destroy(obj);
                        else
                            Destroy(obj);
                    }
                }
                
                _instantiatedObjects.Clear();
                _addressableService?.ReleaseAllAssets();
                
                UpdateStatus("All assets unloaded!");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
            }
        }
        
        private async UniTask BackToMenuAsync()
        {
            if (_isLoading) return;
            
            try
            {
                UpdateStatus("Returning to menu...");
                UnloadAllAssets();
                await _sceneManager.LoadSceneAsync(0);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
            }
        }
        
        private void CreatePlaceholderCube(Transform parent, Color color)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(parent);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = Vector3.one;
            
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = color;
            
            _instantiatedObjects.Add(cube);
            UpdateProgress(1f);
        }
        
        private void UpdateProgress(float progress)
        {
            if (progressBar != null)
                progressBar.value = progress;
        }
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
        
        private void UpdateButtonsInteractability()
        {
            bool canInteract = !_isLoading;
            
            if (loadSpriteButton != null)
                loadSpriteButton.interactable = canInteract;
            if (loadPrefabButton != null)
                loadPrefabButton.interactable = canInteract;
            if (loadCharacterButton != null)
                loadCharacterButton.interactable = canInteract;
            if (backToMenuButton != null)
                backToMenuButton.interactable = canInteract;
        }
        
        private void OnDestroy()
        {
            UnloadAllAssets();
        }
    }
}

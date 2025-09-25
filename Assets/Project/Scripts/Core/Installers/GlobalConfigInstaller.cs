using Game.Config;
using Project.Core.Config.Addressable;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    /// <summary>
    /// Инсталлер глобальных конфигураций. Отвечает за регистрацию ScriptableObject конфигов и их репозиториев.
    /// Global configurations installer. Responsible for registering ScriptableObject configs and their repositories.
    /// </summary>
    public class GlobalConfigInstaller : MonoInstaller
    {
        [Header("Audio Configuration")]
        [SerializeField] private AudioConfig audioConfig;
        
        [Header("Addressable Configuration")]
        [SerializeField] private AddressableConfig addressableConfig;
        
        /// <summary>
        /// Основной метод инсталляции зависимостей.
        /// Main dependency installation method.
        /// </summary>
        public override void InstallBindings()
        {
            BindConfigs();
            BindRepositories();
        }
        
        /// <summary>
        /// Привязывает ScriptableObject конфигурации.
        /// Binds ScriptableObject configurations.
        /// </summary>
        private void BindConfigs()
        {
            // Bind ScriptableObject configs
            if (audioConfig != null)
            {
                Container.Bind<AudioConfig>().FromInstance(audioConfig).AsSingle();
                // Audio config bound successfully
            }
            else
            {
                Debug.LogError("[GlobalConfigInstaller] Audio config is not assigned! Please assign it in the inspector.");
            }
            
            // Bind Addressable config
            if (addressableConfig != null)
            {
                Container.Bind<AddressableConfig>().FromInstance(addressableConfig).AsSingle();
                Debug.Log("[GlobalConfigInstaller] Addressable config bound successfully");
            }
            else
            {
                Debug.LogError("[GlobalConfigInstaller] Addressable config is not assigned! Please assign it in the inspector.");
            }
        }
        
        /// <summary>
        /// Привязывает репозитории конфигураций.
        /// Binds configuration repositories.
        /// </summary>
        private void BindRepositories()
        {
            // Bind config repositories
            Container.Bind<IAudioConfigRepository>().To<AudioConfigRepository>().AsSingle();
            
            // Bind Addressable config repository
            Container.Bind<IAddressableConfigRepository>().To<AddressableConfigRepository>().AsSingle();
            
            Debug.Log("[GlobalConfigInstaller] Config repositories bound successfully");
        }
    }
}
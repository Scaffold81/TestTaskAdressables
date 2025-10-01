using Project.Core.Services.Addressable;
using Project.Core.Services.Addressable.Memory;
using Project.Core.Services.Loading;
using Game.Services;
using UnityEngine;
using Zenject;

namespace Project.Core.Installers
{
    /// <summary>
    /// Main project services installer. Registers all core game services.
    /// Основной инсталлер сервисов проекта. Регистрирует все основные сервисы игры.
    /// </summary>
    public class ProjectServiceInstaller : MonoInstaller
    {
        /// <summary>
        /// Main dependency installation method.
        /// Основной метод инсталляции зависимостей.
        /// </summary>
        public override void InstallBindings()
        {
            BindCoreServices();
            BindAddressableServices();
            BindUtilityServices();
            
            Debug.Log("[ProjectServiceInstaller] All services installed successfully");
        }
        
        /// <summary>
        /// Binds core game services.
        /// Привязывает основные игровые сервисы.
        /// </summary>
        private void BindCoreServices()
        {
            // Scene Management
            Container.Bind<ISceneManagerService>().To<SceneManagerService>().AsSingle();
            
            // Factory Services
            Container.Bind<IGameFactory>().To<GameFactory>().AsSingle();
            
            // Object Pooling
            Container.Bind<IObjectPoolService>().To<ObjectPoolService>().AsSingle();
            
            // Audio Service (depends on IAudioConfigRepository from GlobalConfigInstaller)
            Container.Bind<IAudioService>().To<AudioService>().AsSingle();
            
            // UI Management
            Container.Bind<IUIPageService>().To<UIPageService>().AsSingle();
            
            // Save System
            Container.Bind<ISaveService>().To<SaveService>().AsSingle();
            
            Debug.Log("[ProjectServiceInstaller] Core services bound");
        }
        
        /// <summary>
        /// Binds Addressable services.
        /// Привязывает сервисы Addressables.
        /// </summary>
        private void BindAddressableServices()
        {
            // NOTE: IAddressableConfigRepository is bound in GlobalConfigInstaller
            
            // Bind memory manager
            Container.Bind<IAddressableMemoryManager>().To<AddressableMemoryManager>().AsSingle();
            
            // Bind catalog manager
            Container.Bind<ICatalogManager>().To<CatalogManager>().AsSingle();
            
            // Bind main Addressable service
            Container.Bind<IAddressableService>().To<AddressableService>().AsSingle().NonLazy();
            
            // Bind Loading service
            Container.Bind<ILoadingService>().To<LoadingService>().AsSingle();
            
            Debug.Log("[ProjectServiceInstaller] Addressable services bound");
        }
        
        /// <summary>
        /// Binds utility services.
        /// Привязывает вспомогательные сервисы.
        /// </summary>
        private void BindUtilityServices()
        {
            // Player Data Service
            Container.Bind<IPlayerDataService>().To<PlayerDataService>().AsSingle();
            
            // Settings Service
            Container.Bind<ISettingsService>().To<SettingsService>().AsSingle();
            
            // Localization Service
            Container.Bind<ILocalizationService>().To<UnityLocalizationService>().AsSingle();
            
            Debug.Log("[ProjectServiceInstaller] Utility services bound");
        }
    }
}

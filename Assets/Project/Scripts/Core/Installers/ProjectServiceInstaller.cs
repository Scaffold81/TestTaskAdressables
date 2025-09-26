using Game.Services;
using Project.Core.Services.Addressable;
using Project.Core.Services.Loading;
using Project.Core.Services.Integration;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    /// <summary>
    /// Основной инсталлер сервисов проекта. Регистрирует все основные сервисы игры.
    /// Main project services installer. Registers all core game services.
    /// </summary>
    public class ProjectServiceInstaller : MonoInstaller
    {
        /// <summary>
        /// Основной метод инсталляции зависимостей.
        /// Main dependency installation method.
        /// </summary>
        public override void InstallBindings()
        {
            BindCoreServices();
            BindAddressableServices();
        }

        /// <summary>
        /// Привязывает основные сервисы игры.
        /// Binds core game services.
        /// </summary>
        private void BindCoreServices()
        {
            // Base Services (no dependencies)
            Container.Bind<ISaveService>().To<SaveService>().AsSingle();
            Container.Bind<IObjectPoolService>().To<ObjectPoolService>().AsSingle();
            Container.Bind<IGameFactory>().To<GameFactory>().AsSingle();
            Container.Bind<ISettingsService>().To<SettingsService>().AsSingle();
            
            // Services with dependencies
            Container.Bind<IPlayerDataService>().To<PlayerDataService>().AsSingle();
            Container.Bind<IAudioService>().To<AudioService>().AsSingle().NonLazy();
            Container.Bind<ILocalizationService>().To<UnityLocalizationService>().AsSingle();
            Container.Bind<IUIPageService>().To<UIPageService>().AsSingle();
            Container.Bind<ISceneManagerService>().To<SceneManagerService>().AsSingle();
        }
        
        /// <summary>
        /// Привязывает сервисы Addressables.
        /// Binds Addressable services.
        /// </summary>
        private void BindAddressableServices()
        {
            // Bind catalog manager
            Container.Bind<ICatalogManager>().To<CatalogManager>().AsSingle();
            
            // Bind main Addressable service
            Container.Bind<IAddressableService>().To<AddressableService>().AsSingle().NonLazy();
            
            // Bind Loading service
            Container.Bind<ILoadingService>().To<LoadingService>().AsSingle();
            
            // Bind integration layer
            Container.Bind<AddressableLoadingIntegration>().AsSingle().NonLazy();
            
            Debug.Log("[ProjectServiceInstaller] Addressable services bound successfully");
        }
    }
}
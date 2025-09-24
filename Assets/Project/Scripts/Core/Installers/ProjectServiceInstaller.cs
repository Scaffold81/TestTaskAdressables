using Game.Services;
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
    }
}
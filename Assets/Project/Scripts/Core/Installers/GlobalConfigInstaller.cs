using Game.Config;
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
        }
        
        /// <summary>
        /// Привязывает репозитории конфигураций.
        /// Binds configuration repositories.
        /// </summary>
        private void BindRepositories()
        {
            // Bind config repositories
            Container.Bind<IAudioConfigRepository>().To<AudioConfigRepository>().AsSingle();
            
            // Config repositories bound successfully
        }
    }
}
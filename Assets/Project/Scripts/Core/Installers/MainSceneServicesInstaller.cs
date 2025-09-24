using UnityEngine;
using Zenject;

namespace Game.Installers
{
    /// <summary>
    /// Инсталлер сервисов для главной сцены. Может содержать сервисы, специальные для основного геймплея.
    /// Main scene services installer. Can contain services specific to main gameplay.
    /// </summary>
    public class MainSceneServicesInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
        }
    }
}
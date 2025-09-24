using UnityEngine;
using Zenject;

namespace Game.Installers
{
    /// <summary>
    /// Инсталлер локальных конфигураций. Может использоваться для конфигов, специальных для конкретной сцены.
    /// Local configurations installer. Can be used for configs specific to a particular scene.
    /// </summary>
    public class LocalConfigsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
        }
    }
}
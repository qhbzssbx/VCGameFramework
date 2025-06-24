using Game.Modules.Resource.Domain;
using UnityEngine;


namespace Game.Modules.Resource.Infrastructure
{
    [CreateAssetMenu(fileName = "ResourceConfig", menuName = "Game/ResourceConfig", order = 0)]
    public class ScriptableResourceConfig : ScriptableObject
    {
        public string RemoteURL;
        public string PackageName = "DefaultPackage";
        public bool EnableHotUpdate = true;

        public ResourceConfig ToConfig()
        {
            return new ResourceConfig
            {
                RemoteURL = RemoteURL,
                PackageName = PackageName,
                EnableHotUpdate = EnableHotUpdate
            };
        }
    }
}


using System.Collections.Generic;
using Core.Actors.Modules.Abstract;
using Core.Actors.Traits;
using UnityEngine;

namespace Core.Actors.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Actor", fileName = "Actor")]
    public sealed class ActorConfig : ScriptableObject
    {
        [SerializeField] private Actor prefab;
        [SerializeField] private List<ActorTrait> traits = new();
        [SerializeField] private List<ActorModuleConfig> modules = new();
        [SerializeField] private float mass = 1f;
        [SerializeField] private float spawnHeight = 0.5f;

        public Actor Prefab => prefab;
        public IReadOnlyList<ActorModuleConfig> Modules => modules;
        public float Mass => mass;
        public float SpawnHeight => spawnHeight;

        public bool HasTrait<T>() where T : class
        {
            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i] is T)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

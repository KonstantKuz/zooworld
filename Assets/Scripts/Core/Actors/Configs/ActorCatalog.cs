using System.Collections.Generic;
using UnityEngine;

namespace Core.Actors.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Actor Catalog", fileName = "ActorCatalog")]
    public sealed class ActorCatalog : ScriptableObject
    {
        [SerializeField] private List<ActorConfig> actors = new();

        public IReadOnlyList<ActorConfig> Actors => actors;

        public ActorConfig GetRandom()
        {
            return actors[Random.Range(0, actors.Count)];
        }
    }
}

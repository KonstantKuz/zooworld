using Core.Actors.Traits;
using Game.Actors.Traits.Abstract;
using UnityEngine;

namespace Game.Actors.Traits
{
    [CreateAssetMenu(menuName = "ZooWorld/Traits/Predator", fileName = "PredatorTrait")]
    public sealed class PredatorTrait : ActorTrait, IPredator
    {
    }
}

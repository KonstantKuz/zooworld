using Core.Actors.Traits;
using Game.Actors.Traits.Abstract;
using UnityEngine;

namespace Game.Actors.Traits
{
    [CreateAssetMenu(menuName = "ZooWorld/Traits/Prey", fileName = "PreyTrait")]
    public sealed class PreyTrait : ActorTrait, IPrey
    {
    }
}

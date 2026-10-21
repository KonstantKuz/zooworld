using Core.Actors.Modules.Abstract;
using Core.Actors.Modules;
using Game.Actors.Modules.Visual;
using UnityEngine;

namespace Game.Actors.Modules.Visual.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Modules/Visual", fileName = "VisualModule")]
    public sealed class VisualModuleConfig : ActorModuleConfig
    {
        [Tooltip("How far the color moves towards a tint: 0 not at all, 1 fully.")]
        [SerializeField, Range(0f, 1f)] private float tintStrength = 0.5f;

        public override IActorModule Create(in ActorContext context)
        {
            return new VisualModule(context.GetInChildren<Renderer>(), tintStrength);
        }
    }
}

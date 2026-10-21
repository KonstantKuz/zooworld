using System;
using UnityEngine;

namespace Game.Navigation.Configs
{
    [Serializable]
    public sealed class NavigationSettings
    {
        [Tooltip("The NavMesh keeps this far from obstacles: enough for the long snake body to turn without snagging.")]
        [SerializeField, Min(0.1f)] private float agentRadius = 0.9f;
        [Tooltip("Side of the square area (centered at the origin) the NavMesh is built over.")]
        [SerializeField, Min(1f)] private float areaSize = 60f;

        public float AgentRadius => agentRadius;
        public float AreaSize => areaSize;
    }
}

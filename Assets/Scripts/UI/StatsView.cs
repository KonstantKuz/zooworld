using Game.Stats;
using R3;
using UnityEngine.UI;
using UnityEngine;
using Zenject;

namespace UI
{
    public sealed class StatsView : MonoBehaviour
    {
        [SerializeField] private Text preyText;
        [SerializeField] private Text predatorsText;

        private KillStatsService stats;

        [Inject]
        private void Construct(KillStatsService stats)
        {
            this.stats = stats;
        }

        private void Start()
        {
            stats.DeadPrey.Subscribe(count => preyText.text = $"Dead prey: {count}").AddTo(this);
            stats.DeadPredators.Subscribe(count => predatorsText.text = $"Dead predators: {count}").AddTo(this);
        }
    }
}

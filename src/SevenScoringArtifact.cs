using System;
using Aotenjo;

namespace ModenjoASM
{
    /// <summary>
    /// Unremovable run artifact. It is granted by the round-start hook, occupies
    /// one inventory slot, and grants one additional slot so the host run is not
    /// squeezed by the 7nnkmj rules.
    ///
    /// This is a C# artifact because the Lua API cannot override sell/remove
    /// behavior or reliably reserve capacity.
    /// </summary>
    public sealed class SevenScoringArtifact : Artifact
    {
        private bool _slotGranted;

        public SevenScoringArtifact()
            : base(Bootstrap.ArtifactId, Rarity.COMMON)
        {
        }

        public override bool CanBeSellByPlayer() => false;

        public override bool CanBeSold(Player player) => false;

        public override string GetName(Func<string, string> localizer) => "七子计分核心";

        public override string GetDescription(Func<string, string> localizer)
            => "不可出售。启用七子麻将有役与赋位计分；占用 1 格，并额外提供 1 格遗物槽。";

        public override void OnObtain(Player player)
        {
            base.OnObtain(player);
            if (!_slotGranted)
            {
                _slotGranted = true;
                player.ChangeArtifactLimit(1);
            }
        }

        public override void OnRemoved(Player player)
        {
            if (_slotGranted)
            {
                _slotGranted = false;
                player.ChangeArtifactLimit(-1);
            }

            base.OnRemoved(player);
        }

        public static void ApplyScore(Player? player, Permutation? permutation)
        {
            if (player == null || permutation == null)
            {
                return;
            }

            var score = ModenjoASM.SevenYaku.SevenYakuRegistry.Evaluate(permutation, player);
            int points = score.Points;
            UnityEngine.Debug.Log($"[7nnkmj] settle fu={score.Fu} points={points} old={player.RoundAccumulatedScore?.GetScore()}");

            // Aotenjo already added RoundAccumulatedScore to CurrentAccumulatedScore;
            // replace that contribution with the 7nnkmj point-table result.
            double old = player.RoundAccumulatedScore?.GetScore() ?? 0.0;
            player.CurrentAccumulatedScore -= Math.Floor(old);
            player.RoundAccumulatedScore = new Score(points, 1.0);
            player.CurrentAccumulatedScore += points;
        }
    }
}

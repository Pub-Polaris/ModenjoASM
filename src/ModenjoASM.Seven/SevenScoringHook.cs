using Aotenjo;
using UnityEngine;

namespace ModenjoASM.Seven
{
    /// <summary>
    /// Integrates the 7nnkmj fu/fan table into Aotenjo's normal score:
    ///
    ///   Aotenjo Fu  += 7nnkmj Fu
    ///   Aotenjo Fan += 7nnkmj Fu / 7  (because the original rule is 7n fu = n fan)
    ///
    /// This is a module-level scoring hook, not an artifact.
    /// </summary>
    internal static class SevenScoringHook
    {
        private static bool _registered;

        public static void Register()
        {
            if (_registered)
            {
                return;
            }

            _registered = true;

            EventBus.Subscribe<PlayerRoundEvent.Start.Prepare>(
                evt => GrantAll(evt?.player), priority: -100);
            EventBus.Subscribe<PlayerRoundEvent.Start.Pre>(
                evt => GrantAll(evt?.player), priority: -100);
            EventBus.Subscribe<PlayerRoundEvent.Start.Post>(
                evt => GrantAll(evt?.player), priority: -100);
            EventBus.Subscribe<PlayerEvents.PreAddTileEvent>(
                evt => GrantAll(evt?.player), priority: -100);
            EventBus.Subscribe<PlayerEvents.PostAddTileEvent>(
                evt => GrantAll(evt?.player), priority: -100);
            EventBus.Subscribe<PlayerEvents.PostSettlePermutationEvent>(
                evt => ApplyScore(evt?.player, evt?.permutation), priority: 10_000);
        }

        private static void GrantAll(Player? player)
        {
            if (player != null)
            {
                SevenYaku.SevenYakuRegistry.GrantAll(player);
            }
        }

        public static void ApplyScore(Player? player, Permutation? permutation)
        {
            if (player == null || permutation == null)
            {
                return;
            }

            var seven = SevenYaku.SevenYakuRegistry.Evaluate(permutation, player);
            if (seven.Fu <= 0)
            {
                return;
            }

            Score? old = player.RoundAccumulatedScore;
            double oldScore = old?.GetScore() ?? 0.0;
            double oldFan = old?.GetFan() ?? 0.0;
            double oldFu = old?.GetFu() ?? 0.0;

            double sevenFan = seven.Fu / 7.0;
            var updated = new Score(oldFan + sevenFan, oldFu + seven.Fu);

            player.RoundAccumulatedScore = updated;
            player.CurrentAccumulatedScore += updated.GetScore() - oldScore;

            Debug.Log($"[7nnkmj] scoring: fu+={seven.Fu} fan+={sevenFan:0.###} "
                + $"-> fu={updated.GetFu():0.###} fan={updated.GetFan():0.###}");
        }
    }
}

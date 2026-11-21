using Aotenjo;
using UnityEngine;

namespace ModenjoASM.Seven
{
    /// <summary>
    /// Registers the example artifact and wires its round/settle hooks.
    /// All game interaction goes through Aotenjo's public API and EventBus.
    /// </summary>
    internal static class SevenArtifactRegistration
    {
        private static bool _registered;

        public static void Register()
        {
            if (_registered)
            {
                return;
            }

            _registered = true;
            ArtifactRegistry.Register(
                SevenModule.ArtifactId,
                () => new SevenScoringArtifact(),
                SevenModule.ModId);

            EventBus.Subscribe<PlayerRoundEvent.Start.Prepare>(
                evt => EnsureArtifact(evt?.player), priority: -100);
            EventBus.Subscribe<PlayerRoundEvent.Start.Pre>(
                evt => EnsureArtifact(evt?.player), priority: -100);
            EventBus.Subscribe<PlayerRoundEvent.Start.Post>(
                evt => EnsureArtifact(evt?.player), priority: -100);
            EventBus.Subscribe<PlayerEvents.PreAddTileEvent>(
                evt => EnsureArtifact(evt?.player), priority: -100);
            EventBus.Subscribe<PlayerEvents.PostAddTileEvent>(
                evt => EnsureArtifact(evt?.player), priority: -100);
            EventBus.Subscribe<PlayerEvents.PostSettlePermutationEvent>(
                evt => SevenScoringArtifact.ApplyScore(evt?.player, evt?.permutation), priority: 10_000);
        }

        private static void EnsureArtifact(Player? player)
        {
            if (player == null)
            {
                return;
            }

            foreach (var artifact in player.GetArtifacts())
            {
                if (artifact != null && artifact.GetRegName() == SevenModule.ArtifactId)
                {
                    return;
                }
            }

            player.ObtainArtifact(new SevenScoringArtifact(), forced: true);
            SevenYaku.SevenYakuRegistry.GrantAll(player);
            Debug.Log("[7nnkmj] scoring artifact granted");
        }
    }
}

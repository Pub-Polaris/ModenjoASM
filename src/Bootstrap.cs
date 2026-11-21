using System;
using System.Collections.Generic;

namespace ModenjoASM
{
    /// <summary>
    /// Runtime bootstrap for the 7nnkmj port.
    /// It is intentionally small: the game's Lua mod entry can load this assembly
    /// and call <see cref="Init"/> without patching Assembly-CSharp.dll.
    /// </summary>
    public sealed class Bootstrap
    {
        public const string ModId = "7nnkmj";
        public const string ArtifactId = "7nnkmj:scoring_artifact";

        private static bool _initialized;

        public void Init()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            ModenjoASM.SevenYaku.SevenYakuRegistry.RegisterAll();
            SevenArtifactRegistration.Register();

            var versionTag = new UnityEngine.GameObject("ModenjoASM.VersionTag");
            versionTag.AddComponent<ModenjoAsmVersionTag>();

            UnityEngine.Debug.Log("[7nnkmj] ModenjoASM initialized");
        }
    }

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
            Aotenjo.ArtifactRegistry.Register(
                Bootstrap.ArtifactId,
                () => new SevenScoringArtifact(),
                Bootstrap.ModId);

            Aotenjo.EventBus.Subscribe<Aotenjo.PlayerRoundEvent.Start.Prepare>(
                evt => EnsureArtifact(evt?.player), priority: -100);
            Aotenjo.EventBus.Subscribe<Aotenjo.PlayerRoundEvent.Start.Pre>(
                evt => EnsureArtifact(evt?.player), priority: -100);
            Aotenjo.EventBus.Subscribe<Aotenjo.PlayerRoundEvent.Start.Post>(
                evt => EnsureArtifact(evt?.player), priority: -100);
            Aotenjo.EventBus.Subscribe<Aotenjo.PlayerEvents.PreAddTileEvent>(
                evt => EnsureArtifact(evt?.player), priority: -100);
            Aotenjo.EventBus.Subscribe<Aotenjo.PlayerEvents.PostAddTileEvent>(
                evt => EnsureArtifact(evt?.player), priority: -100);
            Aotenjo.EventBus.Subscribe<Aotenjo.PlayerEvents.PostSettlePermutationEvent>(
                evt => SevenScoringArtifact.ApplyScore(evt?.player, evt?.permutation), priority: 10_000);
        }

        private static void EnsureArtifact(Aotenjo.Player? player)
        {
            if (player == null)
            {
                return;
            }

            UnityEngine.Debug.Log($"[7nnkmj] artifact check, held={player.GetArtifacts().Count}");
            foreach (var artifact in player.GetArtifacts())
            {
                if (artifact != null && artifact.GetRegName() == Bootstrap.ArtifactId)
                {
                    return;
                }
            }

            player.ObtainArtifact(new SevenScoringArtifact(), forced: true);
            ModenjoASM.SevenYaku.SevenYakuRegistry.GrantAll(player);
            UnityEngine.Debug.Log("[7nnkmj] scoring artifact granted");
        }
    }
}

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

            Aotenjo.EventBus.Subscribe<Aotenjo.PlayerRoundEvent.Start.Prepare>(EnsureArtifact, priority: -100);
        }

        private static void EnsureArtifact(Aotenjo.PlayerRoundEvent.Start.Prepare evt)
        {
            if (evt == null || evt.player == null)
            {
                return;
            }

            var player = evt.player;
            foreach (var artifact in player.GetArtifacts())
            {
                if (artifact != null && artifact.GetRegName() == Bootstrap.ArtifactId)
                {
                    return;
                }
            }

            player.ObtainArtifact(new SevenScoringArtifact(), forced: true);
            ModenjoASM.SevenYaku.SevenYakuRegistry.GrantAll(player);
        }
    }
}

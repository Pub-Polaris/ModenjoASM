using UnityEngine;

namespace ModenjoASM.Seven
{
    /// <summary>
    /// Example module: the 7nnkmj yaku table and its scoring integration.
    /// No artifact is registered; the module only adds Fu/Fan through the
    /// normal Aotenjo score pipeline.
    /// </summary>
    public sealed class SevenModule : IModenjoModule
    {
        public const string ModId = "7nnkmj";

        public string Id => ModId;

        public void Initialize()
        {
            SevenYaku.SevenYakuRegistry.RegisterAll();
            SevenScoringHook.Register();
            Debug.Log("[7nnkmj] example module initialized (no artifact)");
        }
    }
}

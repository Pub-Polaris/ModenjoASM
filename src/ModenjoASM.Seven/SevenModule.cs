using UnityEngine;

namespace ModenjoASM.Seven
{
    /// <summary>
    /// Example module: the 7nnkmj yaku table and its scoring artifact.
    /// It is deliberately self-contained and can be removed without breaking
    /// the host or other modules.
    /// </summary>
    public sealed class SevenModule : IModenjoModule
    {
        public const string ModId = "7nnkmj";
        public const string ArtifactId = "7nnkmj:scoring_artifact";

        public string Id => ModId;

        public void Initialize()
        {
            SevenYaku.SevenYakuRegistry.RegisterAll();
            SevenArtifactRegistration.Register();
            Debug.Log("[7nnkmj] example module initialized");
        }
    }
}

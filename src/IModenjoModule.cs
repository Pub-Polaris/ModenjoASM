namespace ModenjoASM
{
    /// <summary>
    /// Contract for a self-contained ModenjoASM extension module.
    /// A module is loaded after the host and receives no game objects
    /// directly; it uses Aotenjo's public API and EventBus.
    /// </summary>
    public interface IModenjoModule
    {
        string Id { get; }

        void Initialize();
    }
}

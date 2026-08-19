namespace ImmunologyTD.Platform
{
    /// <summary>
    /// Placeholder for Steam integration. Sprint 0 scope is app-ID plumbing
    /// only -- no real Steamworks SDK dependency yet, so this compiles
    /// clean with no external packages. Next step (not Sprint 0): pick
    /// Steamworks.NET or Facepunch.Steamworks, add it via the Package
    /// Manager (needs network access, so it's an Editor-GUI/Director step,
    /// same as the build steps), and replace this stub's body with the
    /// real SDK init call. Record the choice in ENGINE_STATUS.md when it
    /// happens.
    /// </summary>
    public static class SteamStub
    {
        // Placeholder App ID -- replace with the real one once a
        // Steamworks account/app page exists. 480 is Valve's public test
        // App ID, safe to leave here as an obvious "not real yet" marker.
        public const uint AppId = 480;

        public static bool TryInitialize()
        {
            UnityEngine.Debug.Log(
                "[SteamStub] Steam integration not wired up yet (Sprint 0 scope was app-ID plumbing only).");
            return false;
        }
    }
}

namespace ImmunologyTD.Economy
{
    /// <summary>
    /// A one-line static bridge from the deep-in-the-combat-code kill event
    /// (<c>SearchUnit.RegisterKill</c>) to the wallet, without threading an
    /// <see cref="AtpWallet"/> reference through <c>BoneMarrowManager</c> →
    /// <c>SearchUnit</c> → every emitted unit purely so a kill can add 3 ATP.
    /// Same pattern the project already uses for shared presentation /
    /// input services (<c>DegranulationFlash.Configure</c>,
    /// <c>CytokineToggle.Enabled</c>, <c>RuntimeSprites</c>).
    ///
    /// <c>GameBootstrap</c> sets <see cref="PayForKill"/> in Awake; a
    /// headless harness leaves it null (kills then simply pay nothing, which
    /// is what the lifecycle/combat harnesses want) or points it at a test
    /// wallet.
    /// </summary>
    public static class EconomyHooks
    {
        /// <summary>Invoked once per pathogen a unit kills. Null = no
        /// economy wired (harness default).</summary>
        public static System.Action PayForKill;

        public static void ReportKill() => PayForKill?.Invoke();
    }
}

namespace TowerWar.Core
{
    /// <summary>
    /// Carries the selected match mode between the menu and the gameplay scene.
    /// The default makes opening Level1 directly a playable solo match.
    /// </summary>
    public static class MatchSession
    {
        public static MatchMode Mode { get; private set; } = MatchMode.HumanVsComputer;

        public static void Configure(MatchMode mode)
        {
            Mode = mode;
        }

        public static void Reset()
        {
            Mode = MatchMode.HumanVsComputer;
        }
    }
}

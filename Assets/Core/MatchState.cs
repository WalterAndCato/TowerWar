using System;

namespace TowerWar.Core
{
    public sealed class MatchState
    {
        public int CurrentPlayerIndex { get; private set; }
        public MatchPhase Phase { get; private set; }
        public int? WinnerIndex { get; private set; }

        public bool IsFinished => Phase == MatchPhase.Finished;

        public MatchState(int startingPlayerIndex = 0)
        {
            if (startingPlayerIndex is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(startingPlayerIndex));

            CurrentPlayerIndex = startingPlayerIndex;
            Phase = MatchPhase.SelectingBlock;
        }

        public bool CanPlayerAct(int playerIndex)
        {
            return !IsFinished && CurrentPlayerIndex == playerIndex;
        }

        public void SelectBlock()
        {
            EnsureNotFinished();
            Phase = MatchPhase.Aiming;
        }

        public void BeginShot()
        {
            EnsureNotFinished();

            if (Phase != MatchPhase.Aiming)
                throw new InvalidOperationException("A block must be selected before shooting.");

            Phase = MatchPhase.ProjectileInFlight;
        }

        public void CompleteShot()
        {
            EnsureNotFinished();

            if (Phase != MatchPhase.ProjectileInFlight)
                throw new InvalidOperationException("No shot is currently in flight.");

            CurrentPlayerIndex = CurrentPlayerIndex == 0 ? 1 : 0;
            Phase = MatchPhase.SelectingBlock;
        }

        public void Finish(int winnerIndex)
        {
            if (winnerIndex is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(winnerIndex));

            if (IsFinished)
                return;

            WinnerIndex = winnerIndex;
            Phase = MatchPhase.Finished;
        }

        private void EnsureNotFinished()
        {
            if (IsFinished)
                throw new InvalidOperationException("The match has already finished.");
        }
    }
}

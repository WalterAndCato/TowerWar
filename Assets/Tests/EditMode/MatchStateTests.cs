using System;
using NUnit.Framework;
using TowerWar.Core;

public class MatchStateTests
{
    [Test]
    public void NewMatch_StartsWithPlayerOneSelectingBlock()
    {
        var state = new MatchState();

        Assert.That(state.CurrentPlayerIndex, Is.Zero);
        Assert.That(state.Phase, Is.EqualTo(MatchPhase.SelectingBlock));
        Assert.That(state.CanPlayerAct(0), Is.True);
        Assert.That(state.CanPlayerAct(1), Is.False);
    }

    [Test]
    public void CompletedShot_SwitchesPlayerAndReturnsToSelection()
    {
        var state = new MatchState();
        state.SelectBlock();
        state.BeginShot();

        state.CompleteShot();

        Assert.That(state.CurrentPlayerIndex, Is.EqualTo(1));
        Assert.That(state.Phase, Is.EqualTo(MatchPhase.SelectingBlock));
    }

    [Test]
    public void ShotWithoutSelectedBlock_IsRejected()
    {
        var state = new MatchState();

        Assert.Throws<InvalidOperationException>(() => state.BeginShot());
    }

    [Test]
    public void FinishedMatch_StoresWinnerAndRejectsFurtherActions()
    {
        var state = new MatchState();

        state.Finish(1);

        Assert.That(state.IsFinished, Is.True);
        Assert.That(state.WinnerIndex, Is.EqualTo(1));
        Assert.That(state.CanPlayerAct(0), Is.False);
        Assert.Throws<InvalidOperationException>(() => state.SelectBlock());
    }

    [TestCase(-1)]
    [TestCase(2)]
    public void InvalidStartingPlayer_IsRejected(int playerIndex)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MatchState(playerIndex));
    }
}

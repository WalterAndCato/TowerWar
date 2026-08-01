using NUnit.Framework;
using TowerWar.Core;
using UnityEngine;

public class ShotCalculatorTests
{
    [Test]
    public void ZeroDegrees_ProducesRightFacingShot()
    {
        var velocity = ShotCalculator.CalculateVelocity(0f, 0.5f, 100f, 0.2f);

        Assert.That(velocity.x, Is.EqualTo(70f).Within(0.001f));
        Assert.That(velocity.y, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void Charge_IsClampedToValidRange()
    {
        var overcharged = ShotCalculator.CalculateVelocity(90f, 2f, 100f, 0.2f);
        var negativeCharge = ShotCalculator.CalculateVelocity(90f, -1f, 100f, 0.2f);

        Assert.That(overcharged.magnitude, Is.EqualTo(120f).Within(0.001f));
        Assert.That(negativeCharge.magnitude, Is.EqualTo(20f).Within(0.001f));
    }

    [Test]
    public void OneHundredEightyDegrees_ProducesLeftFacingShot()
    {
        var velocity = ShotCalculator.CalculateVelocity(180f, 0.5f, 100f, 0.2f);

        Assert.That(velocity.x, Is.EqualTo(-70f).Within(0.001f));
        Assert.That(velocity.y, Is.EqualTo(0f).Within(0.001f));
    }
}

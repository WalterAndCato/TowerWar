using UnityEngine;

namespace TowerWar.Core
{
    public static class ShotCalculator
    {
        public static Vector2 CalculateVelocity(
            float angleDegrees,
            float charge,
            float maximumForce,
            float minimumCharge = 0.2f)
        {
            var clampedCharge = Mathf.Clamp01(charge);
            var angleRadians = angleDegrees * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
            return direction * (clampedCharge + minimumCharge) * maximumForce;
        }
    }
}

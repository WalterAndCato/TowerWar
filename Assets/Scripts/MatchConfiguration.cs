using UnityEngine;

[CreateAssetMenu(fileName = "MatchConfiguration", menuName = "TowerWar/Match Configuration")]
public class MatchConfiguration : ScriptableObject
{
    [Header("Shot")]
    [Min(1f)] public float maximumShotForce = 10000f;
    [Range(0f, 1f)] public float minimumShotCharge = 0.2f;
    [Min(1f)] public float rotationSpeed = 90f;

    [Header("Computer player")]
    [Min(0f)] public float computerThinkTime = 0.8f;
    [Range(0f, 1f)] public float computerMinimumCharge = 0.55f;
    [Range(0f, 1f)] public float computerMaximumCharge = 0.9f;
    [Range(0f, 30f)] public float computerAimVariation = 8f;

    private void OnValidate()
    {
        computerMaximumCharge = Mathf.Max(computerMinimumCharge, computerMaximumCharge);
    }
}

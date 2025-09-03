using UnityEngine;

[System.Serializable]
public class DetectedVictimData
{
    public GameObject VictimGameObject;
    public ConsciousState State;
    public enum ConsciousState { Unconscious, Conscious }
    public float Distance;

    public DetectedVictimData(GameObject victim, float distance, ConsciousState state)
    {
        VictimGameObject = victim;
        Distance = distance;
        State = state;
    }
    public string QuickStatus() =>
        State == ConsciousState.Unconscious ? "Bất tỉnh" : "Còn tỉnh táo";
}
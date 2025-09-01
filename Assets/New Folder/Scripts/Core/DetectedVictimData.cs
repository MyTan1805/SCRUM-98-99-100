using UnityEngine;

[System.Serializable]
public class DetectedVictimData
{
    public GameObject VictimGameObject;
    public float Distance;

    // Constructor để dễ dàng tạo đối tượng mới
    public DetectedVictimData(GameObject victim, float distance)
    {
        VictimGameObject = victim;
        Distance = distance;
    }
}
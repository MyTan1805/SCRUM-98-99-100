using UnityEngine;

public enum TargetType
{
    None,       
    Fire,   
    Civilian, 
    Door, 
    Debris
}

public interface ITargetable
{
    Vector3 GetPosition();

    TargetType GetTargetType();

    void Interact(FirefighterBot bot);

    bool IsTaskComplete();
}
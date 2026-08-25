using UnityEngine;

public class EnvironmentReactables : MonoBehaviour
{
    [Header("Keys")]
    public GameObject redKey;
    public GameObject blueKey;
    public GameObject yellowKey;

    [Header("Doors")]
    public GameObject redDoor;
    public GameObject blueDoor;
    public GameObject yellowDoor;

    
    enum ReactableType { None, Key, Door, Launchpad, hearts, spikes}
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

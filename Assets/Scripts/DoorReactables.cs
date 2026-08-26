using UnityEngine;

public class DoorReactables : MonoBehaviour
{
    [Header("Keys")]
    public GameObject redKey;
    public GameObject blueKey;
    public GameObject yellowKey;

    //[Header("Doors")]
    //public GameObject redDoor;
    //public GameObject blueDoor;
    //public GameObject yellowDoor;

    [Header("Checks")]
    public bool hasRedKey;
    public bool hasBlueKey;
    public bool hasYellowKey;

   

    void Update()
    {
        

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("redKey"))
        {
            hasRedKey = true;
            redKey.SetActive(false);
        }

        if (other.CompareTag("blueKey"))
        {
            hasBlueKey = true;
            blueKey.SetActive(false);
        }

        if (other.CompareTag("yellowKey"))
        {
            hasYellowKey = true;
            yellowKey.SetActive(false);
        }

        if (other.CompareTag("redDoor") && hasRedKey)
        {
            Debug.Log("Red door opened!");
        }

        if (other.CompareTag("blueDoor") && hasBlueKey)
        {
            Debug.Log("Blue door opened!");
        }

        if (other.CompareTag("yellowDoor") && hasYellowKey)
        {
            Debug.Log("Yellow door opened!");
        }
    }
}

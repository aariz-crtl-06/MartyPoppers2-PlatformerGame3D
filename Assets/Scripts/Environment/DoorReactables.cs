using UnityEngine;

public class DoorReactables : MonoBehaviour
{

    [Header("Keys")]
    public GameObject redKey;
    public GameObject blueKey;
    public GameObject yellowKey;
    public Transform hubSpawn;

    //[Header("Doors")]
    //public GameObject redDoor;
    //public GameObject blueDoor;
    //public GameObject yellowDoor;

    [Header("Checks")]
    public bool hasRedKey;
    public bool hasBlueKey;
    public bool hasYellowKey;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void TeleportToHub()
    {
        if (controller != null)
        {
            controller.enabled = false;
            transform.position = hubSpawn.position;
            controller.enabled = true;
        }
        else
        {
            transform.position = hubSpawn.position;
        }
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
            TeleportToHub();
        }

        if (other.CompareTag("yellowKey"))
        {
            hasYellowKey = true;
            yellowKey.SetActive(false);
            TeleportToHub();
        }

        if (other.CompareTag("redDoor") && hasRedKey)
        {
            other.GetComponent<DoorFadeOut>().Open();
            Debug.Log("Red door opened!");
        }

        if (other.CompareTag("blueDoor") && hasBlueKey)
        {
            other.GetComponent<DoorFadeOut>().Open();
            Debug.Log("Blue door opened!");
        }

        if (other.CompareTag("yellowDoor") && hasYellowKey)
        {
            other.GetComponent<DoorFadeOut>().Open();
            Debug.Log("Yellow door opened!");
        }
    }
}

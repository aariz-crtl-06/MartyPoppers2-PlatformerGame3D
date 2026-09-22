using UnityEngine;

public class DoorReactables : MonoBehaviour
{
    public AudioSource doorOpenSound;
    public AudioSource keySound;
    public AudioSource teleportSound;

    [Header("Keys")]
    public GameObject redKey;
    public GameObject blueKey;
    public GameObject yellowKey;
    public Transform hubSpawn;

    [Header("UI")]
    public GameObject redUI;
    public GameObject blueUI;
    public GameObject yellowUI;

    [Header("Checks")]
    public bool hasRedKey;
    public bool hasBlueKey;
    public bool hasYellowKey;

    [Header("Portals")]
    public GameObject bluePortal;
    public GameObject yellowPortal;

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
            keySound.PlayOneShot(keySound.clip);
            redKey.SetActive(false);
            redUI.SetActive(true);
        }

        if (other.CompareTag("blueKey"))
        {
            hasBlueKey = true;
            keySound.PlayOneShot(keySound.clip);
            blueKey.SetActive(false);
            bluePortal.SetActive(true);
            blueUI.SetActive(true);
        }

        if (other.CompareTag("yellowKey"))
        {
            hasYellowKey = true;
            keySound.PlayOneShot(keySound.clip);
            yellowKey.SetActive(false);
            yellowPortal.SetActive(true);
            yellowUI.SetActive(true);
        }

        if (other.CompareTag("bluePortal"))
        {
            TeleportToHub();
            teleportSound.PlayOneShot(teleportSound.clip);
        }

        if (other.CompareTag("yellowPortal"))
        {
            TeleportToHub();
            teleportSound.PlayOneShot(teleportSound.clip);
        }

        if (other.CompareTag("redDoor") && hasRedKey)
        {
            other.GetComponent<DoorFadeOut>().Open();
            doorOpenSound.PlayOneShot(doorOpenSound.clip);
            redUI.SetActive(false);
        }

        if (other.CompareTag("blueDoor") && hasBlueKey)
        {
            other.GetComponent<DoorFadeOut>().Open();
            doorOpenSound.PlayOneShot(doorOpenSound.clip);
            blueUI.SetActive(false);
        }

        if (other.CompareTag("yellowDoor") && hasYellowKey)
        {
            other.GetComponent<DoorFadeOut>().Open();
            doorOpenSound.PlayOneShot(doorOpenSound.clip);
            yellowUI.SetActive(false);
        }
    }
}

using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("References")]
    public Collider respawnCollider;
    public Renderer flag;
    public Material flagOnMaterial;
    public GameObject checkpointEffect;
    public AudioSource checkpointSound;
    public GameObject confetti;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            checkpointSound.PlayOneShot(checkpointSound.clip);
            confetti.SetActive(true);
            CheckpointManager.Instance.SetCheckpoint(transform.position);
            respawnCollider.enabled = false;
            flag.material = flagOnMaterial;
            checkpointEffect.SetActive(false);
        }
    }
}
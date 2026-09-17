using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    public Vector3 CurrentRespawnPosition { get; private set; }

    [SerializeField] private PlayerCombat playerCombat;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (playerCombat == null)
            playerCombat = GetComponent<PlayerCombat>();

        playerCombat.OnPlayerDeath += playerCombat.Respawn;
    }

    public void SetCheckpoint(Vector3 position)
    {
        CurrentRespawnPosition = position;
    }
}
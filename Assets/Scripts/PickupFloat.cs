using UnityEngine;

public class PickupFloat : MonoBehaviour
{
    [Header("Spin")]
    [SerializeField] private float spinSpeed = 60f;          // degrees per second
    [SerializeField] private Vector3 spinAxis = Vector3.up;

    [Header("Bob")]
    [SerializeField] private float bobHeight = 0.25f;        // units above/below start
    [SerializeField] private float bobSpeed = 1.5f;          // higher = faster bob

    [Header("Variation")]
    [Tooltip("Offsets the bob so multiple items in a scene don't move in lockstep.")]
    [SerializeField] private bool randomizePhase = true;

    private Vector3 startLocalPos;
    private float phase;

    private void Start()
    {
        startLocalPos = transform.localPosition;
        phase = randomizePhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    private void Update()
    {
        // Spin
        transform.Rotate(spinAxis.normalized * (spinSpeed * Time.deltaTime), Space.World);

        // Bob: sine wave around the starting position
        float offset = Mathf.Sin((Time.time * bobSpeed) + phase) * bobHeight;
        transform.localPosition = startLocalPos + (Vector3.up * offset);
    }
}
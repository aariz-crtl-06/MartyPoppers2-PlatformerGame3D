using UnityEngine;

[DefaultExecutionOrder(-100)] // runs before the player's scripts each frame
public class MovingPlatform : MonoBehaviour
{
    [Header("References")]
    public Transform point1;
    public Transform point2;

    [Header("Platform Settings")]
    public float speedToPoint2 = 5f;
    public float speedToPoint1 = 3f;
    public float arriveDistance = 0.05f;

    public Vector3 DeltaMovement { get; private set; }

    bool movingToPoint2 = true;

    void Start()
    {
        transform.position = point1.position;
    }

    void Update()
    {
        Vector3 previousPosition = transform.position;

        Transform target = movingToPoint2 ? point2 : point1;
        float speed = movingToPoint2 ? speedToPoint2 : speedToPoint1;

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Measured AFTER moving, so it matches this frame's movement
        DeltaMovement = transform.position - previousPosition;

        if (Vector3.Distance(transform.position, target.position) < arriveDistance)
        {
            movingToPoint2 = !movingToPoint2;
        }
    }
}
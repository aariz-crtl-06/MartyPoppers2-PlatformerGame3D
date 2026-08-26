using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("References")]
    public GameObject point1;
    public GameObject point2;

    [Header("Checks")]
    bool atPoint1;
    bool atPoint2;

    [Header("Platform Settings")]
    Vector3 movingPlat = new Vector3 (0, 0, 0);
    public Vector3 DeltaMovement { get; private set; }

    void Start()
    {
        movingPlat = point1.transform.position;
        movingPlat = transform.position;
        atPoint1 = true;
    }

    void Update()
    {
        Vector3 previousPosition = transform.position;
        transform.position = movingPlat;
        DeltaMovement = transform.position - previousPosition;

        transform.position = movingPlat;
        

        if (atPoint1)
        {
            movingPlat = Vector3.MoveTowards(movingPlat, point2.transform.position, 5f * Time.deltaTime);

            if (Vector3.Distance(movingPlat, point2.transform.position) < 0.05f) //Check to see if reached destination
            {
                atPoint2 = true;
                atPoint1 = false;
            }
        }

        if (atPoint2)
        {
            movingPlat = Vector3.MoveTowards(movingPlat, point1.transform.position, 3f * Time.deltaTime);

            if (Vector3.Distance(movingPlat, point1.transform.position) < 0.05f) //Check to see if reached destination
            {
                atPoint1 = true;
                atPoint2 = false;
            }
        }

        
    }
}

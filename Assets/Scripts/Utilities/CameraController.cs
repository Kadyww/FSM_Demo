using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Singleton Instance 
    private static CameraController instance;

    //Camera Settings 
    [Header("Camera Settings")] [SerializeField]
    private Camera mainCamera; // Reference to the main camera

    [SerializeField] private Transform target; // Target to follow (e.g., player)
    [SerializeField] private float distance = 5f; // Initial distance from the target
    [SerializeField] private float minDistance = 2f; // Minimum zoom distance
    [SerializeField] private float maxDistance = 15f; // Maximum zoom distance
    [SerializeField] private float height = 0.6f; // Height offset above the target
    [SerializeField] private float rotationSpeed = 10f; // Camera rotation speed
    [SerializeField] private float movementSpeed = 6f; // Camera movement speed (smoothness)
    [SerializeField] private float rotMinX = 10f; // Minimum vertical rotation angle (degrees)
    [SerializeField] private float rotMaxX = 75f; // Maximum vertical rotation angle (degrees)

    //Public Static Properties for Convenient Access 
    public static Camera MainCamera => instance.mainCamera;
    public static Vector3 ForwardFlat => new(MainCamera.transform.forward.x, 0, MainCamera.transform.forward.z);

    private void Awake()
    {
        //Ensure Singleton Pattern 
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject); // Destroy duplicates

        //Initialize Camera Reference 
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Start()
    {
        //Find Player Target (Optional) 
        if (target == null)
            // Note: This assumes you have a script named "Player" on your player object.
            // You can adjust this to your specific setup.
            target = FindAnyObjectByType<Player>().transform;
    }

    private void FixedUpdate() // Physics-based updates for smooth camera movement
    {
        CameraFollow(); // Make camera follow the target
        Zoom(); // Adjust camera zoom
        Rotate(); // Rotate camera around the target
    }

    private void CameraFollow()
    {
        if (!target) return; // Safety check

        Vector3 targetPosition = target.position + Vector3.up * height; // Calculate desired position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSpeed); // Smoothly move towards it
    }

    private void Rotate()
    {
        Vector3 rotation = transform.rotation.eulerAngles; // Get current rotation

        //Rotate Horizontally (Around Y-axis) 
        rotation.y += Input.GetAxis("Mouse X") * rotationSpeed;

        //Rotate Vertically (Pitch, Around X-axis) 
        rotation.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
        rotation.x = Mathf.Clamp(rotation.x, rotMinX, rotMaxX); // Clamp vertical rotation

        transform.rotation = Quaternion.Euler(rotation); // Apply the rotation
    }

    private void Zoom()
    {
        distance -= Input.mouseScrollDelta.y; // Adjust distance based on scroll input
        distance = Mathf.Clamp(distance, minDistance, maxDistance); // Clamp to min/max distance
        mainCamera.transform.localPosition = new Vector3(0, 0, -distance); // Update camera position
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
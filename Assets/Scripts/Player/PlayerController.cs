using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static readonly int aGrounded = Animator.StringToHash("Grounded");

    private static readonly int aForwardSpeed = Animator.StringToHash("ForwardSpeed");

    [Header("References")] [SerializeField]
    private Animator animator;

    [SerializeField] private Rigidbody rb;

    [Header("Movement Settings")] [SerializeField]
    private float maxSpeed = 10f;

    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float rotationSpeed = 10f;

    private float moveSpeed;
    private Vector3 previousPosition;
    private Vector3 targetRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator.SetBool(aGrounded, true);
        previousPosition = transform.position;
    }

    private void FixedUpdate() // Physics-based movement
    {
        Move();
        Turn();
    }

    private void Move()
    {
        //Get Input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f) // Only move if there's significant input
        {
            //Align Movement to Camera
            Vector3 cameraForward = CameraController.ForwardFlat;
            Vector3 relativeInput = Quaternion.LookRotation(cameraForward) * inputDirection;

            //Smoothly Accelerate
            moveSpeed = Mathf.Lerp(moveSpeed, maxSpeed, Time.deltaTime * acceleration);

            //Calculate Movement Vector
            Vector3 movement = relativeInput * (moveSpeed * Time.deltaTime);
            rb.MovePosition(rb.position + movement);

            //Update Target Rotation
            targetRotation = Quaternion.LookRotation(relativeInput).eulerAngles;
        }

        SmoothLocomotion(); // Update animation
    }

    private void SmoothLocomotion()
    {
        //Calculate Velocity for Animation
        Vector3 velocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;
        float targetForwardAnimSpeed = velocity.magnitude;

        //Smoothly Transition Animation Speed
        float currentForwardAnimSpeed = animator.GetFloat(aForwardSpeed);
        float animSpeed = Mathf.Lerp(currentForwardAnimSpeed, targetForwardAnimSpeed, Time.deltaTime * 5f);
        animator.SetFloat(aForwardSpeed, animSpeed);
    }

    private void Turn()
    {
        //Smoothly Rotate Towards Target 
        Vector3 currentRotation = transform.eulerAngles;
        var targetRotationY = new Vector3(0, targetRotation.y, 0); // Only rotate around Y axis

        if (targetRotationY != currentRotation)
            transform.rotation = Quaternion.Slerp(Quaternion.Euler(currentRotation), Quaternion.Euler(targetRotationY), Time.deltaTime * rotationSpeed);
    }
}
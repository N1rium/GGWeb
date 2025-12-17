using UnityEngine;

// Used to handle 3rd person movement for avatars (think Super Mario 64)
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Joystick joystick;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationCurve moveCurve;
    [SerializeField] private float speedMultiplier = 2f;
    [SerializeField] private float sprintSpeedMultiplier = 5f;
    
    private CharacterController _characterController;

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
        }
        void Update()
        {
            /*var joystickInput = new Vector3(joystick.Horizontal, joystick.Vertical, 0f);
            var dir = GetInputDirection(joystickInput);
            var joystickMagnitude = joystickInput.magnitude;
            var speed = moveCurve.Evaluate(joystickMagnitude);
            
            _characterController.Move(dir.normalized * speed * speedMultiplier * Time.deltaTime);
            var isRunning = dir.magnitude > 0f;
            animator.SetFloat("Move", joystickMagnitude);*/

            if (!_characterController.isGrounded)
            {
                _characterController.Move(Vector3.down * 5f * Time.deltaTime);
            }

            /*if (!isRunning || dir == Vector3.zero) return;
            Quaternion forwardRotation = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, forwardRotation, rotationSpeed * Time.deltaTime);

            AlignToSlope();*/
        }

        public void Move(Vector2 delta)
        {
            if (!_characterController.isGrounded) return;
            var speed = moveCurve.Evaluate(delta.magnitude);
            var cameraRelative = GetInputDirection(delta);
            var isRunning = delta.magnitude > 0f;
            var magnitude = delta.magnitude;
            animator.SetFloat("Move", magnitude);
            _characterController.Move(cameraRelative.normalized * speed * speedMultiplier * Time.deltaTime);
            
            if (!isRunning || delta == Vector2.zero) return;
            Quaternion forwardRotation = Quaternion.LookRotation(cameraRelative, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, forwardRotation, rotationSpeed * Time.deltaTime);

            AlignToSlope();
        }
        
        void AlignToSlope()
        {
            // Cast a ray downwards from the object's position
            RaycastHit hit;
            var rot = transform.rotation;
            if (!Physics.Raycast(transform.position, Vector3.down, out hit, 1f)) return;
            var hitNormal = hit.normal;
            var targetRotation = Quaternion.FromToRotation(transform.up, hitNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(rot, targetRotation, 12f * Time.deltaTime);
        }
    
    private Vector3 GetInputDirection(Vector3 input)
    {
        var forward = Camera.main.transform.forward;
        var right = Camera.main.transform.right;

        forward.y = 0f;
        right.y = 0f;
        
        forward.Normalize();
        right.Normalize();

        return forward * input.y + right * input.x;
    }
}

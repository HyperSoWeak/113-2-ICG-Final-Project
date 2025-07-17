using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed;
    [SerializeField] float lookSpeed;

    Vector3 moveInput = Vector3.zero;
    bool isLooking;
    Vector2 lookDelta = Vector2.zero;

    void Awake() {
        var moveAction = InputSystem.actions.FindAction("Camera/Move");
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        var lookAction = InputSystem.actions.FindAction("Camera/Look");
        lookAction.performed += OnLook;
        lookAction.canceled += OnLook;
        var lookEnableAction = InputSystem.actions.FindAction("Camera/LookEnable");
        lookEnableAction.performed += OnLookEnable;
        lookEnableAction.canceled += OnLookEnable;
    }

    void Update() {
        Vector3 moveDirection = transform.TransformDirection(moveInput).normalized;
        transform.position += moveSpeed * Time.deltaTime * moveDirection;

        if (isLooking) {
            Vector3 angles = transform.rotation.eulerAngles;
            angles.x -= lookDelta.y * lookSpeed;
            angles.y += lookDelta.x * lookSpeed;
            transform.rotation = Quaternion.Euler(angles);
        }
    }

    void OnMove(InputAction.CallbackContext context) {
        if (context.performed) {
            moveInput = context.ReadValue<Vector3>();
        }
        else if (context.canceled) {
            moveInput = Vector3.zero;
        }
    }
    
    void OnLook(InputAction.CallbackContext context) {
        if (context.performed) {
            lookDelta = context.ReadValue<Vector2>();
        }
        else if (context.canceled) {
            lookDelta = Vector2.zero;
        }
    }

    void OnLookEnable(InputAction.CallbackContext context) {
        if (context.performed) {
            isLooking = true;
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;
        }
        else if (context.canceled) {
            isLooking = false;
            // Cursor.lockState = CursorLockMode.None;
            // Cursor.visible = true;
        }
    }
}

using UnityEngine;

public class MouseCameraController : MonoBehaviour
{
    public float movementSpeed = 10.0f;
    public float panSpeed = 80.0f; // Pan mode speed
    public float lookSpeed = 2.0f;
    public float zoomSpeed = 2.0f;
    public float shiftMultiplier = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private float roll = 0.0f;
    private bool isAltPressed = false;
    private bool isPanEnabled = false;

    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        roll = transform.eulerAngles.z;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Check if Alt key is pressed
        isAltPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        if (isAltPressed)
        {
            yaw += lookSpeed * Input.GetAxis("Mouse X");
            pitch -= lookSpeed * Input.GetAxis("Mouse Y");
            roll += lookSpeed * Input.GetAxis("Mouse ScrollWheel") * 15; // Adjust the multiplier as needed
            transform.eulerAngles = new Vector3(pitch, yaw, roll);
        }

        // Enable/disable pan with Q key
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isPanEnabled = !isPanEnabled;
        }

        // Pan with middle mouse button or when pan mode is enabled
        if (Input.GetMouseButton(2) || isPanEnabled)
        {
            float h = -Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
            float v = -Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;
            transform.Translate(h, v, 0);
        }

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 direction = transform.forward * scroll * zoomSpeed;
        transform.position += direction;

        // Unlock cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Ensure the cursor is always visible
        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }
    }
}

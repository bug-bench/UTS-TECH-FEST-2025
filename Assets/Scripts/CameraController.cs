using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    public float panSpeed = 10f;
    public Vector2 panLimitMin = new Vector2(-20f, -20f);
    public Vector2 panLimitMax = new Vector2(20f, 20f);

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 20f;

    [Header("Target")]
    public Vector3 centerTarget = Vector3.zero; // Potato center

    private Camera cam;
    private Vector3 lastMousePosition;
    private bool isPanning = false;
    private bool isZoomingWithMiddleMouse = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandlePan();
        HandleZoom();
        HandleSnap();
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse down
        {
            lastMousePosition = Input.mousePosition;
            isPanning = true;
        }

        if (Input.GetMouseButtonUp(1))
            isPanning = false;

        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * Time.deltaTime;

            transform.position += move;
            lastMousePosition = Input.mousePosition;

            // Clamp camera position
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, panLimitMin.x, panLimitMax.x);
            pos.y = Mathf.Clamp(pos.y, panLimitMin.y, panLimitMax.y);
            transform.position = pos;
        }
    }

    void HandleZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y;

        // Zoom with scroll wheel
        if (scrollDelta != 0)
        {
            cam.orthographicSize -= scrollDelta * zoomSpeed * Time.deltaTime;
        }

        // Zoom with middle mouse drag
        if (Input.GetMouseButtonDown(2)) // Middle mouse down
        {
            lastMousePosition = Input.mousePosition;
            isZoomingWithMiddleMouse = true;
        }

        if (Input.GetMouseButtonUp(2))
            isZoomingWithMiddleMouse = false;

        if (isZoomingWithMiddleMouse)
        {
            float mouseDeltaY = Input.mousePosition.y - lastMousePosition.y;
            cam.orthographicSize -= mouseDeltaY * zoomSpeed * 0.01f;
            lastMousePosition = Input.mousePosition;
        }

        // Clamp zoom
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }

    void HandleSnap()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = new Vector3(centerTarget.x, centerTarget.y, transform.position.z);
        }
    }
}
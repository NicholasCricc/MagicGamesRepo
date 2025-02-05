using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private Vector3 startPosition; // The original position of the item
    private bool isDragging = false; // Whether the item is being dragged
    private bool isOverDropZone = false; // Whether the item is over a drop zone
    private Transform dropZone; // Reference to the drop zone this item is over

    private float pressStartTime; // Time when the mouse button was pressed
    private float pressDuration; // How long the mouse button was held
    private bool hasDragged = false; // Whether the user has started dragging the item
    private float shortPressThreshold = 0.4f; // Adjusted short press threshold
    private float longPressThreshold = 0.5f; // Threshold for long press (dragging)
    private float dragDistanceThreshold = 0.05f; // Minimum distance to start dragging

    private Vector3 mouseStartPosition; // Position of the mouse when the press started
    private ItemChanger itemChanger; // Reference to the parent ItemChanger script

    void Start()
    {
        startPosition = transform.position;

        // Find the parent ItemChanger script
        itemChanger = GetComponentInParent<ItemChanger>();
    }

    void OnMouseDown()
    {
        pressStartTime = Time.time; // Record the time when the mouse button was pressed
        mouseStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Record mouse position
        hasDragged = false; // Reset dragging state
        isDragging = false; // Ensure dragging is reset
    }

    void OnMouseDrag()
    {
        Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector3.Distance(mouseStartPosition, currentMousePosition);

        // Start dragging only if both distance and time thresholds are met
        if (!hasDragged && distance > dragDistanceThreshold && Time.time - pressStartTime >= longPressThreshold)
        {
            hasDragged = true; // Dragging has started
            isDragging = true;
            Debug.Log($"{gameObject.name} is being dragged.");
        }

        // Move the item with the mouse if dragging
        if (isDragging)
        {
            transform.position = new Vector3(currentMousePosition.x, currentMousePosition.y, startPosition.z);
        }
    }

    void OnMouseUp()
    {
        // Calculate how long the mouse button was held
        pressDuration = Time.time - pressStartTime;

        if (hasDragged)
        {
            // Handle dropping logic (already working)
            if (isOverDropZone)
            {
                transform.position = dropZone.position;
                Debug.Log($"{gameObject.name} dropped on {dropZone.name}");
            }
            else
            {
                transform.position = startPosition;
                Debug.Log($"{gameObject.name} dropped outside any drop zone.");
            }
        }
        else if (pressDuration < shortPressThreshold)
        {
            // Short press logic
            Debug.Log($"{gameObject.name} short-pressed.");
            if (itemChanger != null)
            {
                itemChanger.ChangeToNextItem();
            }
        }

        // Reset states
        isDragging = false;
        hasDragged = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DropZone"))
        {
            isOverDropZone = true;
            dropZone = collision.transform;
            Debug.Log($"{gameObject.name} entered drop zone: {collision.name}");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("DropZone"))
        {
            isOverDropZone = false;
            dropZone = null;
            Debug.Log($"{gameObject.name} exited drop zone: {collision.name}");
        }
    }

    public Vector3 GetStartingPosition()
    {
        return startPosition;
    }

}

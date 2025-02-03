using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private Vector3 startPosition; // The original position of the item
    private bool isDragging = false; // Whether the item is being dragged
    private bool isOverDropZone = false; // Whether the item is over a drop zone
    private Transform dropZone; // Reference to the drop zone this item is over

    private float pressStartTime; // Time when the mouse button was pressed
    private bool hasDragged = false; // Whether the user has started dragging the item
    private float longPressThreshold = 0.15f; // Time in seconds for a long press
    private float dragDistanceThreshold = 0.05f; // Minimum distance to start dragging

    private Vector3 mouseStartPosition; // Position of the mouse when the press started
    private ItemChanger itemChanger; // Reference to the parent ItemChanger script

    void Start()
    {
        // Save the starting position
        startPosition = transform.position;

        // Find the parent ItemChanger script
        itemChanger = GetComponentInParent<ItemChanger>();
    }

    void OnMouseDown()
    {
        // Record the press start time and mouse position
        pressStartTime = Time.time;
        mouseStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        hasDragged = false; // Reset dragging state
        isDragging = false; // Ensure dragging is reset
    }

    void OnMouseDrag()
    {
        // Calculate how far the mouse has moved
        Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector3.Distance(mouseStartPosition, currentMousePosition);

        // Start dragging only if both time and distance thresholds are met
        if (!hasDragged && distance > dragDistanceThreshold && Time.time - pressStartTime >= longPressThreshold)
        {
            hasDragged = true; // Confirm that dragging has started
            isDragging = true;
            Debug.Log($"{gameObject.name} is being dragged.");

            // Notify the ItemChanger to spawn the next item
            if (itemChanger != null)
            {
                itemChanger.ShowNextItem(startPosition);
            }
        }

        // Move the item with the mouse if dragging
        if (isDragging)
        {
            transform.position = new Vector3(currentMousePosition.x, currentMousePosition.y, startPosition.z);
        }
    }

    void OnMouseUp()
    {
        // If the item was dragged, handle dropping logic
        if (hasDragged)
        {
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
        else
        {
            // If it wasn't dragged, treat it as a click
            Debug.Log($"{gameObject.name} clicked.");
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
}

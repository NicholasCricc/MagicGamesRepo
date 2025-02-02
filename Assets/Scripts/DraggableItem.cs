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

    private int originalLayer; // To store the original layer of the item

    void Start()
    {
        startPosition = transform.position;

        // Find the parent ItemChanger script
        itemChanger = GetComponentInParent<ItemChanger>();

        // Save the original layer
        originalLayer = gameObject.layer;
    }

    void OnMouseDown()
    {
        pressStartTime = Time.time;
        mouseStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        hasDragged = false;
        isDragging = false;
    }

    void OnMouseDrag()
    {
        Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector3.Distance(mouseStartPosition, currentMousePosition);

        if (!hasDragged && distance > dragDistanceThreshold && Time.time - pressStartTime >= longPressThreshold)
        {
            hasDragged = true;
            isDragging = true;
            Debug.Log($"{gameObject.name} is being dragged.");

            // Change the layer to ignore collisions during dragging
            gameObject.layer = LayerMask.NameToLayer("IgnoreCollisions");

            // Notify the ItemChanger to show the next item
            if (itemChanger != null)
            {
                itemChanger.ShowNextItem(startPosition);
            }
        }

        if (isDragging)
        {
            transform.position = new Vector3(currentMousePosition.x, currentMousePosition.y, startPosition.z);
        }
    }

    void OnMouseUp()
    {
        if (hasDragged)
        {
            if (isOverDropZone)
            {
                // Snap to drop zone
                transform.position = dropZone.position;
                Debug.Log($"{gameObject.name} dropped on {dropZone.name}");
            }
            else
            {
                // Return to starting position
                transform.position = startPosition;
                Debug.Log($"{gameObject.name} dropped outside any drop zone.");
            }
        }

        // Restore the original layer after dragging
        gameObject.layer = originalLayer;

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

using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private bool isDragging = false;
    private bool isOverDropZone = false;
    private Transform dropZone; // Stores the drop position
    private Vector3 startPosition; // Stores the item's original position

    private void Start()
    {
        // Save the original position
        startPosition = transform.position;
    }

    private void OnMouseDrag()
    {
        // Drag the item with the mouse
        if (gameObject.activeSelf) // Only drag if the item is active
        {
            isDragging = true;
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, startPosition.z);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        if (isOverDropZone)
        {
            // Snap the item to the drop zone
            transform.position = dropZone.position;
        }
        else
        {
            // Return to the original position
            transform.position = startPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DropZone"))
        {
            isOverDropZone = true;
            dropZone = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("DropZone"))
        {
            isOverDropZone = false;
            dropZone = null;
        }
    }
}

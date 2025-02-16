using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private Vector3 startPosition;
    private bool isDragging = false;
    private bool isOverDropZone = false;
    private Transform dropZone;

    private float pressStartTime;
    private float pressDuration;
    private bool hasDragged = false;
    private float shortPressThreshold = 0.4f;
    private float longPressThreshold = 0.2f;
    private float dragDistanceThreshold = 0.02f;

    private Vector3 mouseStartPosition;
    private ItemChanger itemChanger;

    void Start()
    {
        startPosition = transform.position;
        itemChanger = GetComponentInParent<ItemChanger>();
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

        if (!hasDragged && (distance > dragDistanceThreshold || Time.time - pressStartTime >= longPressThreshold))
        {
            hasDragged = true;
            isDragging = true;
            Debug.Log($"{gameObject.name} is being dragged.");
        }

        if (isDragging)
        {
            transform.position = new Vector3(currentMousePosition.x, currentMousePosition.y, startPosition.z);
        }
    }

    void OnMouseUp()
    {
        pressDuration = Time.time - pressStartTime;

        if (hasDragged)
        {
            if (isOverDropZone)
            {
                // 🔹 Snap item to the drop zone
                transform.position = dropZone.position;

                // 🔹 Disable dragging
                this.enabled = false;
                GetComponent<Collider2D>().enabled = false;

                Debug.Log($"✅ {gameObject.name} placed in {dropZone.name}");

                // 🔹 Show the next available item
                if (itemChanger != null)
                {
                    itemChanger.ShowNextAvailableItem();
                }
            }
            else
            {
                // 🔹 Return the item to its original position
                transform.position = startPosition;
                Debug.Log($"❌ {gameObject.name} dropped outside any drop zone.");
            }
        }
        else if (pressDuration < shortPressThreshold)
        {
            if (!isOverDropZone && itemChanger != null)
            {
                itemChanger.ChangeToNextItem();
            }
        }

        isDragging = false;
        hasDragged = false;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DropZone"))
        {
            isOverDropZone = true;
            dropZone = collision.transform;
            Debug.Log($"✅ {gameObject.name} entered drop zone: {collision.name}");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("DropZone"))
        {
            isOverDropZone = false;
            dropZone = null;
            Debug.Log($"❌ {gameObject.name} exited drop zone: {collision.name}");
        }
    }

    public Vector3 GetStartingPosition()
    {
        return startPosition;
    }
}

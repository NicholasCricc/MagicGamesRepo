using UnityEngine;
using System.Collections;

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
    private float longPressThreshold = 0.3f;
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
        if (isOverDropZone && dropZone != null)
        {
            DropZone dropZoneScript = dropZone.GetComponent<DropZone>();

            if (dropZoneScript == null)
            {
                Debug.LogError($"❌ DropZone script missing on {dropZone.name}");
                transform.position = startPosition;
                return;
            }

            // ✅ Call `PlaceItem()` to handle swapping
            dropZoneScript.PlaceItem(this.gameObject);
            transform.position = dropZone.position;
            Debug.Log($"✅ {gameObject.name} placed in {dropZone.name} (Drop Zone is now FULL)");

            StartCoroutine(DisableColliderAfterDelay());

            if (itemChanger != null)
            {
                itemChanger.ShowNextAvailableItem();
            }
        }
        else
        {
            Debug.Log($"❌ {gameObject.name} dropped outside any valid drop zone, returning to start.");
            transform.position = startPosition;
        }
    }
    else if (pressDuration < shortPressThreshold)
    {
        if (!isOverDropZone && itemChanger != null)
        {
            Debug.Log($"🔁 {gameObject.name} clicked - Changing to next item");
            itemChanger.ChangeToNextItem();
        }
    }

    isDragging = false;
    hasDragged = false;
}



// ✅ New function to delay setting `isOccupied`
private IEnumerator SetOccupiedAfterDelay(DropZone dropZoneScript)
{
    yield return new WaitForSeconds(0.2f); // ✅ Allows Unity time to process placement
    dropZoneScript.isOccupied = true;
}



    private IEnumerator DisableColliderAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("DropZone_Head") && gameObject.CompareTag("HatItem"))
    {
        isOverDropZone = true;
        dropZone = collision.transform;
        Debug.Log($"✅ {gameObject.name} entered {dropZone.name} (Valid Placement).");

        DropZone dropZoneScript = dropZone.GetComponent<DropZone>();
        if (dropZoneScript != null && !dropZoneScript.isOccupied)
        {
           // dropZoneScript.isOccupied = true; // ✅ Ensure the drop zone is marked as occupied immediately
        }
    }
}


        public Vector3 GetStartingPosition()
    {
        return startPosition;
    }
}

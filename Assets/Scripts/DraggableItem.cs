using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour
{
    private Vector3 dragOffset;

    private Vector3 startPosition;
    private bool isDragging = false;
    private bool isOverDropZone = false;
    private Transform dropZone;
    private Vector3 originalScale;
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

        originalScale = transform.localScale;

        Debug.Log($"🔵 {gameObject.name} Initialized - Start Position: {startPosition}");
    }

void OnMouseDown()
{
    pressStartTime = Time.time;
    mouseStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    hasDragged = false;
    isDragging = false;

    // ✅ Store offset between mouse and object pivot
    Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    dragOffset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z);

    // ✅ Raise sorting order for all visible parts (e.g. both shoes)
    SetAllChildSortingOrder(5);

    Debug.Log($"🟡 {gameObject.name} Clicked - Start Dragging.");
}


    void OnMouseDrag()
    {
        Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector3.Distance(mouseStartPosition, currentMousePosition);

        if (!hasDragged && (distance > dragDistanceThreshold || Time.time - pressStartTime >= longPressThreshold))
        {
            hasDragged = true;
            isDragging = true;
            isOverDropZone = false; // ✅ Ensure it doesn't stay true when dragging out of drop zone

            Debug.Log($"🟠 {gameObject.name} is being dragged.");

            // ✅ Temporarily disable collision with the item in the drop zone
            IgnoreDropZoneCollisions(true);
        }

if (isDragging)
{
    currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    transform.position = new Vector3(
        currentMousePosition.x + dragOffset.x,
        currentMousePosition.y + dragOffset.y,
        startPosition.z
    );

    // ✅ Bring to front on drag (safe across single/clothing/multisprite items)
    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    if (sr == null)
        sr = GetComponentInChildren<SpriteRenderer>();

    if (sr != null)
    {
        sr.sortingOrder = 5;
    }
}

    }


    void OnMouseUp()
    {
        pressDuration = Time.time - pressStartTime;
        Debug.Log($"🔴 {gameObject.name} Released - isOverDropZone: {isOverDropZone}, DropZone: {(dropZone != null ? dropZone.name : "None")}");

        if (hasDragged)
        {
            if (isOverDropZone && dropZone != null)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                if (dropZone.GetComponent<Collider2D>().OverlapPoint(mousePosition))
                {
                    DropZone dropZoneScript = dropZone.GetComponent<DropZone>();

                    if (dropZoneScript == null)
                    {
                        Debug.LogError($"❌ DropZone script missing on {dropZone.name}");
                        transform.position = startPosition;
                        return;
                    }

                    dropZoneScript.PlaceItem(this.gameObject);


                        transform.position = Vector3.zero;
                        transform.localScale = Vector3.one;

                    Debug.Log($"✅ {gameObject.name} placed in {dropZone.name}");

                    // ✅ Disable collider only — keep visible and enabled
                    StartCoroutine(DisableColliderAfterDelay());

                    if (itemChanger != null)
                    {
                        itemChanger.MarkItemAsPlaced(this.gameObject);

                        itemChanger.ResetIndex();

                        if (itemChanger.HasAvailableItems())
                        {
                            itemChanger.ShowNextAvailableItem();
                        }
                        else
                        {
                            Debug.Log("🟣 No more items to show on the rod.");
                        }

                    }
                }
                else
                {
                    Debug.Log($"❌ {gameObject.name} dropped outside valid drop zone, returning to start.");
                    transform.position = startPosition;
                    transform.localScale = originalScale;
                    EnableInteraction();
                }
            }
            else
            {
                Debug.Log($"❌ {gameObject.name} dropped outside any valid drop zone, returning to start.");
                transform.position = startPosition;
                transform.localScale = originalScale;
                EnableInteraction();
            }
        }
        else if (pressDuration < shortPressThreshold)
        {
            if (itemChanger != null)
            {
                Debug.Log($"🔁 {gameObject.name} clicked - Changing to next item");

                isOverDropZone = false;
                dropZone = null;

                itemChanger.ChangeToNextItem();
            }
        }

        SetAllChildSortingOrder(2);
        isDragging = false;
        hasDragged = false;
    }





    private void IgnoreDropZoneCollisions(bool ignore)
    {
        if (dropZone != null)
        {
            DraggableItem dropZoneItem = dropZone.GetComponentInChildren<DraggableItem>();
            if (dropZoneItem != null && dropZoneItem != this)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), dropZoneItem.GetComponent<Collider2D>(), ignore);
                Debug.Log($"🔄 {gameObject.name} Collision with {dropZoneItem.name} set to {ignore}");
            }
        }
    }


    public void EnableInteraction()
    {
        gameObject.SetActive(false);
        gameObject.SetActive(true); // ✅ Force Unity to refresh event detection

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        this.enabled = true; // ✅ Ensure script is fully active

        // ✅ Force Unity's event system to recognize the object
        //EventSystem.current.SetSelectedGameObject(null);
        //EventSystem.current.SetSelectedGameObject(gameObject);

        Debug.Log($"✅ {gameObject.name} Fully Reset and Clickable in Start Position.");
    }



    public void ResetDropZoneState()
{
    isOverDropZone = false;
    dropZone = null;
    Debug.Log($"♻️ {gameObject.name} drop zone state reset.");
}



    private IEnumerator DisableColliderAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false; // ✅ Just disable the collider
        }

        // ❌ Do NOT disable the script!
        // this.enabled = false;

        Debug.Log($"🛑 {gameObject.name} Collider Disabled, script still active.");
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("DropZone_Head") && gameObject.CompareTag("HatItem")) ||
            (collision.CompareTag("DropZone_Head_GS") && gameObject.CompareTag("HatItemGS")) ||
            (collision.CompareTag("DropZone_Body") && gameObject.CompareTag("ShirtItem")) ||
            (collision.CompareTag("DropZone_Legs") && gameObject.CompareTag("PantsItem")) ||
            (collision.CompareTag("DropZone_Feet") && gameObject.CompareTag("ShoesItem")))
        {
            isOverDropZone = true;
            dropZone = collision.transform;
            Debug.Log($"✅ {gameObject.name} entered {dropZone.name} (Valid Placement).");
        }
    }

    public Vector3 GetStartingPosition()
    {
        return startPosition;
    }

    public void CacheStartPosition()
    {
        startPosition = transform.position;
    }

    public Vector3 GetOriginalScale()
    {
        return originalScale;
    }

    private void SetAllChildSortingOrder(int order)
{
    SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
    foreach (SpriteRenderer sr in renderers)
    {
        sr.sortingOrder = order;
    }
}



}

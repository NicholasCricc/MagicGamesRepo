using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DraggableItem : MonoBehaviour
{
    private Vector3 dragOffset;
    public Transform originalParent;
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
    private bool swappedItemDuringPlacement = false;

    private Vector3 mouseStartPosition;
    public ItemChanger itemChanger;
    [SerializeField] private ItemChanger hatsChanger;   // Assign Hats rod in Inspector

    void Start()
    {
        // Cache initial state
        originalParent = transform.parent;
        startPosition = transform.position;
        originalScale = transform.localScale;
        itemChanger = GetComponentInParent<ItemChanger>();
        Debug.Log($"🔵 {name} Initialized at {startPosition}");

        // Find Hats changer if not set
        if (hatsChanger == null)
        {
            var hatsGO = GameObject.Find("Hats");
            if (hatsGO != null)
                hatsChanger = hatsGO.GetComponent<ItemChanger>();
            if (hatsChanger == null)
                Debug.LogError($"[DraggableItem] Missing hatsChanger for {name}");
        }

        // Reparent hats items under the real Hats rod
        var cloth = GetComponent<ClothingItem>();
        if (hatsChanger != null && cloth != null
            && cloth.clothingType == ClothingType.Hat
            && transform.parent != null
            && transform.parent.name.Contains("Headwear"))
        {
            transform.SetParent(hatsChanger.transform, true);
            originalParent = hatsChanger.transform;
            cloth.parentChanger = hatsChanger;
            itemChanger = hatsChanger;
            if (!hatsChanger.itemList.Contains(gameObject))
                hatsChanger.itemList.Add(gameObject);
            Debug.Log($"🔀 {name} moved to {hatsChanger.name}");
        }
    }

    void OnMouseDown()
    {
        // Start tracking click
        pressStartTime = Time.time;
        mouseStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        hasDragged = false;
        isDragging = false;

        // Compute offset for smooth drag
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dragOffset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z);

        // Bring to front
        SetAllChildSortingOrder(5);
        Debug.Log($"🟡 {name} MouseDown");
    }

    void OnMouseDrag()
    {
        // Check if drag threshold met
        Vector3 currentMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float dist = Vector3.Distance(mouseStartPosition, currentMouse);
        if (!hasDragged && (dist > dragDistanceThreshold || Time.time - pressStartTime >= longPressThreshold))
        {
            hasDragged = true;
            isDragging = true;
            isOverDropZone = false;
            Debug.Log($"🟠 {name} Dragging started");
            IgnoreDropZoneCollisions(true);
        }

        if (isDragging)
        {
            // Update position
            currentMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(
                currentMouse.x + dragOffset.x,
                currentMouse.y + dragOffset.y,
                startPosition.z
            );
            // Ensure top render order
            var sr = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 5;
        }
    }

    void OnMouseUp()
    {
        pressDuration = Time.time - pressStartTime;
        Debug.Log($"🔴 {name} MouseUp overDZ={isOverDropZone}");

        var myCloth = GetComponent<ClothingItem>();
        if (myCloth == null) return;

        Transform rodParent = transform.parent;
        bool placed = false;

        if (hasDragged)
        {
            if (isOverDropZone && dropZone != null)
            {
                // Drop into zone
                var dz = dropZone.GetComponent<DropZone>();
                if (dz == null)
                {
                    Debug.LogError($"❌ {dropZone.name} missing DropZone");
                    ReturnHome();
                    return;
                }
                GameObject returned = dz.PlaceItem(gameObject);
                placed = true;

                // Hide returned item on its rod
                if (returned != null)
                {
                    var retCloth = returned.GetComponent<ClothingItem>();
                    if (retCloth != null && retCloth.parentChanger != null)
                    {
                        retCloth.parentChanger.ClearCurrentRodItem(returned);
                        Debug.Log($"🧹 Cleared {returned.name}");
                    }
                }

                // Snap visuals
                transform.position = Vector3.zero;
                transform.localScale = Vector3.one;
                if (!gameObject.activeInHierarchy)
                {
                    gameObject.SetActive(true);
                    Debug.Log($"🔄 Reactivated {name}");
                }
                StartCoroutine(DisableColliderAfterDelay());
                Debug.Log($"✅ {name} placed in {dz.name}");

                // Cleanup siblings on same rod
                if (rodParent != null && itemChanger != null)
                {
                    var rodSet = new HashSet<GameObject>(itemChanger.itemList);
                    foreach (Transform sib in rodParent)
                    {
                        var c = sib.GetComponent<ClothingItem>();
                        if (c != null && rodSet.Contains(sib.gameObject) && !c.isPlaced)
                        {
                            sib.gameObject.SetActive(false);
                            DisableComponents(sib.gameObject);
                            Debug.Log($"🧹 Disabled {sib.name}");
                        }
                    }
                    foreach (Transform sib in rodParent)
                    {
                        var c = sib.GetComponent<ClothingItem>();
                        if (c != null && rodSet.Contains(sib.gameObject)
                            && sib.gameObject != returned && !sib.gameObject.activeSelf && !c.isPlaced)
                        {
                            sib.gameObject.SetActive(true);
                            EnableComponents(sib.gameObject);
                            itemChanger.itemList.Add(sib.gameObject);
                            itemChanger.SetCurrentIndex(itemChanger.itemList.IndexOf(sib.gameObject));
                            Debug.Log($"🎯 Activated {sib.name}");
                            break;
                        }
                    }
                }
            }
            else
            {
                // Not dropped in valid zone
                ReturnHome();
            }
        }
        else if (pressDuration < shortPressThreshold)
        {
            // Quick tap = cycle item
            if (itemChanger != null)
            {
                Debug.Log($"🔁 {name} tapped → cycle");
                itemChanger.ChangeToNextItem();
            }
        }

        // Reset state
        SetAllChildSortingOrder(2);
        isDragging = false;
        hasDragged = false;
    }

    private void ReturnHome()
    {
        // Move back to start
        transform.position = startPosition;
        transform.localScale = originalScale;
        EnableInteraction();
        Debug.Log($"❌ {name} returned home");
    }

    private void DisableComponents(GameObject go)
    {
        if (go.TryGetComponent<Collider2D>(out var c)) c.enabled = false;
        if (go.TryGetComponent<DraggableItem>(out var d)) d.enabled = false;
        if (go.TryGetComponent<SpriteRenderer>(out var sr)) sr.sortingOrder = 2;
    }

    private void EnableComponents(GameObject go)
    {
        if (go.TryGetComponent<Collider2D>(out var c)) c.enabled = true;
        if (go.TryGetComponent<DraggableItem>(out var d)) d.enabled = true;
        if (go.TryGetComponent<SpriteRenderer>(out var sr)) sr.sortingOrder = 3;
    }

    private void IgnoreDropZoneCollisions(bool ignore)
    {
        if (dropZone != null)
        {
            var other = dropZone.GetComponentInChildren<DraggableItem>();
            if (other != null && other != this)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other.GetComponent<Collider2D>(), ignore);
                Debug.Log($"🔄 Collision {ignore} between {name} and {other.name}");
            }
        }
    }

    public void EnableInteraction()
    {
        // Force Unity to re-enable events
        gameObject.SetActive(false);
        gameObject.SetActive(true);
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = true;
        enabled = true;
        Debug.Log($"✅ {name} ready for interaction");
    }

    public void ResetDropZoneState()
    {
        // Clear drop zone flags
        isOverDropZone = false;
        dropZone = null;
        Debug.Log($"♻️ {name} drop zone reset");
    }

    private IEnumerator DisableColliderAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        if (TryGetComponent<Collider2D>(out var col))
            col.enabled = false;
        Debug.Log($"🛑 {name} collider disabled");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        var zone = collision.GetComponent<DropZone>();
        var cloth = GetComponent<ClothingItem>();
        if (zone != null && cloth != null && zone.AcceptsType(cloth.clothingType))
        {
            isOverDropZone = true;
            dropZone = collision.transform;
            Debug.Log($"✅ {name} entered {dropZone.name}");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        var zone = collision.GetComponent<DropZone>();
        var cloth = GetComponent<ClothingItem>();
        if (zone != null && cloth != null && zone.AcceptsType(cloth.clothingType))
        {
            isOverDropZone = false;
            dropZone = null;
            Debug.Log($"❌ {name} left {zone.name}");
        }
    }

    public Vector3 GetStartingPosition() => startPosition;

    public void CacheStartPosition() => startPosition = transform.position;

    public Vector3 GetOriginalScale() => originalScale;

    private void SetAllChildSortingOrder(int order)
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.sortingOrder = order;
    }
}

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
    [SerializeField] private ItemChanger hatsChanger;   // drag your Hats rod here in the Inspector

    void Start()
    {
        // 1) Cache original parent & transforms
        originalParent = transform.parent;
        startPosition = transform.position;
        originalScale = transform.localScale;
        itemChanger = GetComponentInParent<ItemChanger>();
        Debug.Log($"🔵 {name} Initialized – Start Pos: {startPosition}");

        // 2) Ensure hatsChanger is set (Inspector override or find by name)
        if (hatsChanger == null)
        {
            var hatsGO = GameObject.Find("Hats");
            if (hatsGO != null)
                hatsChanger = hatsGO.GetComponent<ItemChanger>();

            if (hatsChanger == null)
                Debug.LogError($"[DraggableItem] hatsChanger not assigned and no GameObject named 'Hats' found for {name}");
        }

        // 3) Reroute any Hat-typed item under "Headwear" into the real Hats rod
        var cloth = GetComponent<ClothingItem>();
        if (hatsChanger != null
            && cloth != null
            && cloth.clothingType == ClothingType.Hat
            && transform.parent != null
            && transform.parent.name.Contains("Headwear"))
        {
            // a) Physically move it under the Hats container (preserve world position)
            transform.SetParent(hatsChanger.transform, worldPositionStays: true);

            // b) Update runtime pointers so all rod logic uses the Hats changer
            originalParent = hatsChanger.transform;
            cloth.parentChanger = hatsChanger;
            itemChanger = hatsChanger;

            // c) Add it to the Hats rod's itemList if it's not already there
            if (!hatsChanger.itemList.Contains(gameObject))
                hatsChanger.itemList.Add(gameObject);

            Debug.Log($"🔀 {name} re-parented into {hatsChanger.gameObject.name} rod at Start()");
        }

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

            swappedItemDuringPlacement = false;
            var myClothing = GetComponent<ClothingItem>();
            if (myClothing == null) return;

            Transform parent = transform.parent;
            bool wasPlacedSuccessfully = false;

            if (hasDragged)
            {
                if (isOverDropZone && dropZone != null)
                {
                    // 1) Place and get any old item
                    var dz = dropZone.GetComponent<DropZone>();
                    GameObject old = dz.PlaceItem(this.gameObject);
                    wasPlacedSuccessfully = true;

                    // 2) Hide returned accessory
                    if (old != null)
                    {
                        var oc = old.GetComponent<ClothingItem>();
                        if (oc != null && oc.parentChanger == null)
                        {
                            old.SetActive(false);
                            Debug.Log($"🛑 {old.name} swapped out and hidden");
                        }
                    }

                    // 3) Snap this one in
                    transform.position = Vector3.zero;
                    transform.localScale = Vector3.one;
                    Debug.Log($"✅ {gameObject.name} placed in {dropZone.name}");
                    StartCoroutine(DisableColliderAfterDelay());

                    // 4) Mark & remove this from rod if needed
                    if (itemChanger != null)
                    {
                        if (itemChanger.CompareCurrentRodItem(this.gameObject))
                        {
                            itemChanger.ClearCurrentRodItem(this.gameObject);
                            Debug.Log($"🧼 {gameObject.name} untracked from rod");
                        }

                        itemChanger.MarkItemAsPlaced(this.gameObject);
                        if (itemChanger.itemList.Contains(this.gameObject))
                        {
                            itemChanger.itemList.Remove(this.gameObject);
                            Debug.Log($"🗑 {gameObject.name} removed from rod list");
                        }
                    }

                    // 5) Build a quick lookup for "what is still in the rod"
                    HashSet<GameObject> rodSet = null;
                    if (itemChanger != null)
                        rodSet = new HashSet<GameObject>(itemChanger.itemList);

                    // 6) CLEANUP: disable any **rod** siblings that remain and aren't placed
                    if (wasPlacedSuccessfully && parent != null && rodSet != null)
                    {
                        foreach (Transform sib in parent)
                        {
                            var c = sib.GetComponent<ClothingItem>();
                            if (c != null
                                && rodSet.Contains(sib.gameObject)
                                && sib.gameObject.activeSelf
                                && !c.isPlaced)
                            {
                                sib.gameObject.SetActive(false);
                                if (sib.TryGetComponent<Collider2D>(out var col)) col.enabled = false;
                                if (sib.TryGetComponent<DraggableItem>(out var d)) d.enabled = false;
                                if (sib.TryGetComponent<SpriteRenderer>(out var sr)) sr.sortingOrder = 2;
                                Debug.Log($"🧹 [Rod Cleanup] Disabled {sib.name}");
                            }
                        }

                        // 7) ACTIVATE exactly one next rod item
                        foreach (Transform sib in parent)
                        {
                            var c = sib.GetComponent<ClothingItem>();
                            if (c != null
                                && rodSet.Contains(sib.gameObject)
                                && !sib.gameObject.activeSelf
                                && !c.isPlaced)
                            {
                                sib.gameObject.SetActive(true);
                                if (sib.TryGetComponent<Collider2D>(out var col)) col.enabled = true;
                                if (sib.TryGetComponent<DraggableItem>(out var d)) d.enabled = true;
                                if (sib.TryGetComponent<SpriteRenderer>(out var sr)) sr.sortingOrder = 3;
                                Debug.Log($"✅ [Next Rod] Activated {sib.name}");

                                // re-add to list (if missing) and set index
                                if (!itemChanger.itemList.Contains(sib.gameObject))
                                {
                                    itemChanger.itemList.Add(sib.gameObject);
                                    Debug.Log($"📋 Added {sib.name} to rod list");
                                }
                                int newIndex = itemChanger.itemList.IndexOf(sib.gameObject);
                                if (newIndex >= 0)
                                {
                                    itemChanger.SetCurrentIndex(newIndex);
                                    Debug.Log($"🎯 CurrentIndex = {newIndex} for {sib.name}");
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // dropped outside → return to start
                    Debug.Log($"❌ {gameObject.name} dropped outside, returning");
                    transform.position = startPosition;
                    transform.localScale = originalScale;
                    EnableInteraction();
                }
            }
            else if (pressDuration < shortPressThreshold)
            {
                // click → cycle
                if (itemChanger != null)
                {
                    Debug.Log($"🔁 {gameObject.name} clicked - cycle");
                    isOverDropZone = false;
                    dropZone = null;

                    if (itemChanger.CompareCurrentRodItem(this.gameObject))
                    {
                        itemChanger.ClearCurrentRodItem(this.gameObject);
                        itemChanger.MarkItemAsPlaced(this.gameObject);
                        if (itemChanger.itemList.Contains(this.gameObject))
                        {
                            itemChanger.itemList.Remove(this.gameObject);
                            Debug.Log($"🗑 {gameObject.name} removed after click");
                        }
                        gameObject.SetActive(false);
                        Debug.Log($"🧹 {gameObject.name} deactivated post-click");
                    }
                    itemChanger.ChangeToNextItem();
                }
            }

            // reset visuals & drag state
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
        DropZone zone = collision.GetComponent<DropZone>();
        ClothingItem clothingItem = GetComponent<ClothingItem>();

        if (zone != null && clothingItem != null)
        {
            if (zone.AcceptsType(clothingItem.clothingType))
            {
                isOverDropZone = true;
                dropZone = collision.transform;
                Debug.Log($"✅ {gameObject.name} entered {dropZone.name} (Valid Placement).");
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        DropZone zone = collision.GetComponent<DropZone>();
        ClothingItem clothingItem = GetComponent<ClothingItem>();

        if (zone != null && clothingItem != null)
        {
            if (zone.AcceptsType(clothingItem.clothingType))
            {
                // ✅ Reset when exiting a valid drop zone
                isOverDropZone = false;
                dropZone = null;
                Debug.Log($"❌ {gameObject.name} exited {zone.name} (Cancelled Placement).");
            }
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

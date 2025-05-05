using UnityEngine;
using System.Collections.Generic;

public class ItemChanger : MonoBehaviour
{
    [Header("List of Items")]
    public List<GameObject> itemList;
    private int currentIndex = -1;

    void Start()
    {
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("❌ ItemChanger: itemList is empty in Start().");
            return;
        }

        // hide everything initially
        foreach (GameObject item in itemList)
            item.SetActive(false);

        // give each ClothingItem its back-pointer
        foreach (GameObject item in itemList)
        {
            var clothing = item.GetComponent<ClothingItem>();
            if (clothing != null)
                clothing.parentChanger = this;
        }

        ChangeToNextItem();
    }

    public void ChangeToNextItem()
    {
        // 1) Guard: nothing to do if your list is empty
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("❌ ItemChanger: itemList is empty in ChangeToNextItem().");
            return;
        }

        // 2) If our currentIndex has been invalidated (e.g. you removed an item), reset it
        if (currentIndex < 0 || currentIndex >= itemList.Count)
            currentIndex = -1;

        // 3) Deactivate the old “current” item
        if (currentIndex >= 0)
        {
            var old = itemList[currentIndex];
            if (old != null)
            {
                old.SetActive(false);

                if (old.TryGetComponent<Collider2D>(out var c))
                    c.enabled = false;

                // ← FIXED generic syntax here
                if (old.TryGetComponent<DraggableItem>(out var d))
                    d.enabled = false;

                Debug.Log($"🛑 Deactivated {old.name}");
            }
        }


        // 4) Advance *once*, wrapping around
        currentIndex = (currentIndex + 1) % itemList.Count;
        Debug.Log($"➡️ Cycling to index: {currentIndex}");

        // 5) Skip *only* placed items, up to N tries
        int safety = 0;
        while (itemList[currentIndex].GetComponent<ClothingItem>().isPlaced
               && safety < itemList.Count)
        {
            currentIndex = (currentIndex + 1) % itemList.Count;
            safety++;
        }

        // 6) If all were placed, bail out
        if (safety >= itemList.Count)
        {
            Debug.LogWarning("⚠️ All items placed — nothing to show.");
            return;
        }

        // 7) Finally, show the chosen item
        ActivateItem(currentIndex);
    }


    private void ActivateItem(int index)
    {
        if (index < 0 || index >= itemList.Count)
        {
            Debug.LogError("❌ Invalid index in ActivateItem().");
            return;
        }

        var item = itemList[index];
        if (item == null)
        {
            Debug.LogError("❌ Null GameObject at index in ActivateItem().");
            return;
        }

        item.SetActive(true);
        if (item.TryGetComponent<Collider2D>(out var c)) c.enabled = true;
        if (item.TryGetComponent<DraggableItem>(out var d)) d.enabled = true;
        d?.CacheStartPosition();

        Debug.Log($"✅ Activated {item.name}");
    }

    /// <summary>Reset the cycle so the next ChangeToNextItem() starts from “before zero.”</summary>
    public void ResetIndex()
    {
        currentIndex = -1;
        Debug.Log("🔄 currentIndex reset to -1");
    }

    /// <summary>Jump the cycle pointer so this GameObject becomes “current.”</summary>
    public void SetCurrentRodItem(GameObject item)
    {
        int idx = itemList.IndexOf(item);
        if (idx >= 0)
        {
            currentIndex = idx;
            Debug.Log($"🎯 currentIndex set to {idx} ({item.name})");
        }
        else
        {
            Debug.LogWarning($"⚠️ Tried to SetCurrentRodItem() for {item.name}, but it wasn't in the list.");
        }
    }

    /// <summary>Disable any other items in this rod whose type conflicts with the given one.</summary>
    public void DeactivateConflictingClothingTypes(ClothingType type)
    {
        foreach (var go in itemList)
        {
            var cloth = go.GetComponent<ClothingItem>();
            if (cloth != null && cloth.clothingType != type && !cloth.isPlaced)
            {
                go.SetActive(false);
                if (go.TryGetComponent<Collider2D>(out var c)) c.enabled = false;
                if (go.TryGetComponent<DraggableItem>(out var d)) d.enabled = false;
                Debug.Log($"🚫 Deactivated conflicting {go.name} ({cloth.clothingType})");
            }
        }
    }

    /// <summary>Return true if this GameObject is the one currently “up” in the cycle.</summary>
    public bool CompareCurrentRodItem(GameObject item)
    {
        if (currentIndex >= 0 && currentIndex < itemList.Count)
            return itemList[currentIndex] == item;
        return false;
    }

    /// <summary>If this was the current rod item, clear it so the next cycle won’t skip.</summary>
    public void ClearCurrentRodItem(GameObject item)
    {
        if (CompareCurrentRodItem(item))
        {
            currentIndex = -1;
            Debug.Log($"🧹 Cleared {item.name} from current rod slot");
        }
    }

    /// <summary>Mark an item placed (so ChangeToNextItem skips it).</summary>
    public void MarkItemAsPlaced(GameObject item)
    {
        var c = item.GetComponent<ClothingItem>();
        if (c != null)
        {
            c.isPlaced = true;
            Debug.Log($"✅ Marked {item.name} as placed");
        }
    }

    /// <summary>Directly set the cycle pointer (use sparingly).</summary>
    public void SetCurrentIndex(int index)
    {
        if (index >= 0 && index < itemList.Count)
        {
            currentIndex = index;
            Debug.Log($"🎯 SetCurrentIndex() to {index}");
        }
    }


    private void DeactivateItem(GameObject go)
    {
        go.SetActive(false);
        if (go.TryGetComponent(out Collider2D col)) col.enabled = false;
        if (go.TryGetComponent(out DraggableItem d)) d.enabled = false;
        Debug.Log($"🛑 Deactivated {go.name}");
    }
}
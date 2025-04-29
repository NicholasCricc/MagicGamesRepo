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

        foreach (GameObject item in itemList)
        {
            item.SetActive(false);
        }

        foreach (GameObject item in itemList)
        {
            ClothingItem clothing = item.GetComponent<ClothingItem>();
            if (clothing != null)
            {
                clothing.parentChanger = this; // 🧹 Assign myself as their manager
            }
        }

        ChangeToNextItem();
    }


    public void ChangeToNextItem()
    {
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError("❌ ItemChanger: itemList is empty in ChangeToNextItem().");
            return;
        }

        if (itemList.Count == 1)
        {
            Debug.Log("ℹ️ Only one item available, activating it.");
            ActivateItem(0);
            return;
        }

        // Deactivate current item
        if (currentIndex >= 0 && currentIndex < itemList.Count)
        {
            GameObject currentItem = itemList[currentIndex];
            if (currentItem != null)
            {
                currentItem.SetActive(false);
                if (currentItem.TryGetComponent(out Collider2D col)) col.enabled = false;
                if (currentItem.TryGetComponent(out DraggableItem drag)) drag.enabled = false;
                Debug.Log($"🛑 Deactivated {currentItem.name}");
            }
        }

        // Move to next
        currentIndex = (currentIndex + 1) % itemList.Count;
        Debug.Log($"➡️ Cycling to index: {currentIndex}");

        // 🚀 Skip placed items
        int safetyCounter = 0;
        while (
            itemList[currentIndex].GetComponent<ClothingItem>().isPlaced
            && safetyCounter < itemList.Count
        )
        {
            Debug.Log($"⏭️ Skipping placed item: {itemList[currentIndex].name}");
            currentIndex = (currentIndex + 1) % itemList.Count;
            safetyCounter++;
        }

        // 🔥 FIRST check if all items are placed
        if (safetyCounter >= itemList.Count)
        {
            Debug.LogWarning("⚠️ No unplaced items available to cycle.");
            return;
        }

        // ✅ NOW it's safe to get the next active item
        ClothingItem candidateClothing = itemList[currentIndex].GetComponent<ClothingItem>();
        if (candidateClothing == null)
        {
            Debug.LogError($"❌ No ClothingItem found on {itemList[currentIndex].name}.");
            return;
        }
        ClothingType activeType = candidateClothing.clothingType;
        Debug.Log($"🔵 ActiveType determined: {activeType}");

        // 🔥 Finally, activate it
        ActivateItem(currentIndex);

    }

    private void ActivateItem(int index)
    {
        if (index < 0 || index >= itemList.Count)
        {
            Debug.LogError("❌ Invalid index in ActivateItem().");
            return;
        }

        GameObject item = itemList[index];
        if (item == null)
        {
            Debug.LogError("❌ Null GameObject at index in ActivateItem().");
            return;
        }

        item.SetActive(true);
        if (item.TryGetComponent(out Collider2D col)) col.enabled = true;
        if (item.TryGetComponent(out DraggableItem drag)) drag.enabled = true;
        if (drag != null)
        {
            drag.CacheStartPosition();
        }

        Debug.Log($"✅ Activated {item.name}");
    }

    // 🛠️ Placeholder method
    public void ResetIndex()
    {
        Debug.Log("🛠️ ResetIndex() placeholder called.");
    }

    // 🛠️ Placeholder method
    public void SetCurrentRodItem(GameObject item)
    {
        Debug.Log($"🛠️ SetCurrentRodItem() placeholder called for {item.name}.");
    }

    // 🛠️ Placeholder method
    public void DeactivateConflictingClothingTypes(ClothingType type)
    {
        Debug.Log($"🛠️ DeactivateConflictingClothingTypes() placeholder called for {type}.");
    }

    // 🛠️ Placeholder method
    public bool CompareCurrentRodItem(GameObject item)
    {
        Debug.Log($"🛠️ CompareCurrentRodItem() placeholder called for {item.name}.");
        return false;
    }

    // 🛠️ Placeholder method
    public void ClearCurrentRodItem(GameObject item)
    {
        Debug.Log($"🛠️ ClearCurrentRodItem() placeholder called for {item.name}.");
    }

    // 🛠️ Placeholder method
    public void MarkItemAsPlaced(GameObject item)
    {
        if (item == null) return;

        ClothingItem clothing = item.GetComponent<ClothingItem>();
        if (clothing != null)
        {
            clothing.isPlaced = true;
            Debug.Log($"✅ Marked {item.name} as placed.");
        }
    }


    // 🛠️ Placeholder method
    public void SetCurrentIndex(int index)
    {
        Debug.Log($"🛠️ SetCurrentIndex() placeholder called with index {index}.");
    }

}

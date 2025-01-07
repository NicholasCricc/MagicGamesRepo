using UnityEngine;

public class ItemClick : MonoBehaviour
{
    private ItemChanger itemChanger;

    private void Start()
    {
        // Find the ItemChanger in the scene
        itemChanger = FindObjectOfType<ItemChanger>();
    }

    private void OnMouseDown()
    {
        // Trigger the ChangeToNextItem method
        if (itemChanger != null)
        {
            itemChanger.ChangeToNextItem();
        }
    }
}

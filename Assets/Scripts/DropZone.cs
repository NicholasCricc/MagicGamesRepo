using UnityEngine;

public class DropZone : MonoBehaviour
{
    public bool isOccupied = false; // ✅ Tracks if an item is inside

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            isOccupied = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            isOccupied = false;
        }
    }
}

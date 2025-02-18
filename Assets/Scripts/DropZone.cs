using UnityEngine;

public class DropZone : MonoBehaviour
{
    public bool isOccupied = false; // ✅ Tracks if an item is placed inside

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            // ✅ Ensure another item is NOT inside before resetting isOccupied
            Collider2D[] itemsInDropZone = Physics2D.OverlapCircleAll(transform.position, 0.1f);
            bool isStillOccupied = true;

            foreach (var collider in itemsInDropZone)
            {
                if (collider.CompareTag("Item") && collider.gameObject != collision.gameObject)
                {
                    isStillOccupied = true;
                    break;
                }
            }

            if (!isStillOccupied)
            {
                isOccupied = false;
                Debug.Log($"♻️ {collision.name} exited {gameObject.name}, setting isOccupied = false");
            }
        }
    }
}

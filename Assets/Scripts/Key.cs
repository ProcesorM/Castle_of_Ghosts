using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hráč sebral klíč.");
            RoomGenerator roomGenerator = FindObjectOfType<RoomGenerator>();
            roomGenerator.CollectKey();
            Destroy(gameObject);
        }
    }
}